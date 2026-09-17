"""STEP 4 - Chunking.

Split each page into overlapping chunks. The overlap keeps a sentence that
straddles a boundary retrievable from either side.
"""

from typing import List

from langchain_core.documents import Document
from langchain_text_splitters import RecursiveCharacterTextSplitter

# Bengali danda (।) is included so Bangla PDFs split on sentence ends too.
SEPARATORS = ["\n\n", "\n", "। ", ". ", "? ", "! ", "; ", ", ", " ", ""]


def chunk_documents(docs: List[Document], chunk_size: int, chunk_overlap: int) -> List[Document]:
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap,
        separators=SEPARATORS,
        length_function=len,
        add_start_index=True,
    )

    chunks = splitter.split_documents(docs)

    for i, chunk in enumerate(chunks):
        chunk.metadata["chunk_id"] = i

    avg = sum(len(c.page_content) for c in chunks) / max(len(chunks), 1)
    print(f"[4] Chunked    : {len(chunks)} chunks "
          f"(size={chunk_size}, overlap={chunk_overlap}, avg={avg:.0f} chars)")
    return chunks
