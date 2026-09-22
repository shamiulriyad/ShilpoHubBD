"""STEP 7 - User question, and Step A: understand it.

`get_question` reads the question as before. `analyze_question` is Step A of the query
pipeline: Gemini reads the question (prompts/query_analysis.txt) and returns JSON

    {"language", "question_type", "craft_keys", "districts", "divisions", "category_norm",
     "risk_levels", "is_unesco", "is_gi", "english_query"}

which step 8 embeds (the English query, not the raw question - the embedding model is
English-only, so Bangla questions work by being translated here) and step 9 turns into a
retrieval route with filters.

The model's JSON is never trusted as is: craft keys are checked against the ones actually in
the index, district spellings are normalized, enums are validated, and if the call or the
parse fails the question is still answered - as an unfiltered "describe" search on the raw
question - rather than failing.
"""

import json
import re
import sys
from typing import Any, Dict, List, Optional

from langchain_google_genai import ChatGoogleGenerativeAI

import config
from rag import prompts
from rag.step02_normalize_json import DISTRICT_ALIASES
from rag.step03_clean_data import CATEGORIES, RISK_LEVELS

QUESTION_TYPES = ("describe", "how_made", "materials_tools", "craft_location", "location_crafts",
                  "status", "list", "compare", "time", "heritage", "out_of_scope")

_BANGLA_SCRIPT = re.compile("[ঀ-৿]")
_FENCE = re.compile(r"^```(?:json)?\s*|\s*```$", re.I)


def get_question(argv: Optional[list] = None) -> str:
    argv = argv if argv is not None else sys.argv[1:]

    question = " ".join(argv).strip() if argv else input("\nYour question: ").strip()

    if not question:
        raise ValueError("Empty question.")

    print(f"[7] Question   : {question}")
    return question


# --- what is in the index (craft keys, places) ---------------------------------------------------

_CATALOGS: Dict[tuple, Dict[str, Any]] = {}


def load_catalog(client, collection: str) -> Dict[str, Any]:
    """The crafts, districts and divisions actually present in the index, read from its
    payloads (so the prompt and the validation always match what can be filtered on).
    Cached per (collection, size), so a re-ingest is picked up."""
    key = (collection, client.count(collection, exact=True).count)
    if key in _CATALOGS:
        return _CATALOGS[key]

    crafts: Dict[str, Dict[str, Any]] = {}
    districts, divisions, offset = set(), set(), None
    while True:
        points, offset = client.scroll(collection, limit=256, offset=offset, with_payload=True, with_vectors=False)
        for point in points:
            meta = point.payload.get("metadata") or {}
            districts.update(meta.get("districts") or [])
            divisions.update(meta.get("divisions") or [])
            if meta.get("craft_key"):
                entry = crafts.setdefault(meta["craft_key"], {"names": [], "name_bn": None, "category_norm": set()})
                if meta.get("name_en") and meta["name_en"] not in entry["names"]:
                    entry["names"].append(meta["name_en"])
                entry["name_bn"] = entry["name_bn"] or meta.get("name_bn")
                entry["category_norm"].add(meta.get("category_norm"))
        if offset is None:
            break

    _CATALOGS[key] = {"crafts": crafts, "districts": sorted(districts), "divisions": sorted(divisions)}
    return _CATALOGS[key]


def _craft_lines(catalog: Dict[str, Any]) -> str:
    lines = []
    for key in sorted(catalog["crafts"]):
        entry = catalog["crafts"][key]
        names = " / ".join(entry["names"][:3])                 # the same craft is named differently per file
        bn = f" ({entry['name_bn']})" if entry["name_bn"] else ""
        lines.append(f"- {key}: {names}{bn}")
    return "\n".join(lines)


# --- Step A ----------------------------------------------------------------------------------------

def get_analysis_llm() -> ChatGoogleGenerativeAI:
    """The same Gemini model as step 10, at temperature 0 and asked for JSON."""
    config.require_api_key()
    settings = dict(model=config.LLM_MODEL, google_api_key=config.GOOGLE_API_KEY, temperature=0)
    try:
        return ChatGoogleGenerativeAI(**settings, response_mime_type="application/json")
    except Exception:  # noqa: BLE001 - an older client without JSON mode: the prompt still asks for JSON
        return ChatGoogleGenerativeAI(**settings)


