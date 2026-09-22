"""STEP 2 - Text extraction.

One LangChain Document per page, with the 1-based page number in metadata -
that number is what step 11 cites.

Extraction goes through PyMuPDF plus `pdf_font_repair`: some PDFs embed subset
fonts with no ToUnicode map, and every extractor (pypdf included) then returns
scrambled glyph codes like `& ? ! & &`. `repair_page` undoes that known
substitution cipher; for a normal PDF it is a pass-through and changes nothing.

After extraction every page is scored (alphabetic ratio + English-word ratio).
Pages that still look like garbage are reported, not silently embedded - that is
the point where OCR would be the next step.
"""

from pathlib import Path
from typing import List

import pymupdf
from langchain_core.documents import Document

from rag.pdf_font_repair import repair_page, text_quality, is_readable


def extract_text(pdf_path: Path) -> List[Document]:
    doc = pymupdf.open(str(pdf_path))

    pages: List[Document] = []
    readable = corrupted = blank = 0
    worst: List[tuple] = []

    for index in range(doc.page_count):
        page = doc[index]
        text = repair_page(page)
        page_number = index + 1

        if len(text.strip()) < 20:
            blank += 1
        elif is_readable(text):
            readable += 1
        else:
            corrupted += 1
            alpha_ratio, word_ratio = text_quality(text)
            worst.append((round(word_ratio, 2), round(alpha_ratio, 2), page_number))

        pages.append(Document(
            page_content=text,
            metadata={
                "source_file": pdf_path.name,
                "page": index,
                "page_number": page_number,
            },
        ))

    doc.close()

    total_chars = sum(len(p.page_content) for p in pages)
    print(f"[2] Extracted  : {len(pages)} pages, {total_chars:,} characters")
    print(f"    quality    : {readable} readable, {corrupted} corrupted, {blank} blank")

    if corrupted:
        worst.sort()
        preview = ", ".join(f"p{pn}" for _, _, pn in worst[:10])
        print(f"    ! {corrupted} page(s) still look corrupted after font repair: {preview}")
        print("      If these hold real content, add OCR (pytesseract / ocrmypdf) here.")

    if total_chars < 100:
        print("    ! Almost no text found - this PDF is probably a scan. Add OCR "
              "before this pipeline can work.")

    return pages
