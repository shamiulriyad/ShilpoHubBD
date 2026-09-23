# ShilpoHub Heritage RAG - LangChain + Qdrant + Gemini

Answers questions about Bangladeshi traditional crafts from three JSON datasets
(`craft.json`, `craftDetails.json`, `GEO.json` in `data/`), grounded in the retrieved
text and returned with its sources. Questions may be in English, Bangla or romanised Bangla.

```
INGEST  (python ingest.py)
1  Load JSON              rag/step01_load_json.py        the 3 files, validated
2  Normalize JSON         rag/step02_normalize_json.py   3 schemas -> 1 doc format; craft_key, districts,
                                                          divisions, source_ids, is_unesco, is_gi
3  Clean data             rag/step03_clean_data.py       nulls/dupes/whitespace; category_norm, risk_level;
                                                          text per aspect
4  Aspect chunking        rag/step04_chunking.py         overview / production / geography, < 200 tokens
5  Embedding              rag/step05_embedding.py        dense: all-MiniLM-L6-v2 | sparse: BM25 (fastembed)
6  Store in Qdrant        rag/step06_vector_store.py     dense + bm25 vectors + metadata, payload indexes

QUERY   (python ask.py "...")            orchestrated by rag/pipeline.py
7  User question          rag/step07_user_question.py    Step A: Gemini -> type, filters, english_query
8  Query embedding        rag/step08_query_embedding.py  embeds the ENGLISH query (dense + BM25)
9  Route + retrieve       rag/step09_retrieve.py         per-type filters, hybrid search, expansion, fallback
10 Gemini                 rag/step10_generate.py         Step D: grounded answer (prompts/answer_prompt.txt)
11 Answer + sources       rag/step11_answer.py           {"answer", "question_type", "sources": [...]}
```

## Layout

```
rag/
├── config.py            all settings, read from .env
├── ingest.py            CLI: steps 1-6 over the JSON datasets
├── ask.py               CLI: steps 7-11 (one-shot or interactive)
├── main.py              the same query pipeline over HTTP (FastAPI)
├── prompts/
│   ├── query_analysis.txt   Step A prompt: question -> JSON (type, filters, english_query)
│   └── answer_prompt.txt    Step D prompt: {context} {question} {question_type} {language}
├── rag/                 one module per step, plus:
│   ├── craft_keys.py        the craft_key alias map (one key per craft across the 3 files)
│   ├── pipeline.py          steps 7-11 in order - shared by ask.py, the API and the eval
│   ├── prompts.py           loads/renders prompt templates, retries rate-limited LLM calls
│   ├── index_meta.py        records which embedding model built the collection
│   └── pdf_font_repair.py   legacy from the PDF pipeline - no longer used
├── eval/
│   ├── questions.json       53 questions: English / Bangla / romanised Bangla, every question type
│   └── run_eval.py          detected type, filters, retrieved craft_keys, HIT/MISS, answer, summary
├── tests/test_json_pipeline.py   47 offline tests (no model, Gemini or on-disk Qdrant)
└── data/                craft.json, craftDetails.json, GEO.json
```

## Setup

Use **Python 3.11 or 3.12** (`torch` / `sentence-transformers` have no wheels for 3.13+ yet).

```bash
python3.11 -m venv venv && source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt                      # includes fastembed (BM25)
cp .env.example .env                                 # then paste your Gemini API key
```

Qdrant runs as a **server** in normal mode (`docker run -p 6333:6333 qdrant/qdrant`). Without
Docker set `QDRANT_PATH=qdrant_data` for an **embedded** store: no server, but only the CLIs
can use it (a running service cannot write to it, so `POST .../ingest` returns 501).
Embedded mode ignores payload indexes; a server gets all of them.

## Run

```bash
python ingest.py --recreate                          # build the "shilpohub" collection
python ask.py "Which crafts are associated with Narayanganj?"
python ask.py "শীতল পাটি কীভাবে তৈরি হয়?"              # Bangla in, Bangla out
python ask.py                                        # interactive

python eval/run_eval.py                              # full eval (2 Gemini calls per question)
python eval/run_eval.py --no-answer                  # retrieval only
python -m unittest tests.test_json_pipeline -v       # offline tests
uvicorn main:app --port 8000                         # HTTP: POST /api/kb/{collection}/query
```

Re-running `ingest.py` without `--recreate` is safe (chunk ids are stable, so the same data
overwrites itself) but does not remove records you deleted from the JSON - use `--recreate`.

## How a question is answered

1. **Step A - analysis.** Gemini reads the question with `prompts/query_analysis.txt` and returns
   JSON: `question_type`, `craft_keys`, `districts`, `divisions`, `category_norm`, `risk_levels`,
   `is_unesco`, `is_gi`, `language`, `english_query`. The JSON is validated against what is really
   in the index (unknown craft keys dropped, "Chittagong" -> "Chattogram"); if the call or the
   parse fails, the question is still answered as an unfiltered search on the raw question.
2. **Embed the `english_query`** - the embedding model is English-only, so a Bangla question is
   translated in step A and never embedded as Bangla.
