"""Offline tests for product query analysis and retrieval: no Gemini, no backend, no Qdrant server.

    python -m unittest tests.test_product_search -v
"""

import re
import unittest
from types import SimpleNamespace

from qdrant_client import QdrantClient

from products.analysis import empty_analysis, heuristic_analysis, normalize
from products.retrieve import ProductRetriever, build_filter
from products.store import ProductVectorStore
from products.vocab import Vocabulary

DIM = 64

VOCAB = Vocabulary(
    categories=[{"slug": "jamdani-weaving", "name": "Dhakai Jamdani"}, {"slug": "shital-pati", "name": "Shital Pati"},
                {"slug": "jute-craft", "name": "Jute Craft"}, {"slug": "nakshi-kantha", "name": "Nakshi Kantha"}],
    product_types=[{"slug": "saree", "name": "Saree", "name_bn": "শাড়ি"}, {"slug": "mat", "name": "Mat (Pati)", "name_bn": "পাটি"},
                   {"slug": "bag", "name": "Bag & Pouch", "name_bn": "ব্যাগ"}],
    materials=[{"slug": "cotton", "name": "Cotton", "name_bn": ""}, {"slug": "jute", "name": "Jute", "name_bn": ""}],
    districts=[{"name": "Dhaka", "division": "Dhaka"}, {"name": "Sylhet", "division": "Sylhet"}, {"name": "Narayanganj", "division": "Dhaka"}],
)


class BagOfWords:
    """Deterministic 'embedding': hashed word counts. Similar words -> similar vectors, enough to test retrieval."""
    model = "fake"
    dim = DIM

    @staticmethod
    def _vec(text):
        v = [0.0] * DIM
        for w in re.findall(r"\w+", text.lower()):
            v[hash(w) % DIM] += 1.0
        norm = sum(x * x for x in v) ** 0.5 or 1.0
        return [x / norm for x in v]

    def embed_documents(self, texts):
        return [self._vec(t) for t in texts]

    def embed_query(self, text):
        return self._vec(text)


class Sparse:
    def _emb(self, text):
        words = sorted({abs(hash(w)) % 5000 for w in re.findall(r"\w+", text.lower())}) or [0]
        return SimpleNamespace(indices=words, values=[1.0] * len(words))

    def passage_embed(self, texts):
        return [self._emb(t) for t in texts]

    def query_embed(self, text):
        return [self._emb(text)]


class AnalysisTests(unittest.TestCase):
    def test_price_budget_banglish(self):
        a = heuristic_analysis("10,000 takar moddhe ekta bhalo Jamdani dao", VOCAB)
        self.assertEqual(a["max_price"], 10000.0)
        self.assertIsNone(a["min_price"])
        self.assertEqual(a["category_slug"], "jamdani-weaving")

    def test_bangla_digits_and_thousand_words(self):
        self.assertEqual(heuristic_analysis("১০০০০ টাকার মধ্যে শাড়ি", VOCAB)["max_price"], 10000.0)
        self.assertEqual(heuristic_analysis("saree under 5k", VOCAB)["max_price"], 5000.0)

    def test_min_price(self):
        a = heuristic_analysis("saree 8000 er upore", VOCAB)
        self.assertEqual(a["min_price"], 8000.0)
        self.assertIsNone(a["max_price"])

    def test_highest_rated_is_a_sort_not_a_filter(self):
        a = heuristic_analysis("Highest rated Jamdani gula dekhao", VOCAB)
        self.assertEqual(a["sort"], "rating")
        self.assertEqual(a["category_slug"], "jamdani-weaving")
        self.assertIsNone(a["min_rating"])
        self.assertFalse(a["semantic"])                   # fully structured -> the backend answers with SQL

    def test_district_and_availability(self):
        a = heuristic_analysis("Dhakar moddhe available craft products ki ache?", VOCAB)
        self.assertEqual(a["district"], "Dhaka")
        self.assertTrue(a["in_stock_only"])

    def test_wedding_saree_stays_semantic(self):
        a = heuristic_analysis("Wedding er jonno traditional saree khujchi", VOCAB)
        self.assertEqual(a["product_type"], "saree")
        self.assertIn("wedding", a["occasions"])
        self.assertTrue(a["semantic"])

    def test_plain_craft_search(self):
        a = heuristic_analysis("Jamdani khuje dao", VOCAB)
        self.assertEqual(a["category_slug"], "jamdani-weaving")
        self.assertIsNone(a["max_price"])

    def test_normalize_drops_invented_values(self):
        raw = {"english_query": "bag", "semantic": True, "sort": "banana", "max_price": "abc", "min_rating": 9,
               "district": "Atlantis", "category_slug": "made-up", "product_type": "bag", "materials": ["jute", "unobtainium"],
               "occasions": ["Wedding"], "language": "klingon"}
        a = normalize(raw, "bag", VOCAB, "gemini")
        self.assertEqual(a["sort"], "relevance")
        self.assertIsNone(a["max_price"])
        self.assertIsNone(a["min_rating"])
        self.assertIsNone(a["district"])
        self.assertIsNone(a["category_slug"])
        self.assertEqual(a["product_type"], "bag")
        self.assertEqual(a["materials"], ["jute"])
        self.assertEqual(a["occasions"], ["wedding"])
        self.assertEqual(a["language"], "en")

    def test_normalize_swaps_inverted_range(self):
        a = normalize({"english_query": "x", "min_price": 9000, "max_price": 3000}, "x", VOCAB, "gemini")
        self.assertEqual((a["min_price"], a["max_price"]), (3000.0, 9000.0))


