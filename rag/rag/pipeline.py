"""The query pipeline, steps 7 - 11, in order - one function for the CLI (ask.py), the HTTP
service (api/chat.py) and the eval (eval/run_eval.py), so they cannot drift apart.

    7 / A  understand the question   (step07: Gemini -> type, filters, english_query)
    8      embed the English query   (step08: dense + BM25)
    9      route + retrieve          (step09: hybrid search, filters, expansion, fallback)
    10 / D generate the answer       (step10: Gemini, grounded in the retrieved context)
    11     structured response       (step11: answer + sources)
"""

from dataclasses import dataclass
from typing import Any, Dict, Optional

from rag.step07_user_question import analyze_question, get_analysis_llm, load_catalog
from rag.step08_query_embedding import embed_query
from rag.step09_retrieve import Retrieval, retrieve
from rag.step10_generate import generate_answer, get_llm
from rag.step11_answer import build_response


@dataclass
class QueryContext:
    """Everything that is expensive to build and shared across questions."""
    client: Any
    embeddings: Any
    sparse: Any
    llm: Any                 # step 10
    analysis_llm: Any        # step 7


def build_context(client, embeddings, sparse) -> QueryContext:
    return QueryContext(client=client, embeddings=embeddings, sparse=sparse,
                        llm=get_llm(), analysis_llm=get_analysis_llm())


@dataclass
class QueryResult:
    response: Dict[str, Any]          # {"answer", "question_type", "sources"} - what a UI gets
    analysis: Dict[str, Any]          # step A output (for logging / eval)
    retrieval: Retrieval              # what was retrieved and with which filters


def run_query(question: str, ctx: QueryContext, collection: str, generate: bool = True,
              min_score: Optional[float] = None) -> QueryResult:
    """`generate=False` stops after retrieval (used by the eval's retrieval-only mode)."""
    catalog = load_catalog(ctx.client, collection)
    analysis = analyze_question(question, catalog, llm=ctx.analysis_llm)             # 7 / A

    dense, sparse = embed_query(ctx.embeddings, ctx.sparse, analysis["english_query"])  # 8
    retrieval = retrieve(ctx.client, collection, analysis, dense, sparse, min_score=min_score)  # 9

    if not generate:
        return QueryResult({"answer": "", "question_type": analysis["question_type"], "sources": []},
                           analysis, retrieval)
    answer, refused = generate_answer(analysis, retrieval, llm=ctx.llm)              # 10 / D
    return QueryResult(build_response(answer, refused, analysis, retrieval), analysis, retrieval)  # 11
