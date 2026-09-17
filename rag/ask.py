"""Query pipeline - steps 7 to 11.

    python ask.py "what is the refund policy?"
    python ask.py                 # interactive loop, 'exit' to quit
"""

import sys

import config
from rag import index_meta
from rag.step05_embedding import get_embeddings
from rag.step06_vector_store import get_vector_store
from rag.step07_user_question import get_question
from rag.step08_query_embedding import embed_question
from rag.step09_retrieve import retrieve
from rag.step10_generate import generate_answer, get_llm
from rag.step11_answer import present


def answer_once(question: str, embeddings, vector_store, llm) -> dict:
    query_vector = embed_question(embeddings, question)             # 8
    scored_docs = retrieve(vector_store, query_vector, config.TOP_K)  # 9
    answer = generate_answer(question, scored_docs, llm=llm)        # 10
    return present(question, answer, scored_docs)                   # 11


def main() -> None:
    index_meta.check_before_query(config.COLLECTION_NAME)           # model still matches the index?
    embeddings = get_embeddings()                                   # 5 (reused)
    vector_store = get_vector_store(embeddings, config.COLLECTION_NAME)  # 6 (read side)
    llm = get_llm()

    if sys.argv[1:]:
        answer_once(get_question(), embeddings, vector_store, llm)  # 7
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
        answer_once(question, embeddings, vector_store, llm)


if __name__ == "__main__":
    main()
