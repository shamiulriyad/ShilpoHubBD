"""STEP 5 - Embedding.

One embedding object is shared by ingestion (step 6) and querying (step 8) -
the same model must be used on both sides or the vectors are not comparable.

Two kinds of vector are made for every chunk:
  dense  : get_embeddings()         - the semantic vector (all-MiniLM-L6-v2, unchanged)
  sparse : get_sparse_embeddings()  - BM25 term weights (fastembed "Qdrant/bm25"), which
           catch exact words the dense model blurs (a rare place name, "GI", "Tangail")
Step 6 stores both; step 9 searches both and fuses the rankings (hybrid search).

Two providers, chosen by EMBEDDING_PROVIDER in .env:
  google      : Gemini embeddings (recommended; needs an API key with quota)
  huggingface : a local sentence-transformers model, no API key, no quota
"""

import config


def get_embeddings():
    """Build the embedding client for the provider named in .env.

    Raises a clear RuntimeError for a bad provider / model / missing key instead
    of letting a deep library traceback surface.
    """
    config.validate_embedding_config()

    if config.EMBEDDING_PROVIDER == "huggingface":
        try:
            from langchain_huggingface import HuggingFaceEmbeddings

            embeddings = HuggingFaceEmbeddings(model_name=config.EMBEDDING_MODEL)
        except ImportError as exc:
            raise RuntimeError(
                "Local embeddings need the optional packages. Install them with:\n"
                "    pip install -r requirements-local.txt\n"
                "or switch to EMBEDDING_PROVIDER=google in .env (no extra install)."
            ) from exc
        except Exception as exc:  # noqa: BLE001 - re-raised with context
            raise RuntimeError(
                f"Could not load local embedding model '{config.EMBEDDING_MODEL}'. "
                "Check the name on https://huggingface.co/models, or switch to "
                f"EMBEDDING_PROVIDER=google in .env.\nOriginal error: {exc}"
            ) from exc
        print(f"[5] Embedding  : {config.EMBEDDING_MODEL} (local)")
        return embeddings

    from langchain_google_genai import GoogleGenerativeAIEmbeddings

    try:
        embeddings = GoogleGenerativeAIEmbeddings(
            model=config.google_embedding_model_name(),
            google_api_key=config.GOOGLE_API_KEY,
        )
    except Exception as exc:  # noqa: BLE001 - re-raised with context
        raise RuntimeError(
            f"Could not initialise Gemini embeddings ('{config.EMBEDDING_MODEL}').\n"
            f"Original error: {exc}"
        ) from exc
    print(f"[5] Embedding  : {config.EMBEDDING_MODEL} (Gemini)")
    return embeddings


def detect_dimension(embeddings) -> int:
    """Ask the model for one vector and measure it.

    Safer than hard-coding a size: it changes with the model (384 for
    all-MiniLM-L6-v2, 1024 for bge-m3, 3072 for gemini-embedding-001), and a
    mismatch only shows up later as a Qdrant dimension error.
    """
    if config.EMBEDDING_DIM:
        return config.EMBEDDING_DIM

    try:
        dim = len(embeddings.embed_query("dimension probe"))
    except Exception as exc:  # noqa: BLE001 - re-raised with context
        raise RuntimeError(
            "The embedding model could not be reached. For Gemini, check that "
            "GOOGLE_API_KEY is valid and has embedding quota; for a local model, "
            "check the connection used for the first-run download.\n"
            f"Original error: {exc}"
        ) from exc
    print(f"    vector size: {dim}")
    return dim


def get_sparse_embeddings():
    """BM25 sparse embedder for hybrid search. Runs locally (ONNX); the small model files
    (stopwords etc.) are downloaded once on first use."""
    try:
        from fastembed import SparseTextEmbedding
    except ImportError as exc:
        raise RuntimeError("Hybrid search needs fastembed. Install it with:  pip install fastembed") from exc

    try:
        model = SparseTextEmbedding(config.SPARSE_MODEL)
    except Exception as exc:  # noqa: BLE001 - re-raised with context
        raise RuntimeError(
            f"Could not load the sparse model '{config.SPARSE_MODEL}' (needs a one-time download "
            f"from Hugging Face).\nOriginal error: {exc}"
        ) from exc
    print(f"[5] Sparse     : {config.SPARSE_MODEL} (BM25, local)")
    return model
