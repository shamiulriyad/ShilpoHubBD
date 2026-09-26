"""The product vector collection (`shilpohub_products`): dense + BM25 sparse vectors, flat filterable payload.

Qdrant is used ONLY for semantic retrieval. Price, stock and rating stored in the payload are snapshots for
pre-filtering; PostgreSQL stays the source of truth and is re-read for every answer.
"""

import functools
import threading
import uuid
from typing import Any, Dict, Iterable, List, Optional

from qdrant_client import QdrantClient
from qdrant_client.http.models import (
    Distance, FieldCondition, Filter, MatchValue, Modifier, PayloadSchemaType, PointStruct,
    SparseVector, SparseVectorParams, VectorParams,
)

from products import settings

DENSE = "dense"
SPARSE = "bm25"

# Fields the search layer filters on. (Embedded Qdrant ignores payload indexes; a server gets all of them.)
PAYLOAD_INDEXES = {
    "product_id": PayloadSchemaType.KEYWORD,
    "chunk_kind": PayloadSchemaType.KEYWORD,
    "category_slug": PayloadSchemaType.KEYWORD,
    "craft_key": PayloadSchemaType.KEYWORD,
    "product_type": PayloadSchemaType.KEYWORD,
    "materials": PayloadSchemaType.KEYWORD,
    "tags": PayloadSchemaType.KEYWORD,
    "occasions": PayloadSchemaType.KEYWORD,
    "colors": PayloadSchemaType.KEYWORD,
    "district": PayloadSchemaType.KEYWORD,
    "division": PayloadSchemaType.KEYWORD,
    "producer_id": PayloadSchemaType.KEYWORD,
    "is_public": PayloadSchemaType.BOOL,
    "in_stock": PayloadSchemaType.BOOL,
    "is_handmade_verified": PayloadSchemaType.BOOL,
    "is_gi": PayloadSchemaType.BOOL,
    "effective_price": PayloadSchemaType.FLOAT,
    "rating": PayloadSchemaType.FLOAT,
    "bayesian_rating": PayloadSchemaType.FLOAT,
    "review_count": PayloadSchemaType.INTEGER,
}

# Product category slug -> the craft_key the heritage knowledge base uses for the same craft (rag/craft_keys.py
# vocabulary), so a product filter and a heritage answer can talk about the same craft. Read-only mapping;
# categories without a clear counterpart are left out rather than guessed.
CATEGORY_CRAFT_KEYS = {
    "jamdani-weaving": "jamdani",
    "nakshi-kantha": "nakshi_kantha",
    "pottery-terracotta": "pottery",
    "jute-craft": "jute_crafts",
    "bamboo-cane": "bamboo_crafts",
    "handloom-textiles": "cotton_handloom_weaving",
    "shital-pati": "shital_pati",
    "brass-bell-metal": "brass_bell_metal",
    "wood-carving": "wooden_furniture_crafts",
    "rickshaw-art": "rickshaw_art",
    "folk-painting-alpana": "alpana",
    "jewellery-metalwork": "handmade_jewelry",
    "leather-craft": "leather_handicrafts",
    "clay-dolls": "clay_toys",
    "dhakai-muslin": "muslin_revival",
    "rajshahi-silk": "rajshahi_silk",
}


def _locked(method):
    """Embedded Qdrant is not thread-safe, and the service searches and syncs from different threads."""
    @functools.wraps(method)
    def wrapper(self, *args, **kwargs):
        with self.lock:
            return method(self, *args, **kwargs)
    return wrapper


def make_client() -> QdrantClient:
    if settings.QDRANT_PATH:
        return QdrantClient(path=settings.QDRANT_PATH)
    return QdrantClient(url=settings.QDRANT_URL, api_key=settings.QDRANT_API_KEY)


