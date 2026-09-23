"""STEP 3 - Clean the normalized data, derive the normalized fields, render text.

1. Clean - drop null / empty / placeholder values ("N/A", "", [], {}), normalize whitespace
   and Unicode, flatten nested lists, remove duplicate list entries and duplicate documents.

2. Normalize two free-text fields into fixed vocabularies:
     category_norm  one of the 12 CATEGORIES, from the category strings of all three files
     risk_level     one of RISK_LEVELS, from craftDetails.json's free-text endangerment_level

3. Render every craft into three aspects - overview, production, geography - as a list of
   text parts (one line or block each). Step 4 packs the parts into chunks; a part is never
   cut in half unless it alone is too big. Confidence labels are kept in the text so an
   answer can respect them.

    overview    category, description, heritage status, notes, skill transmission, sources
    production  steps, materials, tools, techniques, products, time required
    geography   regions / associations with confidence, primary hub, endangerment
"""

import hashlib
import json
import re
import unicodedata
from collections import OrderedDict
from typing import Any, Dict, List, Optional

# U+200C / U+200D (zero-width non-joiner / joiner) are deliberately NOT stripped: Bangla
# conjuncts and some Bangla spellings depend on them.
_INVISIBLE = re.compile(r"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f​⁠﻿­]")
_SPACES = re.compile(r"\s+")
_NULLISH = {"", "null", "none", "n/a", "na", "nan", "nil", "tbd", "-", "--", "–", "—"}


# --- cleaning ------------------------------------------------------------------------------

def clean_text(text: str) -> str:
    text = unicodedata.normalize("NFC", text)   # canonical Bangla / accent composition
    text = _INVISIBLE.sub("", text)
    return _SPACES.sub(" ", text).strip()       # also folds \n, \t and non-breaking spaces


def _dedupe_key(item: Any) -> str:
    if isinstance(item, str):
        return item.casefold()
    return json.dumps(item, sort_keys=True, ensure_ascii=False, default=str)


def clean_value(value: Any) -> Any:
    """Recursively clean a JSON value. Returns None when nothing meaningful is left."""
    if value is None or isinstance(value, bool):
        return None
    if isinstance(value, (int, float)):
        return str(int(value)) if float(value).is_integer() else str(value)
    if isinstance(value, str):
        text = clean_text(value)
        return None if text.casefold() in _NULLISH else text
    if isinstance(value, (list, tuple)):
        items: List[Any] = []
        seen = set()
        for item in value:
            cleaned = clean_value(item)
            for part in (cleaned if isinstance(cleaned, list) else [cleaned]):   # flatten nested lists
                if part is None:
                    continue
                key = _dedupe_key(part)
                if key not in seen:
                    seen.add(key)
                    items.append(part)
        return items or None
    if isinstance(value, dict):
        out = OrderedDict()
        for key, item in value.items():
            cleaned = clean_value(item)
            if cleaned is not None:
                out[str(key)] = cleaned
        return out or None
    return clean_value(str(value))              # unexpected type: keep it as text


# --- category_norm -----------------------------------------------------------------------------

CATEGORIES = (
    "Textile Heritage", "Natural Fibre Crafts", "Clay Crafts", "Metal Crafts", "Wood Crafts",
    "Folk Arts", "Musical Crafts", "Food Heritage", "Agriculture (Krishi Shilpo)", "Fisheries",
    "Cottage Industries", "Other Traditional Crafts",
)
_T, _NF, _CL, _ME, _WO, _FO, _MU, _FD, _AG, _FI, _CO, _OT = CATEGORIES

# craftDetails.json prefixes each category with the scope document's section letter.
# "A. Handicrafts" is the catch-all for everything hand-made, so it says nothing by itself.
_LETTER_CATEGORY = {"B": _T, "C": _FO, "D": _MU, "E": _ME, "F": _CL, "G": _FD, "H": _AG, "I": _FI, "J": _CO}
_LETTER = re.compile(r"\b([A-J])\.\s")

# craft.json / GEO.json category strings that name a category outright.
# "Handicraft", "Folk Craft" and "Indigenous Craft" are too broad and fall through to the name.
_STRING_CATEGORY = [
    (re.compile(r"textile", re.I), _T),
    (re.compile(r"natural fib", re.I), _NF),
    (re.compile(r"clay", re.I), _CL),
    (re.compile(r"metal", re.I), _ME),
    (re.compile(r"wood", re.I), _WO),
    (re.compile(r"musical", re.I), _MU),
    (re.compile(r"folk art", re.I), _FO),
]

