"""STEP 6 - Store in Qdrant.

Multi-tenant: every function takes an explicit ``collection_name`` (one Qdrant
collection per Knowledge Base - see api/ingest.py / api/chat.py). The CLI
scripts (ingest.py / ask.py) pass ``config.COLLECTION_NAME`` so single-document
local development keeps working exactly as before.

Two ways to run Qdrant:
  server : docker run -p 6333:6333 -v qdrant_storage:/qdrant/storage qdrant/qdrant
  local  : set QDRANT_PATH=./qdrant_data in .env (CLI-only fallback - a running
           service holds one open handle, so the HTTP API always needs server mode)
"""

import shutil
import time
from pathlib import Path
from typing import List, Optional

from langchain_core.documents import Document
from langchain_qdrant import QdrantVectorStore
from qdrant_client import QdrantClient
from qdrant_client.http.models import Distance, FieldCondition, Filter, MatchValue, VectorParams

import config
from rag import index_meta


def get_client() -> QdrantClient:
    if config.QDRANT_PATH:
        return QdrantClient(path=config.QDRANT_PATH)
    return QdrantClient(url=config.QDRANT_URL, api_key=config.QDRANT_API_KEY)


def _reset_local_storage(collection_name: str) -> None:
    """Delete the on-disk folder for this collection (local/CLI mode only).

    In embedded/on-disk mode `delete_collection()` clears the registry entry but
    leaves `collection/<name>/storage.sqlite` on disk, and the next
    `create_collection()` re-attaches to that stale file. Re-ingesting then
    *appends* instead of replacing, and a dimension change breaks inserts
    outright. Removing the folder before any client is open - so no file lock is
    held - is the only reliable way to make `--recreate` truly recreate. Server
    mode does not have this problem; there `delete_collection()` is enough.
    """
    if not config.QDRANT_PATH:
        return
    folder = Path(config.QDRANT_PATH) / "collection" / collection_name
    if folder.exists():
        shutil.rmtree(folder, ignore_errors=True)
        print(f"    wiped local storage for '{collection_name}'")


def _existing_vector_size(client: QdrantClient, name: str):
    """Vector size of an existing collection, or None if it can't be read."""
    try:
        vectors = client.get_collection(name).config.params.vectors
        if hasattr(vectors, "size"):
            return vectors.size
        if isinstance(vectors, dict) and vectors:
            return next(iter(vectors.values())).size
    except Exception:  # noqa: BLE001 - treat as "unknown"
        return None
    return None


def ensure_collection(client: QdrantClient, dimension: int, collection_name: str, recreate: bool = False) -> None:
    exists = client.collection_exists(collection_name)

    if exists and recreate:
        client.delete_collection(collection_name)
        exists = False
        print(f"    dropped old collection '{collection_name}'")

    if exists:
        current_size = _existing_vector_size(client, collection_name)
        if current_size is not None and current_size != dimension:
            raise RuntimeError(
                f"Qdrant dimension mismatch: collection '{collection_name}' stores "
                f"{current_size}-dim vectors, but the current embedding model "
                f"produces {dimension}-dim vectors. Re-ingest with recreate=true."
            )

    if not exists:
        client.create_collection(
            collection_name=collection_name,
            vectors_config=VectorParams(size=dimension, distance=Distance.COSINE),
        )
        print(f"    created collection '{collection_name}' (dim={dimension}, cosine)")


def get_vector_store(embeddings, collection_name: str, client: Optional[QdrantClient] = None) -> QdrantVectorStore:
    return QdrantVectorStore(
        client=client or get_client(),
        collection_name=collection_name,
        embedding=embeddings,
    )


def store_chunks(
    chunks: List[Document], embeddings, collection_name: str, recreate: bool = False,
) -> QdrantVectorStore:
    """Embed and store `chunks` in `collection_name`. `recreate=False` (the default for
    the multi-document Knowledge Base flow) appends onto the existing collection - each
    chunk must already carry a `document_id` in its metadata (see api/ingest.py) so a
    later `delete_document` can remove just that document's vectors."""
    # Refuse to append onto a collection built with a different embedding model.
    index_meta.check_before_ingest(collection_name, recreate)

    # Must run before the client opens a file lock on the storage folder (CLI/local mode).
    if recreate:
        _reset_local_storage(collection_name)

    client = get_client()
    dimension = detect_or_config_dim(embeddings)
    ensure_collection(client, dimension, collection_name, recreate=recreate)
    store = get_vector_store(embeddings, collection_name, client)

    # Gemini caps a single embed call at 100 texts, so send in batches.
    # On the free tier it also caps ~100 texts/minute, so pause between batches.
    batch = config.EMBED_BATCH_SIZE
    for i, start in enumerate(range(0, len(chunks), batch)):
        window = chunks[start:start + batch]
        if i > 0 and config.EMBED_SLEEP:
            time.sleep(config.EMBED_SLEEP)
        store.add_documents(window)
        print(f"    stored {min(start + batch, len(chunks))}/{len(chunks)} chunks", end="\r")

    count = client.count(collection_name, exact=True).count
    print(f"\n[6] Qdrant     : collection '{collection_name}' now holds {count} vectors")

    # Remember what built this collection so a later model change is caught.
    index_meta.save(collection_name, dimension)
    return store


def delete_document(collection_name: str, document_id: str) -> None:
    """Remove every chunk belonging to one document (spec: reprocess/delete a document
    without touching the rest of the Knowledge Base)."""
    client = get_client()
    if not client.collection_exists(collection_name):
        return
    client.delete(
        collection_name=collection_name,
        points_selector=Filter(must=[FieldCondition(key="metadata.document_id", match=MatchValue(value=document_id))]),
    )


def delete_collection(collection_name: str) -> None:
    """Remove an entire Knowledge Base's collection (spec: deleting a Knowledge Base
    must not leave its vectors behind)."""
    client = get_client()
    if client.collection_exists(collection_name):
        client.delete_collection(collection_name)
    if config.QDRANT_PATH:
        _reset_local_storage(collection_name)


def collection_exists(collection_name: str) -> bool:
    return get_client().collection_exists(collection_name)


def detect_or_config_dim(embeddings) -> int:
    from rag.step05_embedding import detect_dimension
    return detect_dimension(embeddings)
