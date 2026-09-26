"""Question -> structured product search (filters, sort, semantic text).

Gemini does the understanding (Bangla / romanised Bangla / English). Everything it returns is validated against
the live vocabulary, and a deterministic analyser takes over when Gemini is unavailable, so search never dies
because of a rate limit. The LLM never sees product data or database credentials.
"""

import json
import re
from collections import OrderedDict
from pathlib import Path
from typing import Any, Dict, List, Optional

import config
from rag.prompts import invoke_llm, render      # existing helpers, used read-only

from products.vocab import Vocabulary

PROMPT_PATH = Path(__file__).with_name("query_analysis_prompt.txt")
SORTS = {"relevance", "rating", "price_asc", "price_desc", "newest", "popular"}
_FENCE = re.compile(r"^```(?:json)?\s*|\s*```$", re.I)
_BN_DIGITS = str.maketrans("০১২৩৪৫৬৭৮৯", "0123456789")
_CACHE: "OrderedDict[str, Dict[str, Any]]" = OrderedDict()
_CACHE_SIZE = 256


def empty_analysis(question: str) -> Dict[str, Any]:
    return {
        "english_query": question.strip(), "semantic": True, "sort": "relevance", "min_price": None, "max_price": None,
        "min_rating": None, "in_stock_only": False, "district": None, "division": None, "category_slug": None,
        "product_type": None, "materials": [], "occasions": [], "colors": [], "language": "en", "source": "default",
    }


# ---- LLM ----------------------------------------------------------------------------------------------------------
def get_llm():
    from langchain_google_genai import ChatGoogleGenerativeAI

    settings = dict(model=config.LLM_MODEL, google_api_key=config.GOOGLE_API_KEY, temperature=0)
    try:
        return ChatGoogleGenerativeAI(**settings, response_mime_type="application/json")
    except TypeError:
        return ChatGoogleGenerativeAI(**settings)


def _lines(items: List[Dict[str, str]], with_bn: bool = False) -> str:
    return "\n".join(f"{i['slug']}: {i['name']}" + (f" ({i['name_bn']})" if with_bn and i.get("name_bn") else "") for i in items) or "(none)"


def _parse_json(text: str) -> Dict[str, Any]:
    text = _FENCE.sub("", text.strip())
    start, end = text.find("{"), text.rfind("}")
    if start < 0 or end <= start:
        raise ValueError("no JSON object in the model reply")
    return json.loads(text[start:end + 1])


def _number(value: Any) -> Optional[float]:
    try:
        number = float(str(value).replace(",", "").translate(_BN_DIGITS))
        return number if number >= 0 else None
    except (TypeError, ValueError):
        return None


def _strings(value: Any) -> List[str]:
    return [str(v).strip() for v in value if str(v).strip()] if isinstance(value, list) else []


def normalize(raw: Dict[str, Any], question: str, vocab: Vocabulary, source: str) -> Dict[str, Any]:
    """Keep only values the marketplace really has; drop anything the model invented."""
    out = empty_analysis(question)
    out["source"] = source
    english = str(raw.get("english_query") or "").strip()
    out["semantic"] = bool(raw.get("semantic", True))
    out["english_query"] = english if out["semantic"] else ""
    out["sort"] = raw.get("sort") if raw.get("sort") in SORTS else "relevance"
    out["min_price"], out["max_price"] = _number(raw.get("min_price")), _number(raw.get("max_price"))
    if out["min_price"] is not None and out["max_price"] is not None and out["min_price"] > out["max_price"]:
        out["min_price"], out["max_price"] = out["max_price"], out["min_price"]
    rating = _number(raw.get("min_rating"))
    out["min_rating"] = rating if rating is not None and 0 < rating <= 5 else None
    out["in_stock_only"] = bool(raw.get("in_stock_only", False))
    out["district"] = vocab.canonical_district(raw.get("district"))
    out["division"] = None if out["district"] else vocab.canonical_division(raw.get("division"))
    slug = raw.get("category_slug")
    out["category_slug"] = slug if slug in vocab.category_slugs() else None
    ptype = raw.get("product_type")
    out["product_type"] = ptype if ptype in vocab.type_slugs() else None
    out["materials"] = [m for m in _strings(raw.get("materials")) if m in vocab.material_slugs()]
    out["occasions"] = [o.lower() for o in _strings(raw.get("occasions"))][:6]
    out["colors"] = [c.lower() for c in _strings(raw.get("colors"))][:6]
    out["language"] = raw.get("language") if raw.get("language") in {"bn", "banglish", "en"} else "en"
    if out["semantic"] and not out["english_query"]:
        out["english_query"] = question.strip()
    return out


def llm_analysis(question: str, vocab: Vocabulary, llm=None) -> Dict[str, Any]:
    prompt = render(
        PROMPT_PATH.read_text(encoding="utf-8"), question=question.strip(), categories=_lines(vocab.categories),
        product_types=_lines(vocab.product_types, True), materials=_lines(vocab.materials, True),
        districts=", ".join(d["name"] for d in vocab.districts), divisions=", ".join(vocab.divisions))
    reply = invoke_llm(llm or get_llm(), prompt, retries=1)
    return normalize(_parse_json(reply), question, vocab, "gemini")


