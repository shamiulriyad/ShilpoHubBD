"""STEP 9 - Retrieve relevant chunks.

Cosine similarity search in Qdrant against the query vector from step 8.
Returns (Document, score) pairs, highest score first.
"""

from typing import List, Tuple

from langchain_core.documents import Document


def retrieve(vector_store, query_vector: List[float], k: int) -> List[Tuple[Document, float]]:
    results = vector_store.similarity_search_with_score_by_vector(query_vector, k=k)

    print(f"[9] Retrieved  : {len(results)} chunks")
    for i, (doc, score) in enumerate(results, start=1):
        page = doc.metadata.get("page_number", "?")
        preview = doc.page_content[:70].replace("\n", " ")
        print(f"    S{i} page {page} | score {score:.3f} | {preview}...")

    return results
