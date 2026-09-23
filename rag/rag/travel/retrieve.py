"""Plain retrieval (no question-type classification, no answer generation) for the Travel
Planner knowledge base.

The .NET AI Travel Planner already calls Gemini once per plan (`GeminiAITourismProvider`),
combining this retrieved context with database and route data (see promt.txt section 7-8) --
this module's only job is: given a district/place type/interests, return the most relevant
grounded snippets. Reuses step08 (query embedding) and step09's generic `build_filter`/
`Searcher` primitives; never touches the craft-only QUESTION_TYPES/ROUTES/prompt files, so the
existing Heritage Assistant path is completely unaffected.
"""

from typing import Any, Dict, List, Optional

import config
from rag.step08_query_embedding import embed_query
from rag.step09_retrieve import Searcher, build_filter


def retrieve_travel_context(
    *, ctx, collection_name: str, query_text: str,
    district: Optional[str] = None, place_type: Optional[str] = None,
    interests: Optional[List[str]] = None, top_k: int = 6,
) -> List[Dict[str, Any]]:
    dense, sparse_vec = embed_query(ctx.embeddings, ctx.sparse, query_text)
    searcher = Searcher(client=ctx.client, collection=collection_name, dense=dense, sparse=sparse_vec)

    flt = build_filter(
        districts=[district] if district else None,
        category_norm=[place_type] if place_type else None,
        themes=list(interests) if interests else None,
    )

    used_filter = flt is not None
    hits = searcher.hybrid(flt, top_k)
    if not hits and used_filter:
        # Same fallback rule as the craft pipeline (step 9): a filtered search that finds
        # nothing is retried once with no filter, rather than returning nothing at all.
        hits = searcher.hybrid(None, top_k)
        used_filter = False

    kept = hits if used_filter else [h for h in hits if h.score >= config.MIN_RELEVANCE_SCORE]

    # A heritage item's background text is shared by every place it occurs at; return it once.
    seen_text, unique = set(), []
    for hit in kept:
        if hit.text not in seen_text:
            seen_text.add(hit.text)
            unique.append(hit)
    kept = unique

    return [
        {
            "text": hit.text,
            "sourceFile": hit.metadata.get("source_file"),
            "docId": hit.metadata.get("doc_id"),
            "districts": hit.metadata.get("districts") or [],
            "placeType": hit.metadata.get("category_norm"),
            "themes": hit.metadata.get("themes") or [],
            "score": round(hit.score, 4),
        }
        for hit in kept
    ]
