"""STEP 6 - Store in Qdrant.

Every chunk becomes one point with TWO vectors and its metadata:

    vectors  "dense"  384 dims, cosine   (all-MiniLM-L6-v2)
             "bm25"   sparse, IDF-weighted (fastembed Qdrant/bm25)  -> hybrid search
    payload  {"page_content": <chunk text>, "metadata": {doc_id, doc_type, aspect, source_file,
              craft_key, name_en, name_bn, category_norm, districts[], divisions[],
              confidences[], source_ids[], risk_level, is_unesco, is_gi, ...}}

The metadata sits under the `metadata.` key, as in the earlier langchain-qdrant layout, so
filters are written `metadata.craft_key`, `metadata.districts`, ...  Payload indexes are
created on every field the query router filters on. Point ids are derived from each chunk's
`chunk_key`, so ingesting the same data again overwrites instead of duplicating.

Multi-tenant: every function takes an explicit ``collection_name``. The CLI scripts pass
``config.COLLECTION_NAME`` ("shilpohub").

Two ways to run Qdrant:
  server : docker run -p 6333:6333 -v qdrant_storage:/qdrant/storage qdrant/qdrant
  local  : set QDRANT_PATH=./qdrant_data in .env (CLI-only fallback - a running
           service holds one open handle, so the HTTP API needs server mode)
"""

import shutil
import time
import uuid
from pathlib import Path
from typing import List, Optional

from langchain_core.documents import Document
from qdrant_client import QdrantClient
from qdrant_client.http.models import (
    Distance, FieldCondition, Filter, MatchValue, Modifier, PayloadSchemaType, PointStruct,
    SparseVector, SparseVectorParams, VectorParams,
)

import config
from rag import index_meta

DENSE_VECTOR = "dense"
SPARSE_VECTOR = "bm25"

# Payload indexes: the fields the query router filters on (step 9). A `*_key` etc. is
# matched exactly (KEYWORD); arrays (districts, divisions) match if any element equals.
PAYLOAD_INDEXES = {
    "craft_key": PayloadSchemaType.KEYWORD,
    "districts": PayloadSchemaType.KEYWORD,
    "divisions": PayloadSchemaType.KEYWORD,
    "category_norm": PayloadSchemaType.KEYWORD,
    "risk_level": PayloadSchemaType.KEYWORD,
    "is_unesco": PayloadSchemaType.BOOL,
    "is_gi": PayloadSchemaType.BOOL,
    "doc_type": PayloadSchemaType.KEYWORD,
    "aspect": PayloadSchemaType.KEYWORD,
    # not in the original list, but step 9 filters on them (REF-* lookups, per-file retrieval)
    "doc_id": PayloadSchemaType.KEYWORD,
    "source_file": PayloadSchemaType.KEYWORD,
}


_CLIENT: Optional[QdrantClient] = None


def get_client() -> QdrantClient:
    """The process-wide Qdrant client. Embedded (on-disk) Qdrant allows ONE open handle per
    folder, so everything in the process - the query context, health checks, collection
    lookups - must share it; a second QdrantClient(path=...) raises "already accessed by
    another instance". A server client shares just as well."""
    global _CLIENT
    if _CLIENT is None:
        _CLIENT = (QdrantClient(path=config.QDRANT_PATH) if config.QDRANT_PATH
                   else QdrantClient(url=config.QDRANT_URL, api_key=config.QDRANT_API_KEY))
    return _CLIENT


def release_client() -> None:
    """Close and forget the shared client (frees the on-disk lock)."""
    global _CLIENT
    if _CLIENT is not None:
        try:
            _CLIENT.close()
        except Exception:  # noqa: BLE001 - already closed / interpreter shutting down
            pass
        _CLIENT = None


def _reset_local_storage(collection_name: str) -> None:
    """Delete the on-disk folder for this collection (local/CLI mode only).

    In embedded/on-disk mode `delete_collection()` clears the registry entry but
    leaves `collection/<name>/storage.sqlite` on disk, and the next
    `create_collection()` re-attaches to that stale file. Removing the folder before any
    client is open - so no file lock is held - is the only reliable way to make
    `--recreate` truly recreate. Server mode does not have this problem.
    """
    if not config.QDRANT_PATH:
        return
    release_client()                    # an open handle would keep the folder locked on Windows
    folder = Path(config.QDRANT_PATH) / "collection" / collection_name
    if folder.exists():
        shutil.rmtree(folder, ignore_errors=True)
        print(f"    wiped local storage for '{collection_name}'")


def _existing_vector_size(client: QdrantClient, name: str) -> Optional[int]:
    """Size of the dense vector of an existing collection, or None if it can't be read."""
    try:
        vectors = client.get_collection(name).config.params.vectors
        if isinstance(vectors, dict):
            return vectors[DENSE_VECTOR].size if DENSE_VECTOR in vectors else None
        return getattr(vectors, "size", None)     # an old, unnamed-vector collection
    except Exception:  # noqa: BLE001 - treat as "unknown"
        return None


