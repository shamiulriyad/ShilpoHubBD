"""STEP 1 - PDF.

Validate the file before anything else, so a wrong path fails here
instead of halfway through the pipeline.
"""

from pathlib import Path


def load_pdf(pdf_path) -> Path:
    path = Path(pdf_path).expanduser().resolve()

    if not path.exists():
        raise FileNotFoundError(f"PDF not found: {path}")
    if path.suffix.lower() != ".pdf":
        raise ValueError(f"Not a PDF file: {path.name}")
    if path.stat().st_size == 0:
        raise ValueError(f"PDF is empty: {path.name}")

    size_kb = path.stat().st_size / 1024
    print(f"[1] PDF        : {path.name} ({size_kb:.1f} KB)")
    return path
