# data/

The three ShilpoHub heritage datasets the RAG pipeline indexes:

| File | Contents |
|---|---|
| `craft.json` | 18 crafts - production steps, materials, tools, regions with confidence |
| `craftDetails.json` | 55 crafts - descriptions, time required, skill transmission, endangerment |
| `GEO.json` | 21 crafts with geographic associations, plus UNESCO elements, heritage sites, craft families |

```bash
python ingest.py --recreate          # from the rag/ folder
```

- Set `DATA_DIR` in `.env` to read the files from somewhere else.
- The same craft can appear in more than one file; each file's record is kept and they are
  linked by a shared `craft_id`.
- Add or edit records in the JSON, then re-run `python ingest.py --recreate` (plain
  `python ingest.py` also works for edits and additions, but will not remove deleted records).