3. **Route** by `question_type`. Every search is hybrid (dense + BM25, fused with RRF) under a
   metadata filter:

   | type | filter | aspects | top_k | extras |
   |---|---|---|---|---|
   | describe | craft_key | overview, geography | 6 | + sibling chunks from the other files (max 10) |
   | how_made, materials_tools | craft_key | production | 6 | |
   | craft_location | craft_key | geography | 6 | + siblings |
   | location_crafts | districts, else divisions | geography | 25 | grouped by craft |
   | status | is_unesco / is_gi / risk_level (+ craft, category) | overview for UNESCO/GI, geography for risk | 20 | + REF-UNESCO for UNESCO questions |
   | list | category_norm | overview | 25 | + REF-TAXONOMY, grouped by craft |
   | compare | each craft separately | any | 6 each | heading per craft, + siblings |
   | time | craft_key | production | 6 | always includes "Time required" from **both** craft.json and craftDetails.json |
   | heritage | craft_key if named | any | 8 | + REF-SITES |
   | out_of_scope | - | - | - | no retrieval; fixed reply |

   A filtered search that finds nothing is retried once without filters. Hits found without a
   relevance filter (no filter, an aspect-only filter, or after that fallback) must reach
   `MIN_RELEVANCE_SCORE` in **dense cosine** (RRF scores are ranks, so each hit is re-scored) or
   they are dropped; if nothing is left the answer is "not available".
4. **Answer** with `prompts/answer_prompt.txt`: only the retrieved context, confidence and
   uncertainty respected, conflicting sources attributed, no outside knowledge. If the context
   does not contain the answer the model says `NOT_AVAILABLE` and the fixed reply (English or
   Bangla) is returned with no sources.
5. **Response** - `{"answer", "question_type", "sources": [{"source_file", "doc_id", "source_ids"}]}`.
   `answer` has the `[Doc N]` citations removed; `sources` lists the documents actually cited.

## The data model

Every craft record becomes up to three chunks, each starting with
`Craft: <name_en> (<name_bn>) | Source: <file> | Aspect: <aspect>`; an aspect that does not fit
200 tokens (counted with the embedding model's own tokenizer, header included) is split at
natural seams. The four reference documents (`REF-UNESCO`, `REF-SITES`, `REF-TAXONOMY`,
`REF-SOURCES`) come from `GEO.json`; its 64 empty `district_records` are not indexed.

Metadata on every chunk (under `metadata.` in the Qdrant payload): `doc_id`, `doc_type`,
`aspect`, `source_file`, `craft_key`, `name_en`, `name_bn`, `category_norm`, `districts[]`,
`divisions[]`, `confidences[]`, `source_ids[]`, `risk_level`, `is_unesco`, `is_gi`.

- **`craft_key`** - one key per craft across the three files (`rag/craft_keys.py`); craftDetails.json
  ids are the canonical keys, other files' names are folded in. Judgement calls are listed there.
- **`category_norm`** - one of 12 categories. A label like "A. Handicrafts" is too broad, so the
  craft's name decides (Shital Pati -> Natural Fibre Crafts, Wooden Crafts -> Wood Crafts).
- **`risk_level`** - keyword-based on the free-text `endangerment_level`; the earliest keyword wins
  ("Stable as a broad category rather than a single *endangered* craft" is `stable`). The original
  text stays in the chunk.
- **`is_unesco`** - from GEO.json's UNESCO list only ("pending files" is not inscribed).
  **`is_gi`** - "GI" in notes / heritage status / description. Both are set on every record of the craft.
- **`districts`** - structured regions, plus the 64 district names matched inside craftDetails.json's
  prose regions (old spellings mapped); **`divisions`** are derived from them.

## Embeddings

The dense model is set in `.env` and not changed by this pipeline (`EMBEDDING_PROVIDER`,
`EMBEDDING_MODEL`; the vector size is detected). Sparse BM25 vectors use fastembed's
`Qdrant/bm25` (`SPARSE_MODEL`), run locally on ONNX, and need a one-time download of a few KB.

| Goal | `EMBEDDING_PROVIDER` | `EMBEDDING_MODEL` | Notes |
|------|--------------------|------------------|-------|
| Local, fast, English (current) | `huggingface` | `sentence-transformers/all-MiniLM-L6-v2` | 384-dim, 256-token limit, English only |
| Local, multilingual | `huggingface` | `BAAI/bge-m3` | 1024-dim, much heavier |
| Gemini | `google` | `gemini-embedding-001` | needs `Gemini__ApiKey` (repo-root `.env`), 3072-dim |

After changing either variable re-ingest with `--recreate`: vectors from two models are not
comparable, and `index_meta.py` stops with a clear message if `.env` no longer matches the index.

## Notes and limits

- **Bangla works through translation.** Step A translates the question; retrieval is English. The
  answer is written in Bangla, but the model's Bangla is only as good as Gemini Flash-Lite's.
- **The analysis call is a single point of failure for routing.** If it fails, the question is
  still answered, but without filters, so precision drops.
- **Large sets are capped.** `location_crafts` retrieves 25 chunks; a district with 15 crafts
  (Rajshahi) can exceed that, so the answer may not list every craft.
- **A district with no data** (Barguna) is retried without filters; the answer prompt then has
  to see that nothing in the context is tied to that place and say so.
- **Gemini rate limits.** LLM calls are retried after 10 / 30 / 60 s on a 429; use `--delay`
  with the eval on a free-tier key.
- `TOP_K` is now only a fallback for an unrecognised question type; each type has its own top_k.
