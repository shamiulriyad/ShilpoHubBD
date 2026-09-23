"""All tunable settings in one place. Read from .env, with sane defaults."""

import hashlib
import os
import sys
from pathlib import Path

from dotenv import load_dotenv

BASE_DIR = Path(__file__).resolve().parent
ROOT_ENV = BASE_DIR.parent / ".env"

# rag/.env holds only this service's non-secret settings (embedding model, Qdrant, ...).
load_dotenv(BASE_DIR / ".env")
# The Gemini key has ONE source: the repo-root .env (shared with the .NET backend). override=True
# so a stale Windows/user environment variable of the same name can never win over it.
load_dotenv(ROOT_ENV, override=True)


def _resolve(value, default: Path) -> Path:
    """Turn a path from .env into an absolute one.

    Relative values are anchored to the project folder, not the shell's current
    directory, so `python ingest.py` works no matter where it is run from.
    """
    if not value:
        return default
    path = Path(value).expanduser()
    return path if path.is_absolute() else (BASE_DIR / path)


# --- Step 1: JSON datasets -----------------------------------------------
# The three ShilpoHub heritage datasets, read from DATA_DIR.
DATA_DIR = _resolve(os.getenv("DATA_DIR"), BASE_DIR / "data")
DATASET_FILES = ("craft.json", "craftDetails.json", "GEO.json")
# Legacy: the PDF-era pipeline read this. Kept so an existing .env still loads;
# the JSON pipeline does not use it.
PDF_PATH = _resolve(os.getenv("PDF_PATH"), BASE_DIR / "data" / "sample.pdf")

# --- Step 4: Chunking ----------------------------------------------------
# Each chunk (header included) must stay under this many EMBEDDING-MODEL tokens: all-MiniLM-L6-v2
# reads at most 256 and silently drops the rest, so 200 leaves headroom.
MAX_CHUNK_TOKENS = int(os.getenv("MAX_CHUNK_TOKENS", "200"))
# Legacy from fixed-size chunking - the aspect chunker does not use these two.
# Field-aware chunking: one chunk per topic (overview / materials / how made /
# location / heritage / endangerment). CHUNK_SIZE/CHUNK_OVERLAP only apply to a
# topic too long for one chunk; MIN_CHUNK_CHARS folds a too-small topic into its
# neighbour instead of emitting a useless tiny chunk.
CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", "1000"))
CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", "150"))
MIN_CHUNK_CHARS = int(os.getenv("MIN_CHUNK_CHARS", "200"))

# --- Step 5: Embedding ---------------------------------------------------
# Name kept so the pipeline modules that read config.GOOGLE_API_KEY are unchanged; the value now
# comes only from the root .env's Gemini__ApiKey (the old GOOGLE_API_KEY variable is not read).
GOOGLE_API_KEY = os.getenv("Gemini__ApiKey") or None


def gemini_key_status() -> dict:
    """Whether the Gemini key is loaded -- never the key. `fingerprint` is the first 8 hex chars of
    its SHA-256, enough to compare two services' keys without revealing either."""
    if not GOOGLE_API_KEY:
        return {"loaded": False, "source": str(ROOT_ENV), "fingerprint": None}
    return {"loaded": True, "source": str(ROOT_ENV),
            "fingerprint": hashlib.sha256(GOOGLE_API_KEY.encode("utf-8")).hexdigest()[:8]}

# Which embedding backend to use. Set this in .env - it is never hard-coded.
#   google      : Gemini embeddings (recommended). Needs GOOGLE_API_KEY with
#                 embedding quota. Best for large PDFs on a CPU-only machine.
#   huggingface : a local sentence-transformers model. No API key, no quota,
#                 no cost, but CPU-bound.
EMBEDDING_PROVIDER = os.getenv("EMBEDDING_PROVIDER", "google").strip().lower()

KNOWN_PROVIDERS = ("google", "huggingface")

# Used only when EMBEDDING_MODEL is left unset in .env.
_DEFAULT_MODEL = {
    "google": "gemini-embedding-001",
    "huggingface": "sentence-transformers/all-MiniLM-L6-v2",
}
EMBEDDING_MODEL = (
    os.getenv("EMBEDDING_MODEL", "").strip()
    or _DEFAULT_MODEL.get(EMBEDDING_PROVIDER, _DEFAULT_MODEL["google"])
)

# Gemini embedding models the kit knows about (with or without the "models/"
# prefix the API expects). Anything else is rejected early with a clear message.
KNOWN_GOOGLE_EMBEDDING_MODELS = (
    "gemini-embedding-001",
    "text-embedding-004",
    "embedding-001",
)

# None => detect the real dimension at runtime instead of hard-coding it.
EMBEDDING_DIM = int(os.getenv("EMBEDDING_DIM") or 0) or None
EMBED_BATCH_SIZE = int(os.getenv("EMBED_BATCH_SIZE", "90"))
# Gemini free tier allows ~100 embedded texts / minute; pause between batches so
# a large PDF does not trip a 429. Not needed for a local model.
EMBED_SLEEP = float(os.getenv("EMBED_SLEEP", "60" if EMBEDDING_PROVIDER == "google" else "0"))

