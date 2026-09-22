"""STEP 9 - Route the question and retrieve.

Step 7 decided the question type and its filters; this step turns that into a search. Every
search is HYBRID: a dense (semantic) search and a BM25 (exact words) search, each restricted
by the route's metadata filter, fused with Reciprocal Rank Fusion. RRF scores are ranks, not
similarities, so each hit is also re-scored with its real dense cosine - that number is what
the relevance threshold (MIN_RELEVANCE_SCORE) is applied to.

ROUTES (spec section 8):
    describe         craft_key; overview + geography; top_k 6; + siblings from the other files
    how_made         craft_key; production; top_k 6
    materials_tools  craft_key; production; top_k 6
    craft_location   craft_key; geography; top_k 6; + siblings
    location_crafts  districts (else divisions); geography; top_k 25; grouped by craft
    status           is_unesco / is_gi / risk_level / craft filters; top_k 20; + REF-UNESCO
    list             category_norm; top_k 25; + REF-TAXONOMY; grouped by craft
    compare          each craft separately, top_k 6 each, under a heading per craft; + siblings
    time             craft_key; production; top_k 6; chunks from BOTH craft.json and craftDetails.json
    heritage         craft_key when named; top_k 8; + REF-SITES
    out_of_scope     no retrieval

Rules that apply to every route:
  * Fallback - a filtered search that finds nothing is retried once with no filters.
  * Relevance - hits found WITHOUT a filter (no filter for the question, or the fallback) must
    reach MIN_RELEVANCE_SCORE in dense cosine, else they are dropped. Filtered hits are kept:
    the filter (this craft, this district) is itself the evidence of relevance.
  * Siblings - describe / craft_location / compare add the same craft's chunks from the other
    source files (at most 10 chunks in a craft's block).
"""

from collections import OrderedDict
from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional, Sequence

import numpy as np
from qdrant_client.http.models import FieldCondition, Filter, Fusion, FusionQuery, MatchAny, MatchValue, Prefetch

import config
from rag.step06_vector_store import DENSE_VECTOR, SPARSE_VECTOR

SOURCE_ORDER = ("craft.json", "craftDetails.json", "GEO.json")
ASPECT_ORDER = ("overview", "production", "geography", "reference")
MAX_BLOCK_CHUNKS = 10        # sibling expansion stops here (spec section 9)

# question_type -> (aspects to search, top_k)
ROUTES = {
    "describe": (("overview", "geography"), 6),
    "how_made": (("production",), 6),
    "materials_tools": (("production",), 6),
    "craft_location": (("geography",), 6),
    "location_crafts": (("geography",), 25),
    "status": (None, 20),            # aspects depend on which filters the question has
    "list": (("overview",), 25),     # one overview chunk per record -> one entry per craft
    "compare": (None, 6),
    "time": (("production",), 6),
    "heritage": (None, 8),
}
_EXPAND = {"describe", "craft_location", "compare"}


@dataclass
class Hit:
    text: str
    metadata: Dict[str, Any]
    score: float                     # dense cosine similarity (not the RRF fusion score)


@dataclass
class Block:
    heading: Optional[str]
    hits: List[Hit]


@dataclass
class Retrieval:
    question_type: str
    blocks: List[Block] = field(default_factory=list)
    filters: Dict[str, Any] = field(default_factory=dict)
    fallback: bool = False           # a filtered search found nothing and was retried unfiltered
    skipped: bool = False            # out_of_scope: retrieval was not run

    @property
    def hits(self) -> List[Hit]:
        """Every hit in context order - hit N here is [Doc N] in the prompt."""
        return [hit for block in self.blocks for hit in block.hits]

    @property
    def craft_keys(self) -> List[str]:
        return list(dict.fromkeys(h.metadata["craft_key"] for h in self.hits if h.metadata.get("craft_key")))


# --- filters and searches ------------------------------------------------------------------------

def _condition(key: str, value: Any) -> FieldCondition:
    if isinstance(value, (list, tuple, set)):
        values = list(value)
        match = MatchValue(value=values[0]) if len(values) == 1 else MatchAny(any=values)
    else:
        match = MatchValue(value=value)
    return FieldCondition(key=f"metadata.{key}", match=match)


def build_filter(**fields: Any) -> Optional[Filter]:
    """AND of the metadata fields given (a list means "any of"); None values are skipped and
    no fields at all means no filter. e.g. build_filter(craft_key=["jamdani"], aspect=["overview"])."""
    conditions = [_condition(key, value) for key, value in fields.items()
                  if value is not None and value != [] and value != ()]
    return Filter(must=conditions) if conditions else None


