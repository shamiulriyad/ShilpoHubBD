"""STEP 11 - Answer + sources."""

from typing import List, Tuple

from langchain_core.documents import Document


def format_sources(scored_docs: List[Tuple[Document, float]], preview_chars: int = 160) -> str:
    lines = []
    for i, (doc, score) in enumerate(scored_docs, start=1):
        page = doc.metadata.get("page_number", "?")
        source = doc.metadata.get("source_file", "document")
        preview = " ".join(doc.page_content[:preview_chars].split())
        lines.append(f"  [S{i}] {source}, page {page}  (score {score:.3f})\n       {preview}...")
    return "\n".join(lines)


def present(question: str, answer: str, scored_docs: List[Tuple[Document, float]]) -> dict:
    print("\n" + "=" * 72)
    print(f"Q: {question}")
    print("-" * 72)
    print(answer)
    print("-" * 72)
    print("Sources:")
    print(format_sources(scored_docs))
    print("=" * 72)

    return {
        "question": question,
        "answer": answer,
        "sources": [
            {
                "marker": f"S{i}",
                "page": doc.metadata.get("page_number"),
                "file": doc.metadata.get("source_file"),
                "score": float(score),
                "text": doc.page_content,
            }
            for i, (doc, score) in enumerate(scored_docs, start=1)
        ],
    }
