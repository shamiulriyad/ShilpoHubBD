"""Orchestrates the Travel Planner ingestion: load (reused step 1) -> normalize -> clean
(new, tourism-specific) -> chunk (reused step 4) -> embed + store (reused step 6), into a
Qdrant collection that is never shared with the craft/heritage collection.
"""

from pathlib import Path
from typing import Dict, Optional

import config
from rag.step01_load_json import load_json
from rag.step04_chunking import chunk_documents
from rag.step06_vector_store import store_chunks
from rag.travel.clean import clean_travel_data
from rag.travel.normalize import TOURISM_FILE, normalize_travel_json
from rag.travel.normalize_facilities import FACILITIES_FILE, facilities_available, normalize_facilities_json
from rag.travel.normalize_heritage import HERITAGE_FILE, normalize_heritage_json

DEFAULT_COLLECTION = "travel-planner"


def ingest_travel_dataset(
    *, collection_name: str = DEFAULT_COLLECTION, embeddings, sparse=None,
    data_dir: Optional[Path] = None, recreate: bool = True,
) -> Dict[str, int]:
    """Load, normalize, clean, chunk, embed and store the Travel Planner tourism dataset.

    Mirrors `api/ingest.py`'s `ingest_dataset` shape for the craft datasets, but points at a
    different file and a different (isolated) Qdrant collection by default."""
    base = data_dir or config.DATA_DIR
    files = (TOURISM_FILE, HERITAGE_FILE) + ((FACILITIES_FILE,) if facilities_available(base) else ())
    datasets = load_json(base, filenames=files)                                  # 1 (reused)
    docs = clean_travel_data(                                                           # 2-3 (new)
        normalize_travel_json(datasets) + normalize_heritage_json(datasets)
        + normalize_facilities_json(datasets))
    chunks = chunk_documents(docs)                                               # 4 (reused)
    store_chunks(chunks, embeddings, collection_name, recreate=recreate, sparse=sparse)  # 5+6 (reused)

    return {"documents": len(docs), "chunks": len(chunks)}
