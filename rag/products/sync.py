"""The product embedding / sync worker.

    backend (PostgreSQL, source of truth)  --HTTP feed-->  worker  --embeds (Gemini)-->  Qdrant `shilpohub_products`
                                           <---- ack ----

For every dirty product the feed says what to do:
    upsert   the embedded text changed (or never indexed): embed both chunks, store the points
    payload  only price / stock / rating / visibility changed: refresh the payload, NO embedding call
    delete   the product is gone: remove its points

Results are acknowledged with the feed `version`; a product edited again while the worker was busy stays dirty
and is picked up on the next round. Failures are acknowledged too (the backend retries up to 5 times).
"""

import time
from typing import Any, Dict, List

from products import settings
from products.store import ProductVectorStore


class ProductSyncWorker:
    def __init__(self, api, store: ProductVectorStore, embedder, sparse):
        self.api = api
        self.store = store
        self.embedder = embedder
        self.sparse = sparse

    def run_once(self, limit: int = settings.BATCH_SIZE) -> Dict[str, int]:
        """Process one batch of pending products. Returns counts."""
        batch = self.api.pending(limit)
        items = batch.get("items", [])
        stats = {"pending_total": int(batch.get("pendingTotal", 0)), "fetched": len(items), "upserted": 0, "payload": 0, "deleted": 0, "failed": 0}
        if not items:
            return stats

        acks: List[Dict[str, Any]] = []
        to_embed: List[Dict[str, Any]] = []

        for item in items:
            try:
                action = item["action"]
                if action == "delete":
                    self.store.delete_product(item["productId"])
                    acks.append(self._ok(item))
                    stats["deleted"] += 1
                elif action == "payload" and self.store.has_points(item["productId"]):
                    self.store.refresh_payload(item["productId"], item["payload"])
                    acks.append(self._ok(item))
                    stats["payload"] += 1
                else:
                    # "upsert", or a payload refresh whose points are missing (e.g. the collection was recreated).
                    to_embed.append(item)
            except Exception as exc:  # noqa: BLE001 - one bad product must not stop the batch
                acks.append(self._failed(item, exc))
                stats["failed"] += 1

        if to_embed:
            self._embed_and_store(to_embed, acks, stats)

        self.api.ack(acks)
        return stats

    def run_watch(self, interval: float = 30.0, limit: int = settings.BATCH_SIZE) -> None:
        """Poll forever; when the backlog is empty, sleep."""
        while True:
            stats = self.run_once(limit)
            print(f"[sync] {stats}")
            if stats["fetched"] == 0 or stats["pending_total"] <= stats["fetched"]:
                time.sleep(interval)

    # ---- helpers ----
    def _embed_and_store(self, items: List[Dict[str, Any]], acks: List[Dict[str, Any]], stats: Dict[str, int]) -> None:
        texts = [chunk["text"] for item in items for chunk in item["chunks"]]
        try:
            dense = self.embedder.embed_documents(texts)
            sparse = list(self.sparse.passage_embed(texts))
        except Exception as exc:  # noqa: BLE001 - e.g. Gemini quota; every item is retried later
            for item in items:
                acks.append(self._failed(item, exc))
                stats["failed"] += 1
            return

        cursor = 0
        for item in items:
            n = len(item["chunks"])
            try:
                self.store.upsert_product(
                    item["productId"], item["chunks"], dense[cursor:cursor + n], sparse[cursor:cursor + n],
                    item["payload"], item.get("textHash") or "", self.embedder.model)
                acks.append(self._ok(item, embedded=True))
                stats["upserted"] += 1
            except Exception as exc:  # noqa: BLE001
                acks.append(self._failed(item, exc))
                stats["failed"] += 1
            cursor += n

    def _ok(self, item: Dict[str, Any], embedded: bool = False) -> Dict[str, Any]:
        return {
            "productId": item["productId"], "version": item["version"], "success": True,
            "textHash": item.get("textHash"), "embeddingModel": self.embedder.model if embedded else None,
        }

    @staticmethod
    def _failed(item: Dict[str, Any], exc: Exception) -> Dict[str, Any]:
        return {"productId": item["productId"], "version": item["version"], "success": False, "error": f"{type(exc).__name__}: {exc}"[:900]}
