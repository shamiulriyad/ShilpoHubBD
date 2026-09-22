"""Orchestrates steps 7-11 for one question, scoped to one Qdrant collection (a dedicated
collection per Knowledge Base keeps retrieval from crossing knowledge bases by default,
rather than relying on a filter that could be forgotten)."""

from typing import Dict, Optional

from rag import index_meta
from rag.pipeline import QueryContext, run_query
from rag.step06_vector_store import collection_exists


class KnowledgeBaseNotIndexedError(ValueError):
    """Raised when a collection has no ingested documents yet."""


def answer_question(
    *, collection_name: str, question: str, similarity_threshold: Optional[float], ctx: QueryContext,
) -> Dict:
    index_meta.check_before_query(collection_name)                       # model still matches the index?

    if not collection_exists(collection_name):
        raise KnowledgeBaseNotIndexedError(
            "The ShilpoHub knowledge base has not been indexed yet. Run the ingestion first."
        )

    # An explicit per-request threshold overrides MIN_RELEVANCE_SCORE from .env.
    return run_query(question, ctx, collection_name, min_score=similarity_threshold).response
