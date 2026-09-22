"""Ingestion pipeline - steps 1 to 6.

    python ingest.py                          # datasets from DATA_DIR (default: data/)
    python ingest.py --data-dir data
    python ingest.py --recreate               # drop the collection first

Re-running without --recreate is safe: chunks get stable ids, so the same dataset
overwrites itself instead of being stored twice. Each chunk is stored with a dense and a
BM25 sparse vector (hybrid search). Use --recreate after changing the
embedding model or removing records from the JSON files.
"""

import argparse

import config
from rag.step01_load_json import load_json
from rag.step02_normalize_json import normalize_json
from rag.step03_clean_data import clean_data
from rag.step04_chunking import chunk_documents
from rag.step05_embedding import get_embeddings
from rag.step06_vector_store import store_chunks


def main() -> None:
    config.ensure_utf8_console()

    parser = argparse.ArgumentParser(description="Index the ShilpoHub JSON datasets into Qdrant.")
    parser.add_argument("--data-dir", default=str(config.DATA_DIR),
                        help="folder holding craft.json, craftDetails.json and GEO.json")
    parser.add_argument("--recreate", action="store_true",
                        help="drop the collection first (use after changing the model or the data)")
    args = parser.parse_args()

    print("--- INGEST -------------------------------------------------")
    datasets = load_json(args.data_dir)                                       # 1
    docs = normalize_json(datasets)                                           # 2
    docs = clean_data(docs)                                                   # 3
    chunks = chunk_documents(docs)                                            # 4  (aspect chunks, <200 tokens)
    embeddings = get_embeddings()                                             # 5  dense (BM25 sparse is made in step 6)
    store_chunks(chunks, embeddings, config.COLLECTION_NAME, recreate=args.recreate)  # 6

    print("\nDone. Now ask something:  python ask.py \"What is Jamdani?\"")


if __name__ == "__main__":
    main()