def _parse_json(text: str) -> Dict[str, Any]:
    text = _FENCE.sub("", text.strip())
    start, end = text.find("{"), text.rfind("}")
    if start < 0 or end < start:
        raise ValueError("no JSON object in the reply")
    data = json.loads(text[start:end + 1])
    if not isinstance(data, dict):
        raise ValueError("the reply is not a JSON object")
    return data


def _strings(value: Any) -> List[str]:
    items = value if isinstance(value, list) else ([value] if value else [])
    return [str(v).strip() for v in items if isinstance(v, (str, int)) and str(v).strip()]


def _flag(value: Any) -> Optional[bool]:
    return value if isinstance(value, bool) else None


def _canonical_district(name: str, known: List[str]) -> str:
    """Current spelling: an old one is mapped (Comilla -> Cumilla), a known one is matched
    case-insensitively, anything else is kept (a real district the index has nothing for)."""
    lowered = name.lower()
    lowered = DISTRICT_ALIASES.get(lowered, lowered).lower()
    for candidate in known:
        if candidate.lower() == lowered:
            return candidate
    return lowered.title()


def fallback_analysis(question: str, reason: str) -> Dict[str, Any]:
    return {
        "question": question, "language": "Bangla" if _BANGLA_SCRIPT.search(question) else "English",
        "question_type": "describe", "craft_keys": [], "districts": [], "divisions": [],
        "category_norm": None, "risk_levels": [], "is_unesco": None, "is_gi": None,
        "english_query": question, "source": f"fallback ({reason})",
    }


def normalize_analysis(raw: Dict[str, Any], question: str, catalog: Dict[str, Any]) -> Dict[str, Any]:
    """Validate the model's JSON against the vocabularies the index actually has."""
    qtype = raw.get("question_type")
    if qtype not in QUESTION_TYPES:
        return fallback_analysis(question, f"unknown question_type {qtype!r}")

    keys = [k for k in _strings(raw.get("craft_keys")) if k in catalog["crafts"]]
    divisions = {d.lower(): d for d in catalog["divisions"]}
    category = next((c for c in CATEGORIES if c.lower() == str(raw.get("category_norm") or "").lower()), None)
    risk_levels = [r for r in _strings(raw.get("risk_levels")) if r in RISK_LEVELS and r != "unknown"]
    language = "Bangla" if raw.get("language") == "Bangla" or _BANGLA_SCRIPT.search(question) else "English"

    return {
        "question": question,
        "language": language,
        "question_type": qtype,
        "craft_keys": list(dict.fromkeys(keys)),
        "districts": list(dict.fromkeys(_canonical_district(d, catalog["districts"]) for d in _strings(raw.get("districts")))),
        "divisions": [divisions[d.lower()] for d in _strings(raw.get("divisions")) if d.lower() in divisions],
        "category_norm": category,
        "risk_levels": list(dict.fromkeys(risk_levels)),
        "is_unesco": _flag(raw.get("is_unesco")),
        "is_gi": _flag(raw.get("is_gi")),
        "english_query": (str(raw.get("english_query") or "").strip() or question),
        "source": "gemini",
    }


def analyze_question(question: str, catalog: Dict[str, Any], llm=None) -> Dict[str, Any]:
    prompt = prompts.render(
        prompts.load_prompt("query_analysis.txt"),
        question=question,
        craft_keys=_craft_lines(catalog),
        categories=", ".join(CATEGORIES),
        risk_levels=", ".join(r for r in RISK_LEVELS if r != "unknown"),
    )
    try:
        reply = prompts.invoke_llm(llm or get_analysis_llm(), prompt)
        analysis = normalize_analysis(_parse_json(reply), question, catalog)
    except Exception as exc:  # noqa: BLE001 - the question is still answered, just without routing
        print(f"    ! query analysis failed ({type(exc).__name__}: {str(exc)[:120]}); answering without routing")
        analysis = fallback_analysis(question, type(exc).__name__)

    found = [f"{k}={analysis[k]}" for k in ("craft_keys", "districts", "divisions", "category_norm", "risk_levels",
                                              "is_unesco", "is_gi") if analysis[k] not in (None, [], "")]
    print(f"[A] Analysis   : type={analysis['question_type']} language={analysis['language']}"
          + (f" | {'; '.join(found)}" if found else "") + f"\n    english_query: {analysis['english_query']}")
    return analysis
