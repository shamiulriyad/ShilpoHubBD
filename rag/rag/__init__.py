"""RAG pipeline, one module per step of the pipeline.

1  step01_load_pdf        PDF
2  step02_extract_text    Text extraction
3  step03_clean_text      Text cleaning
4  step04_chunking        Chunking
5  step05_embedding       Embedding model
6  step06_vector_store    Store in Qdrant
7  step07_user_question   User question
8  step08_query_embedding Query embedding
9  step09_retrieve        Retrieve relevant chunks
10 step10_generate        Gemini
11 step11_answer          Answer + sources
"""
