"""ShilpoHub Product Search service (HTTP) - separate from the Heritage RAG (main.py, port 8000).

    uvicorn product_main:app --port 8001          # run from this folder

    GET  /health                                   -> {status, points, sync}
    POST /api/product-search/candidates            header X-Internal-Key
         {query, limit?}                           -> {analysis, mode, pass, candidates: [{productId, score, semantic}]}

It answers "which products might match this request?" and nothing else: the backend loads those products from
PostgreSQL, re-applies price / stock / visibility, ranks and returns them. This service never touches the database.
The same process also runs the sync worker (backend feed -> Gemini embeddings -> Qdrant), because embedded Qdrant
allows a single process to hold the store; with a Qdrant server (PRODUCT_QDRANT_URL) `ingest_products.py --watch`
can run separately and PRODUCT_SYNC_ENABLED=false turns the built-in worker off.
"""

import hmac
import os
import sys
import threading
import time
from contextlib import asynccontextmanager
from typing import Optional

from fastapi import FastAPI, Header, HTTPException
from pydantic import BaseModel, Field

import config

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

from products import settings
from products.analysis import analyze
from products.api_client import ProductIndexApi
from products.embeddings import ProductEmbedder
from products.retrieve import ProductRetriever
from products.suggest import model_name, suggest
from products.store import ProductVectorStore, make_client
from products.sync import ProductSyncWorker
from products.vocab import load_vocabulary
from rag.step05_embedding import get_sparse_embeddings   # existing BM25 sparse embedder, reused read-only

SYNC_ENABLED = os.getenv("PRODUCT_SYNC_ENABLED", "true").strip().lower() != "false"
SYNC_INTERVAL = float(os.getenv("PRODUCT_SYNC_INTERVAL", "30"))


class CandidatesRequest(BaseModel):
    query: str = Field(..., min_length=1, max_length=300)
    limit: int = Field(40, ge=1, le=100)


class SuggestRequest(BaseModel):
    name: str = Field(..., min_length=1, max_length=300)
    description: str = Field("", max_length=6000)
    story: Optional[str] = Field(None, max_length=6000)
    category: Optional[str] = Field(None, max_length=200)
    district: Optional[str] = Field(None, max_length=200)


class _State:
    retriever: Optional[ProductRetriever] = None
    store: Optional[ProductVectorStore] = None
    sync_thread: Optional[threading.Thread] = None
    stop = threading.Event()
    last_sync: dict = {}


state = _State()


def _sync_loop(worker: ProductSyncWorker) -> None:
    """Keep the index fresh. A backend outage or a Gemini quota error must never kill the thread."""
    while not state.stop.is_set():
        try:
            stats = worker.run_once()
            state.last_sync = {"at": time.time(), **stats}
            busy = stats["fetched"] > 0 and stats["pending_total"] > stats["fetched"]
        except Exception as exc:  # noqa: BLE001
            print(f"[sync] failed: {type(exc).__name__}: {str(exc)[:160]}")
            busy = False
        if not busy:
            state.stop.wait(SYNC_INTERVAL)


@asynccontextmanager
async def lifespan(app: FastAPI):
    embedder = ProductEmbedder()
    store = ProductVectorStore(make_client(), embedder.dim)
    store.ensure_collection()
    sparse = get_sparse_embeddings()
    state.store = store
    state.retriever = ProductRetriever(store, embedder, sparse)
    if SYNC_ENABLED and settings.API_KEY:
        state.sync_thread = threading.Thread(target=_sync_loop, args=(ProductSyncWorker(ProductIndexApi(), store, embedder, sparse),), daemon=True)
        state.sync_thread.start()
    yield
    state.stop.set()


app = FastAPI(title="ShilpoHub Product Search", lifespan=lifespan)


def _check_key(supplied: Optional[str]) -> None:
    if not settings.API_KEY:
        raise HTTPException(status_code=503, detail="ProductIndex__ApiKey is not configured")
    if not supplied or not hmac.compare_digest(supplied.encode(), settings.API_KEY.encode()):
        raise HTTPException(status_code=401, detail="invalid key")


@app.get("/health")
def health():
    return {
        "status": "ok" if state.retriever else "starting",
        "collection": settings.COLLECTION,
        "points": state.store.count() if state.store else 0,
        "sync": {"enabled": SYNC_ENABLED, "last": state.last_sync},
    }


@app.post("/api/product-search/candidates")
def candidates(body: CandidatesRequest, x_internal_key: Optional[str] = Header(default=None)):
    _check_key(x_internal_key)
    if state.retriever is None:
        raise HTTPException(status_code=503, detail="product search is starting")

    vocab = load_vocabulary()
    analysis = analyze(body.query, vocab)
    if not analysis["semantic"] or not analysis["english_query"]:
        return {"analysis": analysis, "mode": "filter_only", "pass": "none", "candidates": []}

    try:
        found = state.retriever.candidates(analysis, body.limit)
    except Exception as exc:  # noqa: BLE001 - embedding quota etc.: let the backend fall back to SQL
        raise HTTPException(status_code=502, detail=f"retrieval failed: {type(exc).__name__}") from exc
    return {"analysis": analysis, "mode": "semantic", **found}


@app.post("/api/product-search/suggest-attributes")
def suggest_attributes(body: SuggestRequest, x_internal_key: Optional[str] = Header(default=None)):
    """AI SUGGESTION for a product's search attributes. The backend stores it as pending; the producer must confirm it."""
    _check_key(x_internal_key)
    try:
        suggestion = suggest(body.model_dump(), load_vocabulary())
    except Exception as exc:  # noqa: BLE001 - quota, malformed JSON...: the producer can still fill the form by hand
        raise HTTPException(status_code=502, detail=f"suggestion failed: {type(exc).__name__}") from exc
    return {"suggested": suggestion, "model": model_name()}
