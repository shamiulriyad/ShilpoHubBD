"""Hybrid retrieval over the product collection.

Returns CANDIDATES only (product ids + relevance). It does not answer, price or judge availability: the backend
loads every candidate from PostgreSQL, re-applies the authoritative filters (price, stock, visibility) and ranks.

Two passes, like the heritage retriever's fallback: STRICT applies craft / product type / material as hard filters;
if that finds too little, RELAXED drops them (the backend then scores how well each product matches them).
"""

from typing import Any, Dict, List, Optional

from qdrant_client.http.models import FieldCondition, Filter, MatchAny, MatchValue, Range, SparseVector

from products.store import DENSE, SPARSE, ProductVectorStore

RRF_K = 60
PER_LEG = 60          # nearest chunks taken from each of the dense and the BM25 search
MIN_STRICT = 5        # fewer strict candidates than this -> also run the relaxed pass


def build_filter(analysis: Dict[str, Any], strict: bool) -> Filter:
    must = [FieldCondition(key="is_public", match=MatchValue(value=True))]
    if analysis.get("in_stock_only"):
        must.append(FieldCondition(key="in_stock", match=MatchValue(value=True)))
    if analysis.get("min_price") is not None or analysis.get("max_price") is not None:
        must.append(FieldCondition(key="effective_price", range=Range(gte=analysis.get("min_price"), lte=analysis.get("max_price"))))
    if analysis.get("min_rating") is not None:
        must.append(FieldCondition(key="rating", range=Range(gte=analysis["min_rating"])))
    if analysis.get("district"):
        must.append(FieldCondition(key="district", match=MatchValue(value=analysis["district"])))
    elif analysis.get("division"):
        must.append(FieldCondition(key="division", match=MatchValue(value=analysis["division"])))
    if strict:
        if analysis.get("category_slug"):
            must.append(FieldCondition(key="category_slug", match=MatchValue(value=analysis["category_slug"])))
        if analysis.get("product_type"):
            must.append(FieldCondition(key="product_type", match=MatchValue(value=analysis["product_type"])))
        if analysis.get("materials"):
            must.append(FieldCondition(key="materials", match=MatchAny(any=analysis["materials"])))
    return Filter(must=must)


def has_soft_filters(analysis: Dict[str, Any]) -> bool:
    return bool(analysis.get("category_slug") or analysis.get("product_type") or analysis.get("materials"))


class ProductRetriever:
    def __init__(self, store: ProductVectorStore, embedder, sparse):
        self.store = store
        self.embedder = embedder
        self.sparse = sparse

    def candidates(self, analysis: Dict[str, Any], limit: int = 40) -> Dict[str, Any]:
        text = (analysis.get("english_query") or "").strip()
        if not text:
            return {"pass": "none", "candidates": []}

        dense = self.embedder.embed_query(text)
        sparse_raw = next(iter(self.sparse.query_embed(text)))
        sparse = SparseVector(indices=[int(i) for i in sparse_raw.indices], values=[float(v) for v in sparse_raw.values])

        used = "strict"
        found = self._search(dense, sparse, build_filter(analysis, strict=True), limit)
        if len(found) < MIN_STRICT and has_soft_filters(analysis):
            relaxed = self._search(dense, sparse, build_filter(analysis, strict=False), limit)
            # Strict hits stay first; relaxed ones fill the rest.
            seen = {c["productId"] for c in found}
            found += [c for c in relaxed if c["productId"] not in seen]
            found = found[:limit]
            used = "relaxed"
        return {"pass": used, "candidates": found}

    def _search(self, dense, sparse, flt: Filter, limit: int) -> List[Dict[str, Any]]:
        dense_hits = self.store.search(DENSE, dense, flt, PER_LEG)
        sparse_hits = self.store.search(SPARSE, sparse, flt, PER_LEG)

        scores: Dict[str, float] = {}
        cosine: Dict[str, float] = {}
        for hits, is_dense in ((dense_hits, True), (sparse_hits, False)):
            seen_products = set()
            rank = 0
            for hit in hits:
                pid = str(hit.payload["product_id"])
                if pid in seen_products:            # a product's second chunk does not earn a second rank
                    continue
                seen_products.add(pid)
                rank += 1
                scores[pid] = scores.get(pid, 0.0) + 1.0 / (RRF_K + rank)
                if is_dense:
                    cosine[pid] = float(hit.score)

        best = 2.0 / (RRF_K + 1)                     # rank 1 in both legs
        ranked = sorted(scores.items(), key=lambda kv: -kv[1])[:limit]
        return [{"productId": pid, "score": round(score / best, 4), "semantic": round(cosine.get(pid, 0.0), 4)} for pid, score in ranked]