# --- Step 6: Qdrant ------------------------------------------------------
# Normal mode: connect to a Qdrant SERVER over HTTP. `docker compose up` starts
# one; for local dev without Docker run `docker run -p 6333:6333 qdrant/qdrant`.
QDRANT_URL = os.getenv("QDRANT_URL", "http://localhost:6333")
QDRANT_API_KEY = os.getenv("QDRANT_API_KEY") or None
_qdrant_path = os.getenv("QDRANT_PATH")
# Optional fallback: embedded on-disk store (no server). Only the `python
# ingest.py` CLI can write to it - a running service, and therefore the UI
# upload, need server mode. Leave QDRANT_PATH empty for normal mode.
QDRANT_PATH = str(_resolve(_qdrant_path, BASE_DIR / "qdrant_data")) if _qdrant_path else None
COLLECTION_NAME = os.getenv("COLLECTION_NAME", "shilpohub")

# --- Upload (POST /ingest via the UI) ----------------------------------
# Sensible configurable ceiling - not unlimited. The .NET layer enforces the
# same number for the browser -> backend hop (Upload__MaxBytes).
MAX_UPLOAD_MB = int(os.getenv("MAX_UPLOAD_MB", "200"))
# Legacy (PDF-era scanned-document check); unused by the JSON pipeline.
MIN_TEXT_CHARS = int(os.getenv("MIN_TEXT_CHARS", "200"))

# Sparse (BM25) vectors for hybrid search, made with fastembed. Runs locally on ONNX; the
# model files (stopwords etc., a few KB) are fetched once on first use.
SPARSE_MODEL = os.getenv("SPARSE_MODEL", "Qdrant/bm25")

# Prompt templates read at query time (step 7: query analysis, step 10: answer).
PROMPTS_DIR = _resolve(os.getenv("PROMPTS_DIR"), BASE_DIR / "prompts")

# --- Step 9: Retrieval ---------------------------------------------------
# Fallback only: each question type has its own top_k (see step 9); this is used when the
# question analysis fails and the question is answered without a type.
TOP_K = int(os.getenv("TOP_K", "4"))
# Chunks scoring below this cosine similarity are treated as "not in the knowledge
# base" and dropped before generation, so an off-topic question reaches Gemini
# with no context (and is refused) rather than with the least-bad chunks.
MIN_RELEVANCE_SCORE = float(os.getenv("MIN_RELEVANCE_SCORE", "0.35"))

# --- Step 10: Gemini LLM -------------------------------------------------
LLM_MODEL = os.getenv("LLM_MODEL", "gemini-2.5-flash")
TEMPERATURE = float(os.getenv("TEMPERATURE", "0.2"))


def require_api_key() -> None:
    if not GOOGLE_API_KEY:
        raise RuntimeError(
            "Gemini__ApiKey is not set. Put it in the repo-root .env (the same file the backend "
            "uses): Gemini__ApiKey=your_key (https://aistudio.google.com/app/apikey)."
        )


def google_embedding_model_name() -> str:
    """The model id the Google API expects, i.e. with a 'models/' prefix."""
    name = EMBEDDING_MODEL
    return name if name.startswith("models/") else f"models/{name}"


def validate_embedding_config() -> None:
    """Fail early, with a clear message, on a bad embedding setup in .env.

    Called at the start of step 5 so a typo in EMBEDDING_PROVIDER / EMBEDDING_MODEL
    surfaces here instead of as a deep library traceback later.
    """
    if EMBEDDING_PROVIDER not in KNOWN_PROVIDERS:
        raise RuntimeError(
            f"EMBEDDING_PROVIDER='{EMBEDDING_PROVIDER}' is not valid. "
            f"Use one of: {', '.join(KNOWN_PROVIDERS)} (set it in .env)."
        )

    if EMBEDDING_PROVIDER == "google":
        require_api_key()
        bare = EMBEDDING_MODEL
        if bare.startswith("models/"):
            bare = bare[len("models/"):]
        if bare not in KNOWN_GOOGLE_EMBEDDING_MODELS:
            raise RuntimeError(
                f"EMBEDDING_MODEL='{EMBEDDING_MODEL}' is not a known Gemini "
                f"embedding model. Try one of: "
                f"{', '.join(KNOWN_GOOGLE_EMBEDDING_MODELS)}. "
                "Recommended: gemini-embedding-001."
            )


def ensure_utf8_console() -> None:
    """Make print() safe for Bangla text.

    The datasets contain Bangla names, and a Windows console defaults to cp1252,
    which raises UnicodeEncodeError on the first one. Called from the CLI entry
    points; characters the console still cannot show become '?' rather than a crash.
    """
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8", errors="replace")
        except (AttributeError, ValueError):
            pass  # not a text stream that can be reconfigured (e.g. redirected in a test)
