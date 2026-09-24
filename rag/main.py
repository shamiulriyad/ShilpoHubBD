"""ShilpoHub Heritage RAG service over HTTP - one Qdrant collection per Knowledge Base.

    uvicorn main:app --port 8000          # run from this folder

    GET    /health                                          -> {status, ready, qdrant}
    POST   /api/kb/{collection}/ingest?recreate=
                                                              -> {documents, chunks}
      (re-indexes craft.json, craftDetails.json and GEO.json from DATA_DIR - no body)
    POST   /api/kb/{collection}/query   {question, similarity_threshold?}
                                                              -> {answer, question_type, sources: [{source_file, doc_id, source_ids}]}
    POST   /api/kb/{collection}/retrieve {query, district?, placeType?, interests?, topK?}
                                                              -> {results: [{text, sourceFile, docId, districts, placeType, themes, score}]}
      (plain retrieval, no Gemini classification/generation - built for the isolated
      "travel-planner" Knowledge Base; the caller does its own generation. See rag/travel/.)
    DELETE /api/kb/{collection}/documents/{document_id}      -> 204   (document_id = a dataset file name)
    DELETE /api/kb/{collection}                              -> 204

Every route is scoped to one `collection` path segment, so a collection is never shared
between Knowledge Bases by accident.

This file only exposes api/ingest.py, api/chat.py, rag/travel/retrieve.py and the step0N_*
pipeline modules over HTTP - it holds no RAG logic of its own. Needs **Qdrant in server
mode** (`QDRANT_URL`, no `QDRANT_PATH`) for /ingest specifically: embedded on-disk Qdrant
allows only one open handle, so a running service cannot also write to it, and /ingest
returns 501 in that mode - use the `python ingest.py` / `python ingest_travel.py` CLIs
instead. /query and /retrieve only read, so they work in either mode.
"""

import re
import sys
from contextlib import asynccontextmanager
from typing import List, Optional

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

# Windows defaults stdout/stderr to the console codepage (cp1252) once they're not an
# interactive TTY (e.g. redirected to a log file or piped by a process manager), which
# raises UnicodeEncodeError the moment a Bangla question hits one of the pipeline's
# print() progress lines. Force UTF-8 so non-Latin questions never crash the request.
for _stream in (sys.stdout, sys.stderr):
    if hasattr(_stream, "reconfigure"):
        _stream.reconfigure(encoding="utf-8", errors="replace")

import config
from api.chat import KnowledgeBaseNotIndexedError, answer_question
from api.health import qdrant_is_healthy
from api.ingest import ingest_dataset
from rag.pipeline import build_context
from rag.step05_embedding import get_embeddings, get_sparse_embeddings
from rag.step06_vector_store import delete_collection, delete_document, get_client
from rag.travel.retrieve import retrieve_travel_context

# The embedding models and LLMs are process-wide (one embedding model per service,
# configured via .env) and built once. The Qdrant collection is per-request, never cached.
_state: dict = {}


def _ensure_ready() -> None:
    if _state.get("ready"):
        return
    _state["embeddings"] = get_embeddings()                    # step 5 (dense)
    _state["sparse"] = get_sparse_embeddings()                 # step 5 (BM25)
    _state["ctx"] = build_context(get_client(), _state["embeddings"], _state["sparse"])   # + LLMs (steps 7, 10)
    _state["ready"] = True


@asynccontextmanager
async def lifespan(_app: FastAPI):
    try:
        _ensure_ready()
    except Exception as exc:  # noqa: BLE001 - reported back on first real request
        print(f"[startup] not ready yet: {exc}")
    yield
    _state.clear()


app = FastAPI(title="ShilpoHub Heritage RAG service", lifespan=lifespan)

# Guards against path traversal in any filesystem-backed operation (embedded Qdrant, meta files).
_COLLECTION_RE = re.compile(r"^[A-Za-z0-9_-]{1,128}$")


def _validate_collection(collection: str) -> str:
    if not _COLLECTION_RE.fullmatch(collection):
        raise HTTPException(status_code=400, detail="Invalid Knowledge Base collection name.")
    return collection


class QueryIn(BaseModel):
    question: str
    # Overrides MIN_RELEVANCE_SCORE for this request. (top_k is no longer accepted: every
    # question type has its own top_k, see step 9.)
    similarity_threshold: Optional[float] = None


class SourceOut(BaseModel):
    source_file: str
    doc_id: str
    source_ids: List[str] = []


