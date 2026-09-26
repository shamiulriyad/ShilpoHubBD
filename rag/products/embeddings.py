"""Gemini embeddings for products (gemini-embedding-001), separate from the embedding config of the other collections."""

import time
from typing import List

import config

from products import settings


class ProductEmbedder:
    """Documents and queries are embedded with the matching Gemini task types, at a fixed dimension."""

    def __init__(self, model: str = settings.EMBEDDING_MODEL, dim: int = settings.EMBEDDING_DIM):
        if not config.GOOGLE_API_KEY:
            raise RuntimeError("Gemini__ApiKey is not set in the root .env; product embeddings need it.")
        from langchain_google_genai import GoogleGenerativeAIEmbeddings

        self.model = model
        self.dim = dim
        name = model if model.startswith("models/") else f"models/{model}"
        self._client = GoogleGenerativeAIEmbeddings(model=name, google_api_key=config.GOOGLE_API_KEY)

    def embed_documents(self, texts: List[str]) -> List[List[float]]:
        if not texts:
            return []
        return self._retry(lambda: self._client.embed_documents(
            texts, batch_size=50, task_type="RETRIEVAL_DOCUMENT", output_dimensionality=self.dim))

    def embed_query(self, text: str) -> List[float]:
        return self._retry(lambda: self._client.embed_query(
            text, task_type="RETRIEVAL_QUERY", output_dimensionality=self.dim))

    @staticmethod
    def _retry(call, attempts: int = 4):
        """Rate limits (HTTP 429) are normal on the free tier: back off and retry, then give up loudly."""
        delay = 15.0
        for attempt in range(1, attempts + 1):
            try:
                return call()
            except Exception as exc:  # noqa: BLE001
                limited = "429" in str(exc) or "RESOURCE_EXHAUSTED" in str(exc).upper() or "quota" in str(exc).lower()
                if not limited or attempt == attempts:
                    raise
                print(f"    Gemini rate limit; waiting {delay:.0f}s (attempt {attempt}/{attempts - 1})")
                time.sleep(delay)
                delay *= 2