# ---- deterministic fallback -----------------------------------------------------------------------------------------
_PRICE = re.compile(r"(\d[\d,]*(?:\.\d+)?)\s*(k|হাজার|hajar|thousand|lakh|লাখ)?", re.I)
_MAX_HINT = re.compile(r"\b(under|below|within|less than|upto|up to|moddhe|niche|er moddhe|kom|max|maximum)\b|মধ্যে|নিচে|কম", re.I)
_MIN_HINT = re.compile(r"\b(above|over|more than|at least|min|minimum|upore|beshi)\b|উপরে|বেশি", re.I)
_RATING = re.compile(r"highest rated|top rated|best rated|best reviewed|sera rating|সেরা|best rating|highest rating", re.I)
_CHEAP = re.compile(r"\b(cheapest|cheap|shosta|sasta|lowest price)\b|সস্তা|কম দাম", re.I)
_EXPENSIVE = re.compile(r"most expensive|highest price|দামি", re.I)
_NEWEST = re.compile(r"\b(latest|newest|notun|new arrivals?)\b|নতুন", re.I)
_POPULAR = re.compile(r"best sell|most popular|popular|bestseller", re.I)
_STOCK = re.compile(r"\b(available|in stock|stock e|ache|achhe)\b|আছে|পাওয়া যাবে", re.I)
_OCCASIONS = {"wedding": r"wedding|biye|bibaho|বিয়ে|বিবাহ", "eid": r"\beid\b|ঈদ", "pohela boishakh": r"boishakh|baishakh|বৈশাখ",
              "gift": r"\bgift\b|upohar|উপহার", "puja": r"\bpuja\b|পূজা", "home decor": r"home decor|ghor sajano|ঘর সাজ"}


def _price_amount(number: str, unit: Optional[str]) -> float:
    value = float(number.replace(",", ""))
    unit = (unit or "").lower()
    if unit in {"k", "হাজার", "hajar", "thousand"}:
        value *= 1000
    elif unit in {"lakh", "লাখ"}:
        value *= 100000
    return value


def heuristic_analysis(question: str, vocab: Vocabulary) -> Dict[str, Any]:
    text = question.translate(_BN_DIGITS)
    lower = text.lower()
    out = empty_analysis(question)
    out["source"] = "heuristic"

    amounts = [_price_amount(m.group(1), m.group(2)) for m in _PRICE.finditer(text) if len(m.group(1).replace(",", "")) >= 3 or m.group(2)]
    if amounts:
        if _MIN_HINT.search(text) and not _MAX_HINT.search(text):
            out["min_price"] = amounts[0]
        else:
            out["max_price"] = amounts[0] if len(amounts) == 1 else max(amounts)
            if len(amounts) > 1:
                out["min_price"] = min(amounts)

    out["sort"] = ("rating" if _RATING.search(text) else "price_asc" if _CHEAP.search(text) else "price_desc" if _EXPENSIVE.search(text)
                   else "newest" if _NEWEST.search(text) else "popular" if _POPULAR.search(text) else "relevance")
    out["in_stock_only"] = bool(_STOCK.search(text))
    out["occasions"] = [name for name, pattern in _OCCASIONS.items() if re.search(pattern, text, re.I)]

    words = re.findall(r"[a-z]+", lower)
    # District: a word that starts with the district name ("dhakar", "sylhete") -- only names of 4+ letters, to avoid noise.
    for d in sorted(vocab.districts, key=lambda d: -len(d["name"])):
        name = d["name"].lower()
        if len(name) >= 4 and any(w == name or (w.startswith(name) and len(w) - len(name) <= 3) for w in words):
            out["district"] = d["name"]
            break

    for c in vocab.categories:
        tokens = [t for t in re.findall(r"[a-z]+", (c["name"] + " " + c["slug"]).lower()) if len(t) >= 4 and t not in {"craft", "dhakai", "textiles", "weaving"}]
        if any(t in lower for t in tokens):
            out["category_slug"] = c["slug"]
            break
    for t in vocab.product_types:
        name_tokens = [w for w in re.findall(r"[a-z]+", t["name"].lower()) if len(w) >= 4]
        if any(re.search(rf"\b{re.escape(w)}s?\b", lower) for w in name_tokens):
            out["product_type"] = t["slug"]
            break

    residual = re.sub(r"[\d,\.]+|" + "|".join(["taka", "takar", "tk", "moddhe", "khuje", "dao", "dekhao", "gula", "ache"]), " ", lower)
    structural_only = bool(out["district"] or out["category_slug"]) and len(re.findall(r"[a-z]{4,}", residual)) <= 3 and out["sort"] != "relevance"
    out["semantic"] = not (structural_only and not out["occasions"])
    out["english_query"] = question.strip() if out["semantic"] else ""
    return out


# ---- entry point ---------------------------------------------------------------------------------------------------
def analyze(question: str, vocab: Vocabulary, llm=None, use_llm: bool = True) -> Dict[str, Any]:
    key = question.strip().lower()
    if key in _CACHE:
        _CACHE.move_to_end(key)
        return dict(_CACHE[key])

    analysis = None
    if use_llm and config.GOOGLE_API_KEY:
        try:
            analysis = llm_analysis(question, vocab, llm)
        except Exception as exc:  # noqa: BLE001 - quota, network, malformed JSON: fall back, never fail the search
            print(f"[analysis] Gemini analysis failed ({type(exc).__name__}: {str(exc)[:120]}); using the heuristic analyser")
    analysis = analysis or heuristic_analysis(question, vocab)

    if analysis["source"] == "gemini":       # only cache good model answers, never fallbacks
        _CACHE[key] = analysis
        while len(_CACHE) > _CACHE_SIZE:
            _CACHE.popitem(last=False)
    return dict(analysis)
