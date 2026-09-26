"""Settings for the Product Search index.

This package is deliberately independent of the two existing knowledge bases:

    shilpohub        heritage / craft knowledge RAG        (untouched)
    travel-planner   tourism / trip planner RAG            (untouched)
    shilpohub_products   THIS package: product vectors     (own collection, own Qdrant location)

Only the Gemini API key is reused from the existing config (root .env `Gemini__ApiKey`). The embedding
MODEL is set here, not taken from EMBEDDING_PROVIDER, because the other collections are built with a local
model and must not change.
"""

import os
from pathlib import Path

import config  # existing rag config: loads the .env files and exposes GOOGLE_API_KEY

# Collections that belong to other systems. The product index refuses to touch them.
PROTECTED_COLLECTIONS = frozenset({config.COLLECTION_NAME, "shilpohub", "travel-planner"})

COLLECTION = os.getenv("PRODUCT_COLLECTION", "shilpohub_products").strip()

# gemini-embedding-001 handles Bangla; 768 dims (Matryoshka-truncated) keeps the index small.
EMBEDDING_MODEL = os.getenv("PRODUCT_EMBEDDING_MODEL", "gemini-embedding-001").strip()
EMBEDDING_DIM = int(os.getenv("PRODUCT_EMBEDDING_DIM", "768"))

# A Qdrant server if PRODUCT_QDRANT_URL is set, otherwise an embedded store in its OWN folder (never qdrant_data,
# which the heritage / travel service holds open).
QDRANT_URL = os.getenv("PRODUCT_QDRANT_URL") or None
QDRANT_API_KEY = os.getenv("PRODUCT_QDRANT_API_KEY") or None
QDRANT_PATH = None if QDRANT_URL else str(Path(os.getenv("PRODUCT_QDRANT_PATH") or (config.BASE_DIR / "qdrant_products")))

# The backend feed. The worker talks to the backend over HTTP with a shared secret; it never sees database credentials.
API_URL = os.getenv("PRODUCT_INDEX_API_URL", "http://localhost:5065/api").rstrip("/")
API_KEY = os.getenv("ProductIndex__ApiKey") or None

BATCH_SIZE = int(os.getenv("PRODUCT_SYNC_BATCH", "25"))
CHUNK_KINDS = ("overview", "story")


def assert_own_collection(name: str) -> str:
    if name in PROTECTED_COLLECTIONS:
        raise RuntimeError(
            f"'{name}' belongs to another RAG system. The product index must use its own collection "
            f"(default 'shilpohub_products')."
        )
    return name
