"""Ingestion for the isolated Travel Planner knowledge base - steps 1-6, reusing the same
step 1/4/5/6 modules as ingest.py, with a tourism-specific normalize/clean step (rag/travel/)
and a separate Qdrant collection so it is never mixed with the craft/heritage collection.

    python ingest_travel.py                 # tourism dataset from data/ into "travel-planner"
    python ingest_travel.py --recreate       # drop the collection first
    python ingest_travel.py --collection my-kb

Kept as a separate script from ingest.py on purpose: the craft pipeline's CLI is never at
risk of a change made here. Re-running without --recreate is safe (stable chunk ids upsert).
"""

import argparse

import config
from rag.step05_embedding import get_embeddings
from rag.travel.ingest import DEFAULT_COLLECTION, ingest_travel_dataset


def main() -> None:
    config.ensure_utf8_console()

    parser = argparse.ArgumentParser(description="Index the Travel Planner tourism dataset into Qdrant.")
    parser.add_argument("--data-dir", default=str(config.DATA_DIR),
                        help="folder holding bangladesh_heritage_tourism_dataset.json")
    parser.add_argument("--collection", default=DEFAULT_COLLECTION,
                        help=f"Qdrant collection name (default: {DEFAULT_COLLECTION})")
    parser.add_argument("--recreate", action="store_true",
                        help="drop the collection first (use after changing the model or the data)")
    args = parser.parse_args()

    print("--- INGEST (Travel Planner) ---------------------------------")
    embeddings = get_embeddings()
    result = ingest_travel_dataset(
        collection_name=args.collection, embeddings=embeddings,
        data_dir=args.data_dir, recreate=args.recreate,
    )
    print(f"\nDone: {result['documents']} documents, {result['chunks']} chunks in collection '{args.collection}'.")


if __name__ == "__main__":
    main()
