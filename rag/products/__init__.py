"""Product Search index: a separate, self-contained system (own Qdrant collection, own embedding config).

It shares nothing with the heritage RAG (`shilpohub`) or the Travel Planner RAG (`travel-planner`) except the
Gemini API key from the root .env. See products/settings.py.
"""
