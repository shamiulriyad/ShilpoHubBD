"""Query pipeline - steps 7 to 11.

    python ask.py "Which crafts are associated with Narayanganj?"
    python ask.py "শীতল পাটি কীভাবে তৈরি হয়?"      # Bangla works too
    python ask.py                 # interactive loop, 'exit' to quit
"""

import sys

import config
from rag import index_meta
from rag.pipeline import QueryResult, build_context, run_query
from rag.step05_embedding import get_embeddings, get_sparse_embeddings
from rag.step06_vector_store import collection_exists, get_client
from rag.step07_user_question import get_question
from rag.step11_answer import present


def answer_once(question: str, ctx, collection: str = config.COLLECTION_NAME) -> QueryResult:
    result = run_query(question, ctx, collection)                   # 7 -> 11
    present(question, result.response)
    return result


def main() -> None:
    config.ensure_utf8_console()

    index_meta.check_before_query(config.COLLECTION_NAME)           # model still matches the index?
    client = get_client()
    if not client.collection_exists(config.COLLECTION_NAME):
        raise SystemExit(f"Collection '{config.COLLECTION_NAME}' does not exist yet. Run:  python ingest.py --recreate")

    ctx = build_context(client, get_embeddings(), get_sparse_embeddings())   # 5 (reused) + BM25

    if sys.argv[1:]:
        answer_once(get_question(), ctx)                            # 7
        return

    print("Interactive mode. Type 'exit' to quit.")
    while True:
        try:
            question = get_question([])                             # 7
        except (KeyboardInterrupt, EOFError):
            print()
            return
        if question.lower() in {"exit", "quit", "q"}:
            return
        try:
            answer_once(question, ctx)
        except RuntimeError as exc:                                 # e.g. Gemini quota - keep the session alive
            print(f"\n! {exc}\n")


if __name__ == "__main__":
    main()
