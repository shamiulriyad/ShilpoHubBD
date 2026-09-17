# data/

Drop the PDF(s) you want to index here. This folder is git-ignored
(`data/*.pdf` in [.gitignore](../.gitignore)) so your files never get committed.

## Usage

```bash
python ingest.py --pdf data/your-file.pdf --recreate
```

- `--recreate` on the **first** ingest of a fresh copy of this project — it
  wipes any old Qdrant collection before writing, so nothing from a previous
  PDF lingers.
- Drop `--recreate` on later runs if you're intentionally appending more
  documents to the same collection.
- Or set `PDF_PATH=data/your-file.pdf` in `.env` and just run `python ingest.py`
  with no flags.

## Notes

- One file per `ingest.py` run. For multiple documents in one Knowledge Base,
  use the HTTP API (`main.py` → `POST /api/kb/{collection}/ingest`), which
  appends per-document instead.
- Scanned/image-only PDFs are rejected (`MIN_TEXT_CHARS` in `.env`) — OCR them
  first if extraction comes back empty.
- Sample PDFs used during development live in `../../samples/` (outside this
  project folder), not here — keep this folder limited to the data you're
  actually indexing.
