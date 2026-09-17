"""Liveness/readiness helpers for GET /health."""

from rag.step06_vector_store import get_client


def qdrant_is_healthy() -> bool:
    try:
        get_client().get_collections()
        return True
    except Exception:  # noqa: BLE001 - any failure means "not healthy"
        return False
