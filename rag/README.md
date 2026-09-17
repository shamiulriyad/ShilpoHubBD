# PDF RAG - LangChain + Qdrant + Gemini

Every step in the pipeline is its own module, in order.

```
1  PDF                    ->  rag/step01_load_pdf.py
2  Text extraction        ->  rag/step02_extract_text.py  (+ rag/pdf_font_repair.py)
3  Text cleaning          ->  rag/step03_clean_text.py
4  Chunking               ->  rag/step04_chunking.py
5  Embedding              ->  rag/step05_embedding.py
6  Store in Qdrant        ->  rag/step06_vector_store.py
                              (steps 1-6 run from ingest.py)
7  User question          ->  rag/step07_user_question.py
8  Query embedding        ->  rag/step08_query_embedding.py
9  Retrieve chunks        ->  rag/step09_retrieve.py
10 Gemini                 ->  rag/step10_generate.py
11 Answer + sources       ->  rag/step11_answer.py
                              (steps 7-11 run from ask.py)
```

## Layout

```
rag/
├── config.py            all settings, read from .env
├── ingest.py            CLI: run steps 1-6 over a PDF
├── ask.py               CLI: run steps 7-11 (one-shot or interactive)
├── main.py              same query pipeline over HTTP (FastAPI) for the .NET backend
├── requirements.txt
├── .env.example         copy to .env
├── data/                put your PDFs here (git-ignored)
├── rag/                 one module per pipeline step
│   ├── step01_load_pdf.py … step11_answer.py
│   ├── pdf_font_repair.py   fixes broken subset-font encodings (used by step 2)
│   └── index_meta.py       records which embedding model built the collection
├── tests/
│   └── test_bge.py
└── qdrant_data/         embedded Qdrant store, created by ingest.py (git-ignored)
```

## Setup

Use **Python 3.11 or 3.12** - `torch` / `sentence-transformers` (the local
embedding path) have no wheels for 3.13+ yet.

```bash
python3.11 -m venv venv && source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
cp .env.example .env                                 # then paste your Gemini API key
```

**Qdrant runs as a server** in normal mode (`QDRANT_URL` in `.env.example`,
`QDRANT_PATH` empty) - this is what lets the UI upload index a PDF. Start one:

```bash
docker run -p 6333:6333 -v qdrant_storage:/qdrant/storage qdrant/qdrant
```

No Docker? Set `QDRANT_PATH=qdrant_data` for an **embedded on-disk** store - no
server, but only `python ingest.py` can write to it (a running service can't), so
the UI upload returns a clear 501. See [`../docs/qdrant.md`](../docs/qdrant.md).

## Run

```bash
python ingest.py --pdf data/your.pdf --recreate
python ask.py "what does chapter 3 say about X?"
python ask.py                                        # interactive
```

Or serve the same pipeline over HTTP (what the .NET backend calls):

```bash
uvicorn main:app --port 8000        # GET /health, POST /query, POST /ingest?filename=x.pdf
```

## Embeddings

Set the provider and model in `.env` only - nothing is hard-coded, and the
vector size is detected automatically.

| Goal | `EMBEDDING_PROVIDER` | `EMBEDDING_MODEL` | Notes |
|------|--------------------|------------------|-------|
| **Recommended default** | `google` | `gemini-embedding-001` | Needs `GOOGLE_API_KEY`. 3072-dim. Best for large PDFs on a CPU-only machine. |
| Local, fast, English | `huggingface` | `sentence-transformers/all-MiniLM-L6-v2` | No API key. 384-dim. CPU handles a few-hundred-page PDF in a minute or two. |
| Local, multilingual (incl. Bangla) | `huggingface` | `BAAI/bge-m3` | No API key. 1024-dim. Much heavier - minutes to tens of minutes on CPU. |

**Why Gemini for a large PDF on a CPU-only machine:** a local model embeds every
chunk on the CPU, which is slow and memory-hungry for a big document; the Gemini
API does it remotely in batches, so ingestion stays fast without a GPU.

**After changing `EMBEDDING_PROVIDER` or `EMBEDDING_MODEL` you must re-ingest
with `--recreate`.** Vectors from two models are not comparable and usually have
different sizes, so the old collection is unusable. The kit records which model
built the collection (`.rag_index_meta.json`, git-ignored) and stops with a clear
message if `.env` no longer matches:

```bash
python ingest.py --recreate
```

Invalid values are also caught early: an unknown `EMBEDDING_PROVIDER`, an unknown
Gemini embedding model, a missing `GOOGLE_API_KEY`, an embedding-API failure, and
a Qdrant vector-size mismatch each raise a one-line explanation instead of a
library traceback.

## Notes

- **Vector dimension is detected, not hard-coded.** `gemini-embedding-001`
  returns 3072 dims by default, and older tutorials assume 768. Step 5 embeds a
  probe string and creates the Qdrant collection with whatever size comes back.
- **Re-index after changing the embedding model or the PDF.** Vectors from two
  different models are not comparable. Run `python ingest.py --recreate` - in
  embedded mode it wipes `qdrant_data/collection/<name>` first, so the collection
  is genuinely rebuilt (without `--recreate` a re-run *appends* and you get
  duplicate chunks).
- **Local embeddings are CPU-bound.** `all-MiniLM-L6-v2` handles a few-hundred-page
  PDF in a minute or two; `bge-m3` is far heavier (minutes to tens of minutes) but
  much better for Bangla. For a very large PDF, prefer a small model, a GPU, or
  `EMBEDDING_PROVIDER=google`.
- **Scanned PDFs will not work.** Step 2 warns you if a page yields no text.
  Add OCR (pytesseract / ocrmypdf) before step 3 in that case.
- **Broken font encodings.** Some PDFs embed subset fonts with no `ToUnicode`
  map, so every extractor returns scrambled glyphs (`& ? ! & &`). Step 2 runs
  through `rag/pdf_font_repair.py`, which reverses that substitution cipher, and
  then scores every page (`readable / corrupted / blank`). For a normal PDF the
  repair is a no-op. If pages stay `corrupted`, the maps in `pdf_font_repair.py`
  are PDF-specific and OCR is the fallback.
- **Tuning retrieval.** `CHUNK_SIZE` 800-1200 suits prose; drop to 400-600 for
  dense tables and specs. Raise `TOP_K` if answers miss context, lower it if
  Gemini starts drifting off-topic.
- **Grounding.** Step 10 forbids outside knowledge and requires `[S1]` markers,
  so an answer without a marker is a red flag worth checking against the
  sources printed in step 11.