# Decide by the craft's name (first match wins).
_NAME_CATEGORY = [
    (re.compile(r"kantha|weav|saree|silk|khadi|muslin|jamdani|satrangi|cotton|handloom|textile", re.I), _T),
    (re.compile(r"\bpati\b|bamboo|cane|rattan|jute|\bfans?\b|pakha|pankha", re.I), _NF),
    (re.compile(r"instrument|dotara|ektara|tabla|dhol|flute|harmonium", re.I), _MU),
    (re.compile(r"wood|boat|furniture", re.I), _WO),
    (re.compile(r"pottery|terracotta|\bclay\b", re.I), _CL),
    (re.compile(r"metal|brass|copper|knife|tool", re.I), _ME),
    (re.compile(r"shell|conch|coconut|leather|jewel", re.I), _OT),
    (re.compile(r"folk|paint|alpana|mask|rickshaw|patachitra|calligraph", re.I), _FO),
]


def _category_from_name(name: str) -> Optional[str]:
    for pattern, category in _NAME_CATEGORY:
        if pattern.search(name):
            return category
    return None


def normalize_category(raw: Optional[str], name: str) -> str:
    """One of CATEGORIES for a record, from its own category string and its name."""
    raw = raw or ""
    letters = [_LETTER_CATEGORY[m] for m in _LETTER.findall(raw) if m in _LETTER_CATEGORY]
    by_name = _category_from_name(name or "")

    if letters:
        # "A. Handicrafts / C. Folk Art / B. Textile Heritage": the name settles a tie.
        return by_name if by_name in letters else letters[0]
    for pattern, category in _STRING_CATEGORY:
        if pattern.search(raw):
            return category
    return by_name or _OT


# --- risk_level ---------------------------------------------------------------------------------

RISK_LEVELS = ("critically_endangered", "endangered", "vulnerable", "at_risk", "stable", "growing", "unknown")

# The earliest keyword decides: "Stable as a broad category rather than a single endangered
# craft" is stable (its first keyword), not endangered. Alternation order breaks same-position
# ties, so "critically endangered" beats "endangered".
_RISK_KEYWORDS = re.compile(
    r"(?P<critically_endangered>critically endangered)"
    r"|(?P<endangered>\bendangered\b)"
    r"|(?P<vulnerable>\bvulnerable\b)"
    r"|(?P<at_risk>\bat risk\b|\brisk\b|under pressure|\bdeclin\w*|\bshrinking\b|\brare\b|losing ground)"
    r"|(?P<stable>\bstable\b|\bsteady\b)"
    r"|(?P<growing>\bgrowing\b|\bthriving\b)",
    re.I,
)
_STABLE_BUT = re.compile(r"^\W*(relatively\s+)?stable\s+but\b", re.I)   # "Stable but shrinking ..." is not stable


def classify_risk(text: Optional[str]) -> str:
    if not text:
        return "unknown"
    if _STABLE_BUT.match(text):
        return "at_risk"
    match = _RISK_KEYWORDS.search(text)
    return match.lastgroup if match else "unknown"


# --- rendering -------------------------------------------------------------------------------------

def _pretty(text: str) -> str:
    return text.replace("_", " ")


def _end(text: str) -> str:
    return text if text.endswith((".", "!", "?", "।")) else text + "."


def _as_list(value: Any) -> List[Any]:
    return value if isinstance(value, list) else [value]


def _join(value: Any, sep: str = "; ") -> str:
    return sep.join(str(v) for v in _as_list(value))


_DATA_CONFIDENCE = {
    "verified": "verified",
    "general_knowledge": "general knowledge (not individually verified)",
}


def _confidence_part(value: str) -> str:
    if value in _DATA_CONFIDENCE:
        return f"Data confidence: {_DATA_CONFIDENCE[value]}."
    return f"Dataset confidence: {value}."


def _time_part(value: Any) -> str:
    if isinstance(value, str):
        return f"Time required: {_end(value)}"
    text = f"Time required: {value.get('value', 'not stated')}"
    if value.get("confidence"):
        text += f" (confidence: {value['confidence']})"
    text = _end(text)
    return f"{text} {_end(value['note'])}" if value.get("note") else text


def _region_line(entry: dict) -> str:
    association, confidence = entry.get("association"), entry.get("confidence")
    if "text" in entry:                             # free-text region (craftDetails.json)
        line = entry["text"]
    else:                                           # structured region (craft.json / GEO.json)
        line = ", ".join(p for p in (
            f"{entry['village']} village/cluster" if entry.get("village") else None,
            entry.get("area"),
            f"{entry['district']} district" if entry.get("district") else None,
            f"{entry['division']} division" if entry.get("division") else None) if p)
    details = ([_pretty(association)] if association else []) + ([f"confidence: {confidence}"] if confidence else [])
    return f"Region: {line}" + (f" ({'; '.join(details)})" if details else "")


