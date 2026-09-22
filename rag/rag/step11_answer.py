"""STEP 11 - Answer + sources.

Returns the structured response a chat UI can render:

    {
      "answer": "...",                       # clean prose - the [Doc N] citations are removed
      "question_type": "location_crafts",
      "sources": [
        {"source_file": "craft.json", "doc_id": "craft.json:BDCP-001", "source_ids": ["SRC_UNESCO_JAMDANI"]},
        ...
      ]
    }

`sources` are the documents the answer actually cited ([Doc 1][Doc 3]...), one entry per
document (several chunks of one record collapse into one, their source_ids merged). If the
model cited nothing, every retrieved document is listed. A refusal (not available,
out of scope) cites nothing.
"""

import re
from collections import OrderedDict
from typing import Any, Dict, List

from rag.step09_retrieve import Retrieval

# One citation group. Besides "[Doc 1]" the model drifts into "[Doc 1, Doc 2]" and "[Doc 1, 2]".
_GROUP = r"\[\s*Doc\s*\d+(?:\s*[,;]\s*(?:Doc\s*)?\d+)*\s*\]"
_GROUP_RE = re.compile(_GROUP, re.I)
_RUN = re.compile(rf"[ \t]*{_GROUP}(?:[ \t]*{_GROUP})*", re.I)      # stacked groups are removed together
_NUMBER = re.compile(r"\d+")


def cited_docs(answer: str) -> set:
    """1-based numbers of every context block the answer cites, in any of the forms above."""
    return {int(n) for group in _GROUP_RE.findall(answer) for n in _NUMBER.findall(group)}


def strip_citations(text: str) -> str:
    return _RUN.sub("", text).strip()


def build_sources(answer: str, retrieval: Retrieval) -> List[Dict[str, Any]]:
    cited = cited_docs(answer)
    sources: "OrderedDict[tuple, Dict[str, Any]]" = OrderedDict()
    for number, hit in enumerate(retrieval.hits, start=1):
        if cited and number not in cited:      # the model cited nothing -> keep everything retrieved
            continue
        meta = hit.metadata
        entry = sources.setdefault((meta["source_file"], meta["doc_id"]),
                                   {"source_file": meta["source_file"], "doc_id": meta["doc_id"], "source_ids": []})
        for source_id in meta.get("source_ids") or []:
            if source_id not in entry["source_ids"]:
                entry["source_ids"].append(source_id)
    return list(sources.values())


def build_response(answer: str, refused: bool, analysis: Dict[str, Any], retrieval: Retrieval) -> Dict[str, Any]:
    return {
        "answer": strip_citations(answer),
        "question_type": analysis["question_type"],
        "sources": [] if refused or retrieval.skipped else build_sources(answer, retrieval),
    }


def present(question: str, response: Dict[str, Any]) -> Dict[str, Any]:
    print("\n" + "=" * 72)
    print(f"Q: {question}")
    print(f"type: {response['question_type']}")
    print("-" * 72)
    print(response["answer"])
    print("-" * 72)
    print("Sources:")
    if not response["sources"]:
        print("  (none - no supporting ShilpoHub context was used)")
    for i, source in enumerate(response["sources"], start=1):
        ids = f"  source_ids: {', '.join(source['source_ids'])}" if source["source_ids"] else ""
        print(f"  [{i}] {source['source_file']} | {source['doc_id']}{ids}")
    print("=" * 72)
    return response
