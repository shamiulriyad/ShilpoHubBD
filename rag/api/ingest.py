"""Orchestrates steps 1-6 for one document, scoped to one Knowledge Base's Qdrant
collection. Called by api routes in main.py - holds the multi-tenant wiring, not
pipeline logic (that stays in rag/rag/step0N_*.py, reused as-is)."""

from pathlib import Path
from typing import Dict

import config
from rag.step01_load_pdf import load_pdf
from rag.step02_extract_text import extract_text
from rag.step03_clean_text import clean_documents
from rag.step04_chunking import chunk_documents
from rag.step06_vector_store import store_chunks


class ScannedPdfError(ValueError):
    """Raised when a PDF has little/no extractable text (spec: scanned PDFs need OCR)."""


def ingest_document(
    *, collection_name: str, document_id: str, pdf_path: Path, filename: str,
    chunk_size: int, chunk_overlap: int, embeddings,
) -> Dict[str, int]:
    """Extract, clean, chunk, embed and store one PDF into `collection_name`.

    Appends onto the collection (recreate=False) - a Knowledge Base holds many
    documents, so a new upload must not erase the others. Each chunk is tagged
    with `document_id` so it can be deleted independently later (see
    step06_vector_store.delete_document).
    """
    pages = clean_documents(extract_text(load_pdf(pdf_path)))          # 1-3
    extractable = sum(len(p.page_content) for p in pages)
    if not pages or extractable < config.MIN_TEXT_CHARS:
        raise ScannedPdfError(
            "This PDF appears to be scanned/image-based. OCR is required before indexing."
        )

    chunks = chunk_documents(pages, chunk_size, chunk_overlap)          # 4
    for chunk in chunks:
        chunk.metadata["document_id"] = document_id
        chunk.metadata["source_file"] = filename

    store_chunks(chunks, embeddings, collection_name, recreate=False)   # 5 (embeddings passed in) + 6

    return {"pages": len(pages), "chunks": len(chunks)}
