"""Orchestrates steps 1-6 for the ShilpoHub JSON datasets, scoped to one Qdrant
collection. Called by the api routes in main.py - holds the wiring, not pipeline logic
(that stays in rag/step0N_*.py, reused as-is)."""

from pathlib import Path
from typing import Dict, Optional

from rag.step01_load_json import load_json
from rag.step02_normalize_json import normalize_json
from rag.step03_clean_data import clean_data
from rag.step04_chunking import chunk_documents
from rag.step06_vector_store import store_chunks


def ingest_dataset(
    *, collection_name: str, embeddings, sparse=None,
    data_dir: Optional[Path] = None, recreate: bool = True,
) -> Dict[str, int]:
    """Load, normalize, clean, chunk, embed and store the three JSON datasets.

    `recreate=True` (the default) rebuilds the collection, so records removed from the
    JSON files disappear from the index. `recreate=False` upserts: chunks have stable
    ids, so unchanged records are overwritten in place, never duplicated.
    """
    datasets = load_json(data_dir)                                        # 1
    docs = clean_data(normalize_json(datasets))                           # 2-3
    chunks = chunk_documents(docs)                                        # 4  (aspect chunks, <200 tokens)
    store_chunks(chunks, embeddings, collection_name, recreate=recreate, sparse=sparse)  # 5 (embedders passed in) + 6

    return {"documents": len(docs), "chunks": len(chunks)}