def _cosine(query: Sequence[float], vector: Sequence[float]) -> float:
    a, b = np.asarray(query, dtype=float), np.asarray(vector, dtype=float)
    denominator = float(np.linalg.norm(a) * np.linalg.norm(b))
    return float(a @ b / denominator) if denominator else 0.0


@dataclass
class Searcher:
    client: Any
    collection: str
    dense: List[float]
    sparse: Any                      # qdrant SparseVector

    def _hit(self, point) -> Hit:
        return Hit(text=point.payload["page_content"], metadata=point.payload["metadata"],
                   score=_cosine(self.dense, point.vector[DENSE_VECTOR]))

    def hybrid(self, flt: Optional[Filter], limit: int) -> List[Hit]:
        """Dense + BM25, fused with RRF, both restricted by `flt`."""
        depth = max(limit * 3, 40)
        result = self.client.query_points(
            self.collection,
            prefetch=[Prefetch(query=self.dense, using=DENSE_VECTOR, limit=depth, filter=flt),
                      Prefetch(query=self.sparse, using=SPARSE_VECTOR, limit=depth, filter=flt)],
            query=FusionQuery(fusion=Fusion.RRF),
            limit=limit, with_payload=True, with_vectors=[DENSE_VECTOR],
        )
        return [self._hit(p) for p in result.points]

    def all_matching(self, flt: Optional[Filter], limit: int = 200) -> List[Hit]:
        """Every chunk matching `flt`, no ranking (siblings, REF-* documents)."""
        points, _ = self.client.scroll(self.collection, scroll_filter=flt, limit=limit,
                                       with_payload=True, with_vectors=[DENSE_VECTOR])
        return [self._hit(p) for p in points]


def _order(hits: List[Hit]) -> List[Hit]:
    """A stable reading order for chunks pulled in without ranking."""
    return sorted(hits, key=lambda h: (
        SOURCE_ORDER.index(h.metadata["source_file"]) if h.metadata["source_file"] in SOURCE_ORDER else 9,
        ASPECT_ORDER.index(h.metadata["aspect"]) if h.metadata["aspect"] in ASPECT_ORDER else 9,
        h.metadata.get("chunk_index", 0)))


def _dedupe(hits: List[Hit]) -> List[Hit]:
    seen, out = set(), []
    for hit in hits:
        key = hit.metadata.get("chunk_key")
        if key not in seen:
            seen.add(key)
            out.append(hit)
    return out


def _search(searcher: Searcher, flt: Optional[Filter], limit: int, floor: float, notes: Dict[str, Any],
            evidence: bool) -> List[Hit]:
    """One hybrid search with the fallback and relevance rules applied.

    `evidence` says whether the filter itself shows relevance (this craft, this district ...).
    A filter that only picks an aspect does not: its hits still have to reach the floor."""
    hits = searcher.hybrid(flt, limit)
    if hits:
        return hits if evidence else [h for h in hits if h.score >= floor]

    if evidence:                                          # a filtered search found nothing: retry once, unfiltered
        notes["fallback"] = True
        return [h for h in searcher.hybrid(None, limit) if h.score >= floor]
    return []


# --- expansion and extras ----------------------------------------------------------------------------

def _siblings(searcher: Searcher, key: str, aspects: Sequence[str], have: List[Hit], cap: int) -> List[Hit]:
    """The same craft's chunks (same aspects) from the source files the hits do not cover yet."""
    room = cap - len(have)
    if room <= 0:
        return []
    covered = {h.metadata["source_file"] for h in have if h.metadata.get("craft_key") == key}
    present = {h.metadata["chunk_key"] for h in have}
    candidates = [h for h in searcher.all_matching(build_filter(craft_key=[key], aspect=list(aspects)))
                  if h.metadata["source_file"] not in covered and h.metadata["chunk_key"] not in present]
    return _order(candidates)[:room]


def _reference(searcher: Searcher, doc_id: str) -> List[Hit]:
    return _order(searcher.all_matching(build_filter(doc_id=[doc_id])))


def _ensure_time_from_both_files(searcher: Searcher, keys: List[str], hits: List[Hit]) -> List[Hit]:
    """`time` questions must see the duration from craft.json AND craftDetails.json (they
    word it differently: "variable" vs "1-6 months"), so a chunk holding "Time required" from
    each file is added when the ranking did not already bring it."""
    extra: List[Hit] = []
    for source_file in ("craft.json", "craftDetails.json"):
        for key in keys:
            timed = [h for h in searcher.all_matching(
                         build_filter(craft_key=[key], aspect=["production"], source_file=[source_file]))
                     if "Time required" in h.text]
            if timed and not any(h.metadata.get("chunk_key") == timed[0].metadata["chunk_key"] for h in hits + extra):
                extra.append(timed[0])
    return extra


