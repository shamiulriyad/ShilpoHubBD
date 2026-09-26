"""Product embedding / sync worker CLI (separate collection `shilpohub_products`).

    python ingest_products.py --stats                  # how many products are dirty / synced / failed
    python ingest_products.py                          # process one batch of pending products
    python ingest_products.py --all                    # keep going until nothing is pending
    python ingest_products.py --watch                  # run forever (poll every --interval seconds)
    python ingest_products.py --requeue-all            # mark every product for re-indexing, then sync
    python ingest_products.py --recreate               # drop the product collection, re-queue everything

PostgreSQL stays the source of truth: this script only talks to the backend's internal feed with a shared key
and never sees database credentials. It never touches the `shilpohub` or `travel-planner` collections.
Kept as its own script (like ingest_travel.py) so the craft / travel pipelines are never at risk.
"""

import argparse
import sys

import config


def main() -> int:
    config.ensure_utf8_console()

    parser = argparse.ArgumentParser(description="Sync products from the backend into the product Qdrant collection.")
    parser.add_argument("--stats", action="store_true", help="print the backend's index status counts and exit")
    parser.add_argument("--all", action="store_true", help="loop until no products are pending")
    parser.add_argument("--watch", action="store_true", help="poll forever")
    parser.add_argument("--interval", type=float, default=30.0, help="seconds between polls with --watch")
    parser.add_argument("--limit", type=int, default=None, help="products per batch (default PRODUCT_SYNC_BATCH)")
    parser.add_argument("--requeue-all", action="store_true", help="mark every product dirty before syncing")
    parser.add_argument("--recreate", action="store_true", help="drop the product collection and re-queue every product")
    parser.add_argument("--collection", default=None, help="override the collection name (never a heritage/travel collection)")
    args = parser.parse_args()

    from products import settings
    from products.api_client import ProductIndexApi

    api = ProductIndexApi()
    if args.stats:
        print(api.stats() or "no index states yet")
        return 0

    from products.embeddings import ProductEmbedder
    from products.store import ProductVectorStore, make_client
    from products.sync import ProductSyncWorker
    from rag.step05_embedding import get_sparse_embeddings   # existing BM25 sparse embedder, reused read-only

    embedder = ProductEmbedder()
    store = ProductVectorStore(make_client(), embedder.dim, args.collection or settings.COLLECTION)
    store.ensure_collection(recreate=args.recreate)
    print(f"[products] collection '{store.collection}' · model {embedder.model} ({embedder.dim}-dim) · "
          f"{'server ' + settings.QDRANT_URL if settings.QDRANT_URL else 'embedded ' + str(settings.QDRANT_PATH)}")

    if args.requeue_all or args.recreate:
        print(f"[products] re-queued {api.requeue_all()} products")

    worker = ProductSyncWorker(api, store, embedder, get_sparse_embeddings())
    limit = args.limit or settings.BATCH_SIZE

    if args.watch:
        worker.run_watch(args.interval, limit)
        return 0

    while True:
        stats = worker.run_once(limit)
        print(f"[products] {stats}")
        if not args.all or stats["fetched"] == 0 or stats["failed"] == stats["fetched"]:
            break

    print(f"[products] '{store.collection}' now holds {store.count()} points; backend status: {api.stats()}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
