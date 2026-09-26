"""AI attribute SUGGESTIONS for a product (Gemini).

The backend sends the product's own text; this returns proposed attributes, validated against the live vocabulary.
Nothing here is ever final: the backend stores the result as a PENDING suggestion and the producer must review,
edit and confirm it. This code has no database access and no credentials.
"""

import json
import re
from pathlib import Path
from typing import Any, Dict, List, Optional

from rag.prompts import invoke_llm, render      # existing helpers, used read-only

import config
from products.analysis import _FENCE, get_llm
from products.vocab import Vocabulary

PROMPT_PATH = Path(__file__).with_name("suggest_prompt.txt")
PRODUCTION_METHODS = {"handmade": "Handmade", "handloom": "Handloom", "hand-finished": "Hand-finished"}
OCCASIONS = {"wedding", "eid", "pohela boishakh", "gift", "puja", "festival", "home decor", "daily wear"}
MAX_TEXT = 2500          # the model never needs more than this much product text


def _lines(items: List[Dict[str, str]], with_bn: bool = False) -> str:
    return "\n".join(f"{i['slug']}: {i['name']}" + (f" ({i['name_bn']})" if with_bn and i.get("name_bn") else "") for i in items) or "(none)"


def _clean_list(value: Any, limit: int, max_len: int = 40, lower: bool = False) -> List[str]:
    out: List[str] = []
    for item in value if isinstance(value, list) else []:
        text = re.sub(r"\s+", " ", str(item)).strip()
        text = text.lower() if lower else text
        if text and len(text) <= max_len and text.lower() not in {o.lower() for o in out}:
            out.append(text)
    return out[:limit]


def _short(value: Any, max_len: int) -> Optional[str]:
    text = re.sub(r"\s+", " ", str(value or "")).strip()
    return text[:max_len] if text else None


def normalize(raw: Dict[str, Any], vocab: Vocabulary) -> Dict[str, Any]:
    """Keep only what the marketplace vocabulary allows; never let a model-invented value through."""
    ptype = raw.get("productTypeSlug")
    method = PRODUCTION_METHODS.get(str(raw.get("productionMethod") or "").strip().lower())
    return {
        "productTypeSlug": ptype if ptype in vocab.type_slugs() else None,
        "materialSlugs": [m for m in _clean_list(raw.get("materialSlugs"), 6, 60) if m in vocab.material_slugs()],
        "tags": _clean_list(raw.get("tags"), 8, lower=True),
        "keywords": _clean_list(raw.get("keywords"), 8),
        "occasions": [o for o in _clean_list(raw.get("occasions"), 5, lower=True) if o in OCCASIONS],
        "colors": _clean_list(raw.get("colors"), 6, lower=True),
        "craftTechnique": _short(raw.get("craftTechnique"), 200),
        "productionMethod": method,
        "careInstructions": _short(raw.get("careInstructions"), 300),
        "notes": _short(raw.get("notes"), 200) or "",
    }


def suggest(product: Dict[str, Any], vocab: Vocabulary, llm=None) -> Dict[str, Any]:
    def field(name: str) -> str:
        return str(product.get(name) or "").strip()[:MAX_TEXT] or "(not provided)"

    prompt = render(
        PROMPT_PATH.read_text(encoding="utf-8"), product_types=_lines(vocab.product_types, True), materials=_lines(vocab.materials, True),
        name=field("name"), category=field("category"), district=field("district"), description=field("description"), story=field("story"))
    reply = _FENCE.sub("", invoke_llm(llm or get_llm(), prompt, retries=1).strip())
    start, end = reply.find("{"), reply.rfind("}")
    if start < 0 or end <= start:
        raise ValueError("the model did not return a JSON object")
    return normalize(json.loads(reply[start:end + 1]), vocab)


def model_name() -> str:
    return config.LLM_MODEL
