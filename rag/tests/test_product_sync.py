"""Offline tests for the product sync worker: no Gemini, no Qdrant server, no backend.

    python -m unittest tests.test_product_sync -v
"""

import unittest
from types import SimpleNamespace

from qdrant_client import QdrantClient

from products import settings
from products.store import CATEGORY_CRAFT_KEYS, ProductVectorStore
from products.sync import ProductSyncWorker

DIM = 8


class FakeEmbedder:
    model = "fake-embedding"
    dim = DIM

    def __init__(self):
        self.calls = 0

    def embed_documents(self, texts):
        self.calls += 1
        return [[(hash((t, i)) % 97) / 97.0 for i in range(DIM)] for t in texts]


class FakeSparse:
    def passage_embed(self, texts):
        for t in texts:
            yield SimpleNamespace(indices=[abs(hash(w)) % 1000 for w in t.split()[:5]] or [0], values=[1.0] * (len(t.split()[:5]) or 1))


class FakeApi:
    def __init__(self, items):
        self.items = items
        self.acks = []

    def pending(self, limit):
        return {"items": self.items[:limit], "pendingTotal": len(self.items)}

    def ack(self, items):
        self.acks.extend(items)
        done = {a["productId"] for a in items}
        self.items = [i for i in self.items if i["productId"] not in done]


def product(pid="11111111-1111-1111-1111-111111111111", action="upsert", price=8900.0, category="jamdani-weaving", version=1):
    return {
        "productId": pid, "version": version, "action": action, "textHash": "h1",
        "chunks": [{"kind": "overview", "text": "Jamdani Dupatta. From Dhakai Jamdani, Narayanganj."},
                   {"kind": "story", "text": "Hand woven on a jala loom."}],
        "payload": {"product_id": pid, "name": "Jamdani Dupatta", "category_slug": category, "effective_price": price,
                    "in_stock": True, "is_public": True, "rating": 4.6},
    }


class ProductSyncTests(unittest.TestCase):
    def setUp(self):
        self.client = QdrantClient(":memory:")
        self.store = ProductVectorStore(self.client, DIM, "shilpohub_products_test", embedded=True)
        self.store.ensure_collection()
        self.embedder = FakeEmbedder()

    def worker(self, items):
        self.api = FakeApi(items)
        return ProductSyncWorker(self.api, self.store, self.embedder, FakeSparse())

    def test_upsert_stores_one_point_per_chunk_and_acks(self):
        stats = self.worker([product()]).run_once()
        self.assertEqual(stats["upserted"], 1)
        self.assertEqual(self.store.count(), 2)
        self.assertTrue(self.api.acks[0]["success"])
        self.assertEqual(self.api.acks[0]["embeddingModel"], "fake-embedding")
        payload = self.client.retrieve(self.store.collection, [self.store.point_id("11111111-1111-1111-1111-111111111111", "overview")])[0].payload
        self.assertEqual(payload["craft_key"], "jamdani")
        self.assertEqual(payload["chunk_kind"], "overview")
        self.assertIn("Jamdani Dupatta", payload["page_content"])

    def test_payload_only_refresh_does_not_embed(self):
        self.worker([product()]).run_once()
        calls = self.embedder.calls
        stats = self.worker([product(action="payload", price=7900.0, version=2)]).run_once()
        self.assertEqual(stats["payload"], 1)
        self.assertEqual(self.embedder.calls, calls)                      # no new embedding call
        payload = self.client.retrieve(self.store.collection, [self.store.point_id("11111111-1111-1111-1111-111111111111", "story")])[0].payload
        self.assertEqual(payload["effective_price"], 7900.0)

    def test_payload_refresh_with_missing_points_falls_back_to_full_upsert(self):
        stats = self.worker([product(action="payload")]).run_once()
        self.assertEqual(stats["upserted"], 1)
        self.assertEqual(self.store.count(), 2)

    def test_delete_removes_points(self):
        self.worker([product()]).run_once()
        stats = self.worker([{"productId": "11111111-1111-1111-1111-111111111111", "version": 3, "action": "delete"}]).run_once()
        self.assertEqual(stats["deleted"], 1)
        self.assertEqual(self.store.count(), 0)

    def test_embedding_failure_is_acked_as_failed_not_lost(self):
        class Boom(FakeEmbedder):
            def embed_documents(self, texts):
                raise RuntimeError("429 quota")
        worker = ProductSyncWorker(FakeApi([product()]), self.store, Boom(), FakeSparse())
        stats = worker.run_once()
        self.assertEqual(stats["failed"], 1)
        self.assertFalse(worker.api.acks[0]["success"])
        self.assertIn("429", worker.api.acks[0]["error"])
        self.assertEqual(self.store.count(), 0)

    def test_products_are_isolated_by_id(self):
        other = "22222222-2222-2222-2222-222222222222"
        self.worker([product(), product(pid=other, category="shital-pati")]).run_once()
        self.assertEqual(self.store.count(), 4)
        self.store.delete_product(other)
        self.assertEqual(self.store.count(), 2)

    def test_refuses_heritage_and_travel_collections(self):
        for name in ("shilpohub", "travel-planner"):
            with self.assertRaises(RuntimeError):
                ProductVectorStore(self.client, DIM, name, embedded=True)
        self.assertNotIn(settings.COLLECTION, settings.PROTECTED_COLLECTIONS)

    def test_craft_key_vocabulary_matches_heritage_keys(self):
        import json
        from pathlib import Path
        import config
        details = json.loads((Path(config.DATA_DIR) / "craftDetails.json").read_text(encoding="utf-8"))
        items = details if isinstance(details, list) else next(v for v in details.values() if isinstance(v, list))
        known = {i.get("id") or i.get("slug") for i in items if isinstance(i, dict)}
        for category, key in CATEGORY_CRAFT_KEYS.items():
            self.assertIn(key, known, f"{category} -> {key} is not a heritage craft_key")


if __name__ == "__main__":
    unittest.main()
