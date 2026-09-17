"""Remember which embedding model built each Qdrant collection.

Vectors from two different embedding models are not comparable, and a different
model almost always means a different vector size. So after changing
EMBEDDING_PROVIDER or EMBEDDING_MODEL in .env, every existing collection must be
rebuilt (recreate=True) before it can be appended to or queried again.

One collection per Knowledge Base (multi-tenant) means one meta file per
collection, under `.rag_index_meta/<collection_name>.json` (git-ignored),
recording the provider/model/dimension used for that collection's last ingest.
"""

import json
import re
from pathlib import Path
from typing import Optional

import config

META_DIR: Path = config.BASE_DIR / ".rag_index_meta"

_SAFE_NAME_RE = re.compile(r"[^A-Za-z0-9_.-]+")


def _meta_path(collection_name: str) -> Path:
    safe = _SAFE_NAME_RE.sub("_", collection_name)
    return META_DIR / f"{safe}.json"


def _current(collection_name: str) -> dict:
    return {
        "provider": config.EMBEDDING_PROVIDER,
        "model": config.EMBEDDING_MODEL,
        "collection": collection_name,
    }


def load(collection_name: str) -> Optional[dict]:
    path = _meta_path(collection_name)
    if not path.exists():
        return None
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (ValueError, OSError):
        return None


def save(collection_name: str, dimension: int) -> None:
    META_DIR.mkdir(parents=True, exist_ok=True)
    data = _current(collection_name)
    data["dimension"] = dimension
    _meta_path(collection_name).write_text(json.dumps(data, indent=2), encoding="utf-8")


def _mismatch(collection_name: str, saved: dict) -> Optional[str]:
    now = _current(collection_name)
    if (saved.get("provider"), saved.get("model")) != (now["provider"], now["model"]):
        return (
            f"the '{collection_name}' collection was built with "
            f"{saved.get('provider')}/{saved.get('model')}, but .env now says "
            f"{now['provider']}/{now['model']}"
        )
    return None


def check_before_ingest(collection_name: str, recreate: bool) -> None:
    """Block an append that would mix two embedding models in one collection."""
    saved = load(collection_name)
    if saved is None or recreate:
        return
    problem = _mismatch(collection_name, saved)
    if problem:
        raise RuntimeError(
            f"Embedding model changed - {problem}. Re-ingest with recreate=true to "
            "rebuild this Knowledge Base's collection with the new model."
        )


def check_before_query(collection_name: str) -> None:
    """Stop if the stored index and .env disagree before answering a question."""
    saved = load(collection_name)
    if saved is None:
        return
    problem = _mismatch(collection_name, saved)
    if problem:
        raise RuntimeError(
            f"Embedding model changed - {problem}. Re-ingest this Knowledge Base's "
            "documents with the new model before asking questions."
        )
