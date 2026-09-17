"""STEP 8 - Query embedding.

The question is turned into a vector with the same model used in step 5.
LangChain's retriever would do this internally, but doing it explicitly keeps
the pipeline visible and lets you inspect or cache the vector.
"""

from typing import List


def embed_question(embeddings, question: str) -> List[float]:
    vector = embeddings.embed_query(question)
    print(f"[8] Query vec  : {len(vector)} dimensions")
    return vector
