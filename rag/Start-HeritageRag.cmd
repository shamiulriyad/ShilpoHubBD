@echo off
setlocal
cd /d "%~dp0"

if not exist ".venv\Scripts\python.exe" (
  echo ERROR: rag\.venv was not found. Create it with Python 3.11 or 3.12 first.
  exit /b 1
)

echo Starting ShilpoHub Heritage RAG on http://127.0.0.1:8000
".venv\Scripts\python.exe" -m uvicorn main:app --host 127.0.0.1 --port 8000
