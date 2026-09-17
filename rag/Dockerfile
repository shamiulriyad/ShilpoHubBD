# Python RAG service (FastAPI). Default = Gemini embeddings, so no torch here.
# For local embeddings (EMBEDDING_PROVIDER=huggingface) add a line installing
# requirements-local.txt below - the image gets ~2 GB bigger.
FROM python:3.11-slim

WORKDIR /app

# Install deps first so Docker caches this layer when only source changes.
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
# For local embeddings, uncomment:
# COPY requirements-local.txt .
# RUN pip install --no-cache-dir -r requirements-local.txt

COPY . .

EXPOSE 8000

# In compose, QDRANT_URL points at the qdrant service and QDRANT_PATH is unset,
# so /ingest (upload-to-index) works. Ingest at build time is not done here -
# upload a PDF through the app, or run `python ingest.py` inside the container.
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
