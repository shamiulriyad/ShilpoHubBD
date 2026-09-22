"""RAG pipeline, one module per step of the pipeline.

1  step01_load_json       Load craft.json, craftDetails.json, GEO.json
2  step02_normalize_json  Normalize 3 schemas into one document format (+ craft_key, districts ...)
3  step03_clean_data      Clean values; category_norm, risk_level; text per aspect
4  step04_chunking        Aspect chunks (overview / production / geography), < 200 tokens
5  step05_embedding       Embedding models: dense (MiniLM) and sparse (BM25)
6  step06_vector_store    Store dense + sparse vectors + metadata in Qdrant
7  step07_user_question   User question; Step A: Gemini query analysis -> JSON
8  step08_query_embedding Embed the English query (dense + BM25)
9  step09_retrieve        Route by question type; hybrid search; expansion; fallback
10 step10_generate        Gemini answer, grounded in the retrieved context
11 step11_answer          {"answer", "question_type", "sources"}

   pipeline.py runs steps 7-11 in order; craft_keys.py holds the craft_key alias map.
"""