def indexed_store(products):
    client = QdrantClient(":memory:")
    store = ProductVectorStore(client, DIM, "shilpohub_products_test", embedded=True)
    store.ensure_collection()
    emb, sp = BagOfWords(), Sparse()
    for pid, name, text, payload in products:
        base = {"product_id": pid, "name": name, "is_public": True, "in_stock": True, "rating": 4.0, "effective_price": 5000.0,
                "district": "Dhaka", "division": "Dhaka", "category_slug": "jamdani-weaving", "product_type": "saree", "materials": ["cotton"]}
        base.update(payload)
        chunks = [{"kind": "overview", "text": text}]
        store.upsert_product(pid, chunks, emb.embed_documents([text]), sp.passage_embed([text]), base, "h", "fake")
    return store, ProductRetriever(store, emb, sp)


P1, P2, P3, P4 = ("00000000-0000-0000-0000-00000000000%d" % i for i in range(1, 5))


class RetrievalTests(unittest.TestCase):
    def setUp(self):
        self.store, self.retriever = indexed_store([
            (P1, "Jamdani Saree", "Jamdani saree wedding cotton Dhaka", {"effective_price": 16650.0}),
            (P2, "Jamdani Dupatta", "Jamdani dupatta gift cotton Dhaka", {"effective_price": 8900.0, "product_type": "dupatta"}),
            (P3, "Shital Pati Mat", "Shital pati mat cane Sylhet", {"category_slug": "shital-pati", "product_type": "mat", "district": "Sylhet",
                                                                   "division": "Sylhet", "effective_price": 2000.0, "in_stock": False, "materials": ["cane"]}),
            (P4, "Hidden Jamdani", "Jamdani saree draft not approved", {"is_public": False}),
        ])

    def ids(self, analysis):
        return [c["productId"] for c in self.retriever.candidates({**empty_analysis("q"), **analysis})["candidates"]]

    def test_semantic_match_ranks_the_relevant_product_first(self):
        self.assertEqual(self.ids({"english_query": "jamdani wedding saree"})[0], P1)

    def test_unapproved_products_never_returned(self):
        self.assertNotIn(P4, self.ids({"english_query": "jamdani saree"}))

    def test_budget_filter_is_applied_in_the_index(self):
        found = self.ids({"english_query": "jamdani", "max_price": 10000})
        self.assertIn(P2, found)
        self.assertNotIn(P1, found)

    def test_in_stock_and_district_filters(self):
        self.assertNotIn(P3, self.ids({"english_query": "pati mat", "in_stock_only": True}))
        self.assertEqual(self.ids({"english_query": "pati mat", "district": "Sylhet"}), [P3])

    def test_relaxed_pass_when_the_strict_filter_finds_too_little(self):
        result = self.retriever.candidates({**empty_analysis("q"), "english_query": "jamdani", "product_type": "mat"})
        self.assertEqual(result["pass"], "relaxed")
        self.assertIn(P1, [c["productId"] for c in result["candidates"]])

    def test_filter_only_request_has_no_vector_search(self):
        self.assertEqual(self.retriever.candidates({**empty_analysis("q"), "english_query": ""}), {"pass": "none", "candidates": []})

    def test_scores_are_normalised(self):
        for c in self.retriever.candidates({**empty_analysis("q"), "english_query": "jamdani"})["candidates"]:
            self.assertTrue(0.0 < c["score"] <= 1.0)

    def test_build_filter_always_requires_public(self):
        keys = [c.key for c in build_filter(empty_analysis("q"), strict=True).must]
        self.assertIn("is_public", keys)


if __name__ == "__main__":
    unittest.main()