def ensure_collection(client: QdrantClient, dimension: int, collection_name: str, recreate: bool = False) -> None:
    exists = client.collection_exists(collection_name)

    if exists and recreate:
        client.delete_collection(collection_name)
        exists = False
        print(f"    dropped old collection '{collection_name}'")

    if exists:
        current_size = _existing_vector_size(client, collection_name)
        if current_size is None:
            raise RuntimeError(
                f"Collection '{collection_name}' exists but has no '{DENSE_VECTOR}' vector - it was built "
                "by an older version of this pipeline. Re-ingest with --recreate."
            )
        if current_size != dimension:
            raise RuntimeError(
                f"Qdrant dimension mismatch: collection '{collection_name}' stores "
                f"{current_size}-dim vectors, but the current embedding model "
                f"produces {dimension}-dim vectors. Re-ingest with recreate=true."
            )

    if not exists:
        client.create_collection(
            collection_name=collection_name,
            vectors_config={DENSE_VECTOR: VectorParams(size=dimension, distance=Distance.COSINE)},
            sparse_vectors_config={SPARSE_VECTOR: SparseVectorParams(modifier=Modifier.IDF)},
        )
        if config.QDRANT_PATH:
            indexes = "payload indexes skipped (embedded Qdrant ignores them; a Qdrant server gets them)"
        else:
            for field, schema in PAYLOAD_INDEXES.items():
                client.create_payload_index(collection_name, field_name=f"metadata.{field}", field_schema=schema)
            indexes = f"{len(PAYLOAD_INDEXES)} payload indexes"
        print(f"    created collection '{collection_name}' (dense {dimension}-dim cosine + bm25 sparse, {indexes})")


def _point_id(chunk: Document, collection_name: str) -> str:
    """Stable id from the chunk's identity (set in step 4), so re-ingesting overwrites the
    same point instead of storing the chunk twice."""
    key = chunk.metadata.get("chunk_key")
    return str(uuid.uuid5(uuid.NAMESPACE_URL, f"{collection_name}/{key}")) if key else str(uuid.uuid4())


def store_chunks(
    chunks: List[Document], embeddings, collection_name: str, recreate: bool = False, sparse=None,
) -> int:
    """Embed (dense + BM25) and store `chunks`. `recreate=False` upserts: chunks with the same
    `chunk_key` are overwritten in place. Returns the number of points in the collection."""
    from rag.step05_embedding import get_sparse_embeddings

    # Refuse to append onto a collection built with a different embedding model.
    index_meta.check_before_ingest(collection_name, recreate)

    # Must run before the client opens a file lock on the storage folder (CLI/local mode).
    if recreate:
        _reset_local_storage(collection_name)

    sparse = sparse or get_sparse_embeddings()
    client = get_client()
    dimension = detect_or_config_dim(embeddings)
    ensure_collection(client, dimension, collection_name, recreate=recreate)

    batch = config.EMBED_BATCH_SIZE
    for i, start in enumerate(range(0, len(chunks), batch)):
        window = chunks[start:start + batch]
        if i > 0 and config.EMBED_SLEEP:
            time.sleep(config.EMBED_SLEEP)

        texts = [chunk.page_content for chunk in window]
        dense = embeddings.embed_documents(texts)
        bm25 = list(sparse.passage_embed(texts))
        client.upsert(collection_name, points=[
            PointStruct(
                id=_point_id(chunk, collection_name),
                vector={DENSE_VECTOR: vec,
                        SPARSE_VECTOR: SparseVector(indices=sp.indices.tolist(), values=sp.values.tolist())},
                payload={"page_content": chunk.page_content, "metadata": chunk.metadata},
            )
            for chunk, vec, sp in zip(window, dense, bm25)
        ])
        print(f"    stored {min(start + batch, len(chunks))}/{len(chunks)} chunks", end="\r")

    count = client.count(collection_name, exact=True).count
    print(f"\n[6] Qdrant     : collection '{collection_name}' now holds {count} points")

    # Remember what built this collection so a later model change is caught.
    index_meta.save(collection_name, dimension)
    return count


def delete_document(collection_name: str, document_id: str) -> None:
    """Remove every chunk of one document: a doc_id (e.g. "craftDetails.json:jamdani",
    "REF-SITES") or a whole dataset file name (e.g. "craft.json")."""
    client = get_client()
    if not client.collection_exists(collection_name):
        return
    client.delete(
        collection_name=collection_name,
        points_selector=Filter(should=[
            FieldCondition(key="metadata.doc_id", match=MatchValue(value=document_id)),
            FieldCondition(key="metadata.source_file", match=MatchValue(value=document_id)),
        ]),
    )


def delete_collection(collection_name: str) -> None:
    """Remove an entire collection (so a deleted Knowledge Base leaves no vectors behind)."""
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