class AnswerOut(BaseModel):
    answer: str
    question_type: str
    sources: List[SourceOut]


class IngestOut(BaseModel):
    documents: int
    chunks: int


class RetrieveIn(BaseModel):
    query: str
    district: Optional[str] = None
    placeType: Optional[str] = None
    interests: Optional[List[str]] = None
    topK: Optional[int] = None


class RetrievedSnippet(BaseModel):
    text: str
    sourceFile: Optional[str] = None
    docId: Optional[str] = None
    districts: List[str] = []
    placeType: Optional[str] = None
    themes: List[str] = []
    score: float


class RetrieveOut(BaseModel):
    results: List[RetrievedSnippet]


@app.get("/health")
def health() -> dict:
    key = config.gemini_key_status()
    return {"status": "ok", "ready": bool(_state.get("ready")), "qdrant": "healthy" if qdrant_is_healthy() else "down",
            "gemini_key_loaded": key["loaded"], "gemini_key_fingerprint": key["fingerprint"]}


def _require_server_mode() -> None:
    if config.QDRANT_PATH:
        raise HTTPException(
            status_code=501,
            detail=(
                "This endpoint needs Qdrant in server mode. Clear QDRANT_PATH and set "
                "QDRANT_URL in .env (see `docker compose up`), or use the CLI fallback: "
                "`python ingest.py --recreate`."
            ),
        )


@app.post("/api/kb/{collection}/ingest", response_model=IngestOut)
def ingest(collection: str, recreate: bool = True) -> IngestOut:
    """Steps 1-6 over the three JSON datasets in DATA_DIR, into `collection`."""
    _require_server_mode()
    collection = _validate_collection(collection)

    try:
        _ensure_ready()
        result = ingest_dataset(
            collection_name=collection,
            embeddings=_state["embeddings"],
            sparse=_state["sparse"],
            recreate=recreate,
        )
        return IngestOut(**result)
    except (FileNotFoundError, ValueError) as exc:   # missing / malformed dataset file
        raise HTTPException(status_code=422, detail=str(exc)) from exc
    except Exception as exc:  # noqa: BLE001
        raise HTTPException(status_code=500, detail=f"Ingestion failed: {exc}") from exc


@app.post("/api/kb/{collection}/query", response_model=AnswerOut)
def query(collection: str, body: QueryIn) -> AnswerOut:
    collection = _validate_collection(collection)
    question = body.question.strip()
    if not question:
        raise HTTPException(status_code=400, detail="question is required")

    try:
        _ensure_ready()
    except Exception as exc:  # noqa: BLE001
        raise HTTPException(status_code=503, detail=f"RAG service not ready: {exc}") from exc

    try:
        result = answer_question(
            collection_name=collection,
            question=question,
            similarity_threshold=body.similarity_threshold,
            ctx=_state["ctx"],
        )
    except KnowledgeBaseNotIndexedError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc

    return AnswerOut(**result)


@app.post("/api/kb/{collection}/retrieve", response_model=RetrieveOut)
def retrieve(collection: str, body: RetrieveIn) -> RetrieveOut:
    """Plain retrieval, no question-type classification, no Gemini answer generation -- the
    Travel Planner's caller (the .NET backend) generates its own answer from this context.
    Never touches the craft-only QUESTION_TYPES/ROUTES/prompts, so `/query` is unaffected."""
    collection = _validate_collection(collection)
    query_text = body.query.strip()
    if not query_text:
        raise HTTPException(status_code=400, detail="query is required")

    try:
        _ensure_ready()
    except Exception as exc:  # noqa: BLE001
        raise HTTPException(status_code=503, detail=f"RAG service not ready: {exc}") from exc

    if not get_client().collection_exists(collection):
        raise HTTPException(status_code=503, detail=f"Knowledge Base '{collection}' has not been indexed yet.")

    results = retrieve_travel_context(
        ctx=_state["ctx"], collection_name=collection, query_text=query_text,
        district=body.district, place_type=body.placeType, interests=body.interests,
        top_k=body.topK or 6,
    )
    return RetrieveOut(results=[RetrievedSnippet(**r) for r in results])


@app.delete("/api/kb/{collection}/documents/{document_id}", status_code=204)
def delete_document_route(collection: str, document_id: str) -> None:
    collection = _validate_collection(collection)
    delete_document(collection, document_id)


@app.delete("/api/kb/{collection}", status_code=204)
def delete_collection_route(collection: str) -> None:
    collection = _validate_collection(collection)
    delete_collection(collection)
