"""STEP 3 - Text cleaning.

PDF text arrives with hard line breaks, hyphen-split words, repeated
headers/footers and stray page numbers. All of that pollutes the embeddings,
so it gets removed before chunking.
"""

import re
from collections import Counter
from typing import List

from langchain_core.documents import Document

_CONTROL_CHARS = re.compile(r"[\x00-\x08\x0b\x0c\x0e-\x1f]")
_HYPHEN_BREAK = re.compile(r"(\w)-\n(\w)")
_SINGLE_NEWLINE = re.compile(r"(?<![.!?:;।])\n(?![\n\-\u2022\d])")
_PAGE_NUMBER_LINE = re.compile(r"^\s*(page\s*)?[\divxlIVXL]{1,4}\s*$", re.MULTILINE | re.IGNORECASE)
_SPACES = re.compile(r"[ \t]{2,}")
_BLANK_LINES = re.compile(r"\n{3,}")


def clean_text(text: str) -> str:
    text = text.replace("\u00ad", "")            # soft hyphen
    text = text.replace("\ufeff", "")            # BOM
    text = _CONTROL_CHARS.sub(" ", text)
    text = _HYPHEN_BREAK.sub(r"\1\2", text)      # "informa-\ntion" -> "information"
    text = _PAGE_NUMBER_LINE.sub("", text)
    text = _SINGLE_NEWLINE.sub(" ", text)        # unwrap mid-sentence line breaks
    text = _SPACES.sub(" ", text)
    text = _BLANK_LINES.sub("\n\n", text)
    return text.strip()


def _find_repeating_lines(docs: List[Document], threshold: float = 0.6) -> set:
    """Lines that show up on most pages are headers/footers, not content."""
    if len(docs) < 4:
        return set()

    counter = Counter()
    for doc in docs:
        for line in {ln.strip() for ln in doc.page_content.splitlines() if ln.strip()}:
            if len(line) < 80:
                counter[line] += 1

    cutoff = len(docs) * threshold
    return {line for line, count in counter.items() if count >= cutoff}


def clean_documents(docs: List[Document], min_chars: int = 50) -> List[Document]:
    boilerplate = _find_repeating_lines(docs)

    cleaned: List[Document] = []
    for doc in docs:
        body = doc.page_content
        for line in boilerplate:
            body = body.replace(line, "")

        body = clean_text(body)
        if len(body) < min_chars:      # blank or near-blank page
            continue

        doc.page_content = body
        cleaned.append(doc)

    dropped = len(docs) - len(cleaned)
    print(f"[3] Cleaned    : {len(cleaned)} pages kept, {dropped} dropped, "
          f"{len(boilerplate)} repeating header/footer lines removed")
    return cleaned
