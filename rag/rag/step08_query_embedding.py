"""STEP 8 - Query embedding.

The English search query from step 7 (not the raw question - the embedding model reads
English only) is turned into the same two kinds of vector the chunks have:

  dense  : the semantic vector, from the same model used in step 5
  sparse : BM25 term weights, so exact words (a district, "GI", a craft name) are matched

LangChain's retriever would do this internally, but doing it explicitly keeps the pipeline
visible and lets step 9 re-use one pair of vectors for several filtered searches.
"""

from typing import List, Tuple

from qdrant_client.http.models import SparseVector


def embed_question(embeddings, question: str) -> List[float]:
    vector = embeddings.embed_query(question)
    print(f"[8] Query vec  : {len(vector)} dense dimensions", end="")
    return vector


def embed_question_sparse(sparse, question: str) -> SparseVector:
    embedding = next(iter(sparse.query_embed(question)))
    print(f" + {len(embedding.indices)} BM25 terms")
    return SparseVector(indices=embedding.indices.tolist(), values=embedding.values.tolist())


def embed_query(embeddings, sparse, question: str) -> Tuple[List[float], SparseVector]:
    """Both vectors for one query."""
    return embed_question(embeddings, question), embed_question_sparse(sparse, question)