class ProductVectorStore:
    def __init__(self, client: QdrantClient, dim: int, collection: str = settings.COLLECTION, embedded: Optional[bool] = None):
        self.client = client
        self.dim = dim
        self.collection = settings.assert_own_collection(collection)
        self.embedded = bool(settings.QDRANT_PATH) if embedded is None else embedded
        self.lock = threading.RLock()

    # ---- collection ----
    @_locked
    def ensure_collection(self, recreate: bool = False) -> None:
        exists = self.client.collection_exists(self.collection)
        if exists and recreate:
            self.client.delete_collection(self.collection)
            exists = False
        if exists:
            vectors = self.client.get_collection(self.collection).config.params.vectors
            size = vectors[DENSE].size if isinstance(vectors, dict) and DENSE in vectors else None
            if size != self.dim:
                raise RuntimeError(
                    f"'{self.collection}' stores {size}-dim vectors but the product embedder makes {self.dim}-dim. "
                    "Re-run with --recreate."
                )
            return
        self.client.create_collection(
            collection_name=self.collection,
            vectors_config={DENSE: VectorParams(size=self.dim, distance=Distance.COSINE)},
            sparse_vectors_config={SPARSE: SparseVectorParams(modifier=Modifier.IDF)},
        )
        if not self.embedded:
            for field, schema in PAYLOAD_INDEXES.items():
                self.client.create_payload_index(self.collection, field_name=field, field_schema=schema)

    @_locked
    def count(self) -> int:
        return self.client.count(self.collection, exact=True).count if self.client.collection_exists(self.collection) else 0

    # ---- points ----
    def point_id(self, product_id: str, kind: str) -> str:
        return str(uuid.uuid5(uuid.NAMESPACE_URL, f"{self.collection}/{product_id}/{kind}"))

    @staticmethod
    def _product_filter(product_id: str) -> Filter:
        return Filter(must=[FieldCondition(key="product_id", match=MatchValue(value=str(product_id)))])

    @_locked
    def has_points(self, product_id: str) -> bool:
        return self.client.count(self.collection, count_filter=self._product_filter(product_id), exact=True).count > 0

    def payload_for(self, item_payload: Dict[str, Any], kind: str, text: str, text_hash: str, model: str) -> Dict[str, Any]:
        payload = self.snapshot(item_payload)
        payload.update({"chunk_kind": kind, "page_content": text, "text_hash": text_hash, "embedding_model": model})
        return payload

    @staticmethod
    def snapshot(item_payload: Dict[str, Any]) -> Dict[str, Any]:
        """The feed payload plus the craft_key derived from the category."""
        payload = {k: (str(v) if k.endswith("_id") and v is not None else v) for k, v in item_payload.items()}
        payload["craft_key"] = CATEGORY_CRAFT_KEYS.get(str(payload.get("category_slug") or ""))
        return payload

    @_locked
    def upsert_product(self, product_id: str, chunks: List[Dict[str, str]], dense: List[List[float]], sparse: Iterable[Any],
                       payload: Dict[str, Any], text_hash: str, model: str) -> None:
        kinds = [c["kind"] for c in chunks]
        points = []
        for chunk, vec, sp in zip(chunks, dense, sparse):
            points.append(PointStruct(
                id=self.point_id(product_id, chunk["kind"]),
                vector={DENSE: vec, SPARSE: SparseVector(indices=list(map(int, sp.indices)), values=list(map(float, sp.values)))},
                payload=self.payload_for(payload, chunk["kind"], chunk["text"], text_hash, model),
            ))
        self.client.upsert(self.collection, points=points)
        self._remove_stale_kinds(product_id, kinds)

    def _remove_stale_kinds(self, product_id: str, keep_kinds: List[str]) -> None:
        for kind in settings.CHUNK_KINDS:
            if kind not in keep_kinds:
                self.client.delete(self.collection, points_selector=[self.point_id(product_id, kind)])

    @_locked
    def refresh_payload(self, product_id: str, item_payload: Dict[str, Any]) -> None:
        """Price / stock / rating / visibility changed but the text did not: no re-embedding."""
        snapshot = self.snapshot(item_payload)
        self.client.set_payload(self.collection, payload=snapshot, points=self._product_filter(product_id))

    @_locked
    def delete_product(self, product_id: str) -> None:
        self.client.delete(self.collection, points_selector=self._product_filter(product_id))

    @_locked
    def search(self, using: str, query: Any, query_filter: Optional[Filter], limit: int):
        """Nearest chunks for a dense or sparse query vector, restricted by a payload filter."""
        return self.client.query_points(
            self.collection, query=query, using=using, limit=limit, with_payload=["product_id", "chunk_kind"],
            query_filter=query_filter).points
