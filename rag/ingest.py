"""Ingestion pipeline - steps 1 to 6.

    python ingest.py                       # uses PDF_PATH from .env
    python ingest.py --pdf data/book.pdf
    python ingest.py --pdf data/book.pdf --recreate
"""

import argparse

import config
from rag.step01_load_pdf import load_pdf
from rag.step02_extract_text import extract_text
from rag.step03_clean_text import clean_documents
from rag.step04_chunking import chunk_documents
from rag.step05_embedding import get_embeddings
from rag.step06_vector_store import store_chunks


def main() -> None:
    parser = argparse.ArgumentParser(description="Index a PDF into Qdrant.")
    parser.add_argument("--pdf", default=str(config.PDF_PATH))
    parser.add_argument("--recreate", action="store_true",
                        help="drop the collection first (use after changing the model or PDF)")
    args = parser.parse_args()

    print("--- INGEST -------------------------------------------------")
    pdf_path = load_pdf(args.pdf)                                   # 1
    pages = extract_text(pdf_path)                                  # 2
    pages = clean_documents(pages)                                  # 3
    chunks = chunk_documents(pages, config.CHUNK_SIZE, config.CHUNK_OVERLAP)  # 4
    embeddings = get_embeddings()                                   # 5
    store_chunks(chunks, embeddings, config.COLLECTION_NAME, recreate=args.recreate)  # 6

    print("\nDone. Now ask something:  python ask.py \"your question\"")


if __name__ == "__main__":
    main()
