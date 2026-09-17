"""Multi-tenant RAG service over HTTP - one Qdrant collection per Knowledge Base.

    uvicorn main:app --port 8000          # run from this folder

`backend/Integrations/PythonRag/RagService.cs` calls:

    GET    /health                                          -> {status, ready, qdrant}
    POST   /api/kb/{collection}/ingest?document_id=&filename=&chunk_size=&chunk_overlap=
                                                              -> {pages, chunks}
      (raw PDF bytes as the request body)
    POST   /api/kb/{collection}/query   {question, top_k?, similarity_threshold?}
                                                              -> {answer, sources: [...]}
    DELETE /api/kb/{collection}/documents/{document_id}      -> 204
    DELETE /api/kb/{collection}                              -> 204

Every route is scoped to one `collection` path segment (spec section 13: every
retrieval query MUST filter by the current Knowledge Base) - the .NET backend
derives it deterministically from the Knowledge Base id (`kb_<guid>`), so a
collection is never guessable/reusable across Knowledge Bases by accident.

This file only exposes api/ingest.py, api/chat.py and the step0N_* pipeline
modules over HTTP - it holds no RAG logic of its own. Needs **Qdrant in server
mode** (`QDRANT_URL`, no `QDRANT_PATH`): embedded on-disk Qdrant allows only one
open handle, so a running service cannot also write to it - in that mode
ingest/query return 501 and you use the `python ingest.py`/`ask.py` CLIs instead
(single global collection, for quick local experimentation only).
"""

import os
import re
import tempfile
from contextlib import asynccontextmanager
from pathlib import Path
from typing import List, Optional

from fastapi import FastAPI, HTTPException, Request
from pydantic import BaseModel

import config
from api.chat import KnowledgeBaseNotIndexedError, answer_question
from api.health import qdrant_is_healthy
from api.ingest import ScannedPdfError, ingest_document
from rag.step05_embedding import get_embeddings
from rag.step06_vector_store import delete_collection, delete_document
from rag.step10_generate import get_llm

# The embedding model and LLM are process-wide (spec: one embedding model per
# service, configured via .env) and built once. The Qdrant collection is
# per-request (per Knowledge Base), never cached here.
_state: dict = {}


def _ensure_ready() -> None:
    if _state.get("ready"):
        return
    _state["embeddings"] = get_embeddings()   # step 5
    _state["llm"] = get_llm()                 # step 10
    _state["ready"] = True


@asynccontextmanager
async def lifespan(_app: FastAPI):
    try:
        _ensure_ready()
    except Exception as exc:  # noqa: BLE001 - reported back on first real request
        print(f"[startup] not ready yet: {exc}")
    yield
    _state.clear()


app = FastAPI(title="RAG Starter - Python RAG service", lifespan=lifespan)

# Matches the collection names the .NET backend derives from a Knowledge Base id
# (`kb_<32 hex chars>`) plus the legacy single-tenant default - guards against
# path traversal in any filesystem-backed operation (embedded Qdrant, meta files).
_COLLECTION_RE = re.compile(r"^[A-Za-z0-9_-]{1,128}$")


def _validate_collection(collection: str) -> str:
    if not _COLLECTION_RE.fullmatch(collection):
        raise HTTPException(status_code=400, detail="Invalid Knowledge Base collection name.")
    return collection


class QueryIn(BaseModel):
    question: str
    top_k: Optional[int] = None
    similarity_threshold: Optional[float] = None


class SourceOut(BaseModel):
    document_id: Optional[str] = None
    chunk_id: Optional[str] = None
    page: Optional[int] = None
    source: str
    score: float
    excerpt: str = ""


class AnswerOut(BaseModel):
    answer: str
    sources: List[SourceOut]


class IngestOut(BaseModel):
    pages: int
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
                "QDRANT_URL in .env (see `docker compose up`), or use the single-document "
                "CLI fallback: `python ingest.py --pdf <file> --recreate`."
            ),
        )


_MAX_UPLOAD_BYTES = config.MAX_UPLOAD_MB * 1024 * 1024


async def _stream_body_to_file(request: Request, dest: Path) -> int:
    """Write the request body to `dest` in chunks, enforcing the size cap. Never holds
    the whole PDF in memory. Raises 413 as soon as the limit is crossed, or 400 if the
    bytes are not a PDF (no %PDF- header)."""
    size = 0
    checked_header = False
    with dest.open("wb") as out:
        async for chunk in request.stream():
            if not checked_header and chunk:
                checked_header = True
                if not chunk.startswith(b"%PDF-"):
                    raise HTTPException(status_code=400, detail="That file is not a valid PDF.")
            size += len(chunk)
            if size > _MAX_UPLOAD_BYTES:
                raise HTTPException(
                    status_code=413,
                    detail=f"PDF exceeds the configured upload limit of "
                           f"{config.MAX_UPLOAD_MB} MB (set MAX_UPLOAD_MB to change it).",
                )
            out.write(chunk)
    return size


@app.post("/api/kb/{collection}/ingest", response_model=IngestOut)
async def ingest(
    collection: str, request: Request,
    document_id: str, filename: str = "upload.pdf",
    chunk_size: int = 0, chunk_overlap: int = 0,
) -> IngestOut:
    """Steps 1-6 for one document, appended onto `collection` (one Qdrant collection
    per Knowledge Base). The PDF is the raw request body (streamed to a temp file)."""
    _require_server_mode()
    collection = _validate_collection(collection)

    safe_name = Path(filename).name or "upload.pdf"
    if not safe_name.lower().endswith(".pdf"):
        raise HTTPException(status_code=400, detail="Only .pdf files are supported.")

    tmp_dir = Path(tempfile.gettempdir()) / "rag-starter-ingest"
    tmp_dir.mkdir(parents=True, exist_ok=True)
    fd, tmp_name = tempfile.mkstemp(suffix=".pdf", dir=tmp_dir)
    os.close(fd)
    tmp_path = Path(tmp_name)

    try:
        written = await _stream_body_to_file(request, tmp_path)
        if written == 0:
            raise HTTPException(status_code=400, detail="Empty request body - no PDF received.")

        _ensure_ready()
        result = ingest_document(
            collection_name=collection,
            document_id=document_id,
            pdf_path=tmp_path,
            filename=safe_name,
            chunk_size=chunk_size or config.CHUNK_SIZE,
            chunk_overlap=chunk_overlap or config.CHUNK_OVERLAP,
            embeddings=_state["embeddings"],
        )
        return IngestOut(**result)
    except ScannedPdfError as exc:
        raise HTTPException(status_code=422, detail=str(exc)) from exc
    except HTTPException:
        raise
    except Exception as exc:  # noqa: BLE001
        raise HTTPException(status_code=500, detail=f"Ingestion failed: {exc}") from exc
    finally:
        tmp_path.unlink(missing_ok=True)


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
            top_k=body.top_k or config.TOP_K,
            similarity_threshold=body.similarity_threshold,
            embeddings=_state["embeddings"],
            llm=_state["llm"],
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