def _overview_parts(fields: dict, category_norm: Optional[str]) -> List[str]:
    parts: List[str] = []
    raw = fields.get("category_raw")
    label = " / ".join(x for x in (raw, fields.get("subcategory")) if x)
    if category_norm:
        note = f" (source label: {label})" if label and label.casefold() != category_norm.casefold() else ""
        parts.append(f"Category: {category_norm}{note}")
    for key, prefix in (("description", ""), ("heritage_status", "Heritage status: "),
                        ("notes", "Notes: "), ("skill_transmission", "Skill transmission: ")):
        if fields.get(key):
            parts.append(prefix + _end(fields[key]))
    if fields.get("sources"):
        parts.append(f"Sources: {_join(fields['sources'])}.")
    if fields.get("data_confidence"):
        conf = _confidence_part(fields["data_confidence"])
        parts.append(conf if not fields.get("verification") else f"{conf} Verification: {fields['verification']}.")
    elif fields.get("verification"):
        parts.append(f"Verification: {fields['verification']}.")
    return parts


def _production_parts(fields: dict) -> List[str]:
    parts: List[str] = []
    steps = fields.get("how_made")
    if steps:
        steps = _as_list(steps)
        parts.append("How it is made:\n" + "\n".join(f"{i}. {s}" for i, s in enumerate(steps, 1)))
    for key, label in (("materials", "Materials"), ("tools", "Tools"),
                       ("techniques", "Techniques"), ("products", "Products")):
        if fields.get(key):
            parts.append(f"{label}: {_join(fields[key])}.")
    if fields.get("time_required"):
        parts.append(_time_part(fields["time_required"]))
    return parts


def _geography_parts(fields: dict, risk_level: str) -> List[str]:
    parts = [_region_line(entry) for entry in _as_list(fields.get("location") or [])]
    if fields.get("primary_hub"):
        parts.append(f"Primary hub: {_end(fields['primary_hub'])}")
    if fields.get("endangerment"):
        # The original wording stays in the chunk; the normalized level is added beside it.
        parts.append(f"Endangerment: {_end(fields['endangerment'])} Risk level: {_pretty(risk_level)}.")
    return parts


# --- entry point ---------------------------------------------------------------------------------------

def _content_hash(doc: Dict[str, Any]) -> str:
    text = _SPACES.sub(" ", doc["content"]).casefold()
    return hashlib.sha1(f"{doc['doc_type']}|{doc['craft_key']}|{text}".encode("utf-8")).hexdigest()


def clean_data(docs: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    cleaned: List[Dict[str, Any]] = []
    seen_ids, seen_content = set(), set()
    empty = duplicates = fields_dropped = 0

    for doc in docs:
        name_en = clean_value(doc.get("name_en"))
        aspects: "OrderedDict[str, List[str]]" = OrderedDict()
        fields = OrderedDict()
        for aspect, group in doc["fields"].items():
            fields_dropped += sum(1 for value in group.values() if clean_value(value) is None)
            fields[aspect] = clean_value(group) or OrderedDict()

        if doc["doc_type"] == "reference":
            category_norm, risk_level = None, "unknown"
            lines = fields.get("reference", {}).get("lines") or []
            if lines:
                aspects["reference"] = [str(line) for line in _as_list(lines)]
        else:
            category_raw = fields.get("overview", {}).get("category_raw")
            category_norm = normalize_category(category_raw, name_en or "")
            risk_level = classify_risk(fields.get("geography", {}).get("endangerment"))
            for aspect, parts in (("overview", _overview_parts(fields.get("overview", {}), category_norm)),
                                  ("production", _production_parts(fields.get("production", {}))),
                                  ("geography", _geography_parts(fields.get("geography", {}), risk_level))):
                if parts:
                    aspects[aspect] = parts

        if not name_en or not aspects:              # nothing citable or nothing to say
            empty += 1
            continue

        doc = dict(doc)
        doc.update({
            "name_en": name_en,
            "name_bn": clean_value(doc.get("name_bn")),
            "category_raw": clean_value(doc.get("category_raw")),
            "category_norm": category_norm,
            "risk_level": risk_level,
            "districts": clean_value(doc.get("districts")) or [],
            "divisions": clean_value(doc.get("divisions")) or [],
            "confidences": clean_value(doc.get("confidences")) or [],
            "source_ids": clean_value(doc.get("source_ids")) or [],
            "fields": fields,
            "aspects": aspects,
            "content": "\n\n".join(part for parts in aspects.values() for part in parts),
        })

        content_key = _content_hash(doc)
        if doc["doc_id"] in seen_ids or content_key in seen_content:
            duplicates += 1
            continue
        seen_ids.add(doc["doc_id"])
        seen_content.add(content_key)
        cleaned.append(doc)

    crafts = [d for d in cleaned if d["doc_type"] == "craft"]
    print(f"[3] Cleaned    : {len(cleaned)} documents kept, {empty} empty dropped, {duplicates} duplicate(s) dropped, "
          f"{fields_dropped} null/empty field(s) removed")
    print(f"    category_norm: {len({d['category_norm'] for d in crafts})}/{len(CATEGORIES)} categories used; "
          f"risk_level known for {sum(1 for d in crafts if d['risk_level'] != 'unknown')}/{len(crafts)} records; "
          f"is_unesco: {sum(1 for d in crafts if d['is_unesco'])}, is_gi: {sum(1 for d in crafts if d['is_gi'])} records")
    return cleaned
