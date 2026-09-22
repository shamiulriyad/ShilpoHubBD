"""ShilpoHub Heritage RAG service over HTTP - one Qdrant collection per Knowledge Base.

    uvicorn main:app --port 8000          # run from this folder

    GET    /health                                          -> {status, ready, qdrant}
    POST   /api/kb/{collection}/ingest?recreate=
                                                              -> {documents, chunks}
      (re-indexes craft.json, craftDetails.json and GEO.json from DATA_DIR - no body)
    POST   /api/kb/{collection}/query   {question, similarity_threshold?}
                                                              -> {answer, question_type, sources: [{source_file, doc_id, source_ids}]}
    DELETE /api/kb/{collection}/documents/{document_id}      -> 204   (document_id = a dataset file name)
    DELETE /api/kb/{collection}                              -> 204

Every route is scoped to one `collection` path segment, so a collection is never shared
between Knowledge Bases by accident.

This file only exposes api/ingest.py, api/chat.py and the step0N_* pipeline modules over
HTTP - it holds no RAG logic of its own. Needs **Qdrant in server mode** (`QDRANT_URL`,
no `QDRANT_PATH`): embedded on-disk Qdrant allows only one open handle, so a running
service cannot also write to it - in that mode ingest/query return 501 and you use the
`python ingest.py` / `ask.py` CLIs instead.
"""

import re
from contextlib import asynccontextmanager
from typing import List, Optional

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

import config
from api.chat import KnowledgeBaseNotIndexedError, answer_question
from api.health import qdrant_is_healthy
from api.ingest import ingest_dataset
from rag.pipeline import build_context
from rag.step05_embedding import get_embeddings, get_sparse_embeddings
from rag.step06_vector_store import delete_collection, delete_document, get_client

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


@app.get("/health")
def health() -> dict:
    return {"status": "ok", "ready": bool(_state.get("ready")), "qdrant": "healthy" if qdrant_is_healthy() else "down"}


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


@app.delete("/api/kb/{collection}/documents/{document_id}", status_code=204)
def delete_document_route(collection: str, document_id: str) -> None:
    collection = _validate_collection(collection)
    delete_document(collection, document_id)


@app.delete("/api/kb/{collection}", status_code=204)
def delete_collection_route(collection: str) -> None:
    collection = _validate_collection(collection)
    delete_collection(collection)
