"""Evaluate the RAG pipeline on eval/questions.json.

    python eval/run_eval.py                    # full pipeline: analysis + retrieval + answer (2 Gemini calls per question)
    python eval/run_eval.py --no-answer        # retrieval only (1 Gemini call per question)
    python eval/run_eval.py --only status      # one question_type
    python eval/run_eval.py --limit 5 --delay 4

Per question it prints: the detected question type, the filters used, the craft_keys that
were retrieved, HIT / MISS against expected_crafts (plus recall), and the answer.
At the end: retrieval hit rate per question_type and per language.

questions.json entries:
    question, language (en | bn | banglish), question_type, expected_crafts[], must_not_contain[]
  and optionally:
    expected_docs[]   doc_ids that must be retrieved (REF-UNESCO, REF-SITES ...)
    expect_refusal    the right behaviour is "not available" / out of scope: retrieval must be
                      empty or skipped, and (with an answer) the reply must be a refusal

HIT means every expected craft (and doc) was retrieved. For large sets (a district with 15
crafts) expected_crafts lists only the crafts that MUST be there, and recall is shown as well.
"""

import argparse
import contextlib
import io
import json
import sys
import time
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT))

import config  # noqa: E402

config.ensure_utf8_console()

from rag.pipeline import build_context, run_query  # noqa: E402
from rag.step05_embedding import get_embeddings, get_sparse_embeddings  # noqa: E402
from rag.step06_vector_store import get_client  # noqa: E402
from rag.step10_generate import FIXED_REPLIES  # noqa: E402

REFUSALS = {text for replies in FIXED_REPLIES.values() for text in replies.values()}


def evaluate(question: dict, result, generated: bool) -> dict:
    retrieval, response, analysis = result.retrieval, result.response, result.analysis
    got_keys = retrieval.craft_keys
    got_docs = {h.metadata["doc_id"] for h in retrieval.hits}
    need, need_docs = question.get("expected_crafts", []), question.get("expected_docs", [])
    refused = response["answer"] in REFUSALS
    nothing_retrieved = retrieval.skipped or not retrieval.hits

    recall = None
    if question.get("expect_refusal"):
        # right behaviour: nothing usable retrieved, and (if generated) a refusal
        hit = (refused if generated else (True if nothing_retrieved else None))
    else:
        recall = (len([k for k in need if k in got_keys]) / len(need)) if need else None
        hit = all(k in got_keys for k in need) and all(d in got_docs for d in need_docs)

    lowered = response["answer"].lower()
    violations = [s for s in question.get("must_not_contain", []) if s.lower() in lowered] if generated else []
    return {
        "question": question["question"], "language": question["language"],
        "expected_type": question["question_type"], "detected_type": analysis["question_type"],
        "type_ok": analysis["question_type"] == question["question_type"],
        "filters": retrieval.filters, "fallback": retrieval.fallback,
        "retrieved_craft_keys": got_keys, "retrieved_docs": sorted(got_docs), "chunks": len(retrieval.hits),
        "expected_crafts": need, "recall": recall, "hit": hit,
        "refused": refused, "violations": violations,
        "answer": response["answer"], "analysis_source": analysis["source"],
    }


def rate(rows: list) -> str:
    scored = [r for r in rows if r["hit"] is not None]
    if not scored:
        return "n/a"
    hits = sum(1 for r in scored if r["hit"])
    return f"{hits}/{len(scored)} = {100 * hits / len(scored):.0f}%"