def _grouped(hits: List[Hit]) -> List[Block]:
    """Blocks per craft, in order of first appearance (a craft's chunks stay together)."""
    groups: "OrderedDict[str, List[Hit]]" = OrderedDict()
    for hit in hits:
        groups.setdefault(hit.metadata.get("craft_key") or hit.metadata["doc_id"], []).append(hit)
    return [Block(heading=g[0].metadata["name_en"], hits=g) for g in groups.values()]


# --- entry point ----------------------------------------------------------------------------------------

def retrieve(client, collection: str, analysis: Dict[str, Any], dense: List[float], sparse,
             min_score: Optional[float] = None) -> Retrieval:
    qtype = analysis["question_type"]
    result = Retrieval(question_type=qtype)
    if qtype == "out_of_scope":
        result.skipped = True
        print("[9] Route      : out_of_scope - retrieval skipped")
        return result

    aspects, top_k = ROUTES.get(qtype, (None, config.TOP_K))
    floor = config.MIN_RELEVANCE_SCORE if min_score is None else min_score
    searcher = Searcher(client, collection, dense, sparse)
    keys, districts, divisions = analysis["craft_keys"], analysis["districts"], analysis["divisions"]
    notes: Dict[str, Any] = {}
    filters: Dict[str, Any] = {}

    def search(limit: int = top_k, **fields: Any) -> List[Hit]:
        used = {k: v for k, v in fields.items() if v not in (None, [], ())}
        filters.update(used)
        evidence = any(k != "aspect" for k in used)
        return _search(searcher, build_filter(**fields), limit, floor, notes, evidence)

    extras: List[Hit] = []
    blocks: List[Block] = []

    if qtype == "compare" and len(keys) >= 2:
        for key in keys:                                              # each craft on its own, then merged
            hits = search(craft_key=[key])
            hits = _dedupe(hits + _siblings(searcher, key, ("overview", "geography"), hits, MAX_BLOCK_CHUNKS))
            if hits:
                blocks.append(Block(heading=hits[0].metadata["name_en"], hits=hits))
        filters["craft_key"] = keys
    else:
        if qtype == "location_crafts":
            hits = search(districts=districts or None,
                          divisions=None if districts else (divisions or None), aspect=list(aspects))
        elif qtype == "status":
            unesco_or_gi = analysis["is_unesco"] or analysis["is_gi"]
            aspect = (["overview"] if unesco_or_gi and not analysis["risk_levels"] else
                      ["geography"] if analysis["risk_levels"] and not unesco_or_gi else None)
            hits = search(craft_key=keys or None, category_norm=analysis["category_norm"] or None,
                          risk_level=analysis["risk_levels"] or None,
                          is_unesco=True if analysis["is_unesco"] else None,
                          is_gi=True if analysis["is_gi"] else None, aspect=aspect)
            if analysis["is_unesco"] or "unesco" in analysis["english_query"].lower():
                extras = _reference(searcher, "REF-UNESCO")
        elif qtype == "list":
            hits = search(category_norm=analysis["category_norm"] or None, aspect=list(aspects))
            extras = _reference(searcher, "REF-TAXONOMY")
        elif qtype == "heritage":
            hits = search(craft_key=keys or None)
            extras = _reference(searcher, "REF-SITES")
        elif qtype == "time":
            hits = search(craft_key=keys or None, aspect=list(aspects))
            if keys:
                hits += _ensure_time_from_both_files(searcher, keys, hits)
        else:                                                         # describe, how_made, materials_tools, craft_location, and unknown types
            hits = search(craft_key=keys or None, aspect=list(aspects) if aspects else None)
            if qtype in _EXPAND and keys:
                for key in keys:
                    hits += _siblings(searcher, key, aspects or ("overview", "geography"), hits, MAX_BLOCK_CHUNKS)

        hits = _dedupe(hits)
        if qtype in ("location_crafts", "status", "list"):
            blocks = _grouped(hits)
        elif hits:
            blocks = [Block(heading=None, hits=hits)]
        if extras:
            blocks.insert(0, Block(heading=extras[0].metadata["name_en"], hits=extras))

    result.blocks = [b for b in blocks if b.hits]
    result.filters = filters
    result.fallback = bool(notes.get("fallback"))

    crafts = len(result.craft_keys)
    shown = {k: v for k, v in filters.items()}
    print(f"[9] Route      : {qtype} | filters: {shown or 'none'} | top_k {top_k} -> {len(result.hits)} chunks, {crafts} craft(s)"
          + (" | fell back to an unfiltered search" if result.fallback else ""))
    for i, hit in enumerate(result.hits, start=1):
        m = hit.metadata
        print(f"    Doc {i} {m['name_en'][:34]} [{m['source_file']} / {m['aspect']}] | cosine {hit.score:.3f}")
    return result