def main() -> None:
    parser = argparse.ArgumentParser(description="Evaluate the ShilpoHub RAG pipeline.")
    parser.add_argument("--questions", default=str(ROOT / "eval" / "questions.json"))
    parser.add_argument("--no-answer", action="store_true", help="stop after retrieval (no answer generation)")
    parser.add_argument("--only", help="only this question_type")
    parser.add_argument("--limit", type=int, help="only the first N questions")
    parser.add_argument("--delay", type=float, default=0.0, help="seconds to wait between questions (rate limits)")
    parser.add_argument("--out", default=str(ROOT / "eval" / "results.json"))
    args = parser.parse_args()

    questions = json.loads(Path(args.questions).read_text(encoding="utf-8"))
    if args.only:
        questions = [q for q in questions if q["question_type"] == args.only]
    if args.limit:
        questions = questions[:args.limit]

    client = get_client()
    if not client.collection_exists(config.COLLECTION_NAME):
        raise SystemExit(f"Collection '{config.COLLECTION_NAME}' does not exist. Run:  python ingest.py --recreate")
    with contextlib.redirect_stdout(io.StringIO()):
        ctx = build_context(client, get_embeddings(), get_sparse_embeddings())

    generated = not args.no_answer
    rows = []
    for number, question in enumerate(questions, start=1):
        print(f"\n[{number}/{len(questions)}] ({question['question_type']}, {question['language']}) {question['question']}")
        try:
            with contextlib.redirect_stdout(io.StringIO()):          # the steps narrate; the eval reports
                result = run_query(question["question"], ctx, config.COLLECTION_NAME, generate=generated)
            row = evaluate(question, result, generated)
        except Exception as exc:  # noqa: BLE001 - one bad question must not stop the run
            print(f"   ERROR {type(exc).__name__}: {str(exc)[:200]}")
            rows.append({"question": question["question"], "language": question["language"],
                         "expected_type": question["question_type"], "hit": None, "type_ok": False,
                         "error": str(exc), "violations": []})
            continue

        rows.append(row)
        verdict = {True: "HIT", False: "MISS", None: "n/a"}[row["hit"]]
        recall = f" (recall {row['recall']:.2f})" if row["recall"] is not None else ""
        print(f"   detected : {row['detected_type']} {'ok' if row['type_ok'] else '<- expected ' + row['expected_type']}"
              f" [{row['analysis_source']}]")
        print(f"   filters  : {row['filters'] or 'none'}{'  | fell back to unfiltered' if row['fallback'] else ''}")
        print(f"   retrieved: {row['retrieved_craft_keys']} ({row['chunks']} chunks)"
              + (f" docs={[d for d in row['retrieved_docs'] if d.startswith('REF-')]}" if any(d.startswith('REF-') for d in row['retrieved_docs']) else ""))
        print(f"   retrieval: {verdict}{recall}"
              + (f"  expected {row['expected_crafts']}" if row["hit"] is False else ""))
        if generated:
            shown = " ".join(row["answer"].split())
            print(f"   answer   : {shown[:260]}{'...' if len(shown) > 260 else ''}")
            print(f"   must_not_contain: {'VIOLATED ' + str(row['violations']) if row['violations'] else 'ok'}")
        if args.delay:
            time.sleep(args.delay)

    ok = [r for r in rows if "error" not in r]
    print("\n" + "=" * 72)
    print(f"EVAL SUMMARY  ({len(ok)}/{len(rows)} questions ran; mode: {'retrieval + answer' if generated else 'retrieval only'})")
    print("=" * 72)
    print("Retrieval hit rate per question_type:")
    by_type = defaultdict(list)
    for r in ok:
        by_type[r["expected_type"]].append(r)
    for qtype, items in by_type.items():
        print(f"  {qtype:16} {rate(items)}")
    print("Retrieval hit rate per language:")
    by_lang = defaultdict(list)
    for r in ok:
        by_lang[r["language"]].append(r)
    for language, items in by_lang.items():
        print(f"  {language:16} {rate(items)}")
    print(f"Overall retrieval hit rate : {rate(ok)}")
    print(f"Question type detected right: {sum(1 for r in ok if r['type_ok'])}/{len(ok)}")
    if generated:
        bad = [r for r in ok if r["violations"]]
        print(f"must_not_contain violations : {len(bad)}" + (f"  -> {[r['question'] for r in bad]}" if bad else ""))
    if len(ok) != len(rows):
        print(f"Errors: {len(rows) - len(ok)} question(s) failed - see above")

    Path(args.out).write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"\nPer-question results written to {args.out}")


if __name__ == "__main__":
    main()
