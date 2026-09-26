"""Maps `bangladesh_64_district_tourism_rag.json` (64 districts, each with a list of curated
tourism entities and per-interest summaries) onto the intermediate document shape the Travel
Planner chunker expects, and serves the same entities as a deterministic, filterable list.

The file deliberately carries NO coordinates, prices, opening hours or availability, so nothing
here can supply them: every document says so, and the structured list returns only what the
file states. Entities are keyed `<district-slug>:<index>` -- stable as long as the file's
entity order is, which is what the .NET planner uses to ground Gemini's chosen stops.
"""

import json
from pathlib import Path
from typing import Any, Dict, List, Optional

import config

DISTRICTS_FILE = "bangladesh_64_district_tourism_rag.json"


def _pretty(value: str) -> str:
    return str(value).replace("_", " ").strip()


def _entity_key(district: Dict[str, Any], index: int) -> str:
    return f"{district['slug']}:{index}"


def _iter_entities(data: Dict[str, Any]):
    for district in data.get("districts") or []:
        for index, entity in enumerate(district.get("tourism_entities") or []):
            if entity.get("name"):
                yield district, index, entity


def normalize_districts_json(datasets: Dict[str, Any]) -> List[Dict[str, Any]]:
    data = datasets.get(DISTRICTS_FILE) or {}
    docs: List[Dict[str, Any]] = []
    for district, index, entity in _iter_entities(data):
        key = _entity_key(district, index)
        docs.append({
            "doc_id": f"{DISTRICTS_FILE}:{key}",
            "doc_type": "district_entity",
            "source_file": DISTRICTS_FILE,
            "record_id": key,
            "craft_key": key,
            "name_en": entity["name"],
            "name_bn": None,
            "category_raw": entity.get("entity_type") or "",
            "category_norm": entity.get("entity_type") or "",
            "districts": [district["name"]],
            "divisions": [district["division"]] if district.get("division") else [],
            "area": entity.get("area"),
            "themes": list(entity.get("interests") or []),
            "confidences": [],
            "source_ids": [],
            "is_gi": False,
            "is_unesco": "unesco" in (entity.get("description") or "").lower(),
            "fields": {"district_entity": {**entity, "district": district["name"], "division": district.get("division")}},
        })
    print(f"[2] Normalized : {len(docs)} district tourism entit(y/ies) from {DISTRICTS_FILE}")
    return docs


# --- deterministic retrieval (no embeddings) ------------------------------------------------

def load_districts(data_dir: Optional[Path] = None) -> Dict[str, Any]:
    path = Path(data_dir or config.DATA_DIR) / DISTRICTS_FILE
    return json.loads(path.read_text(encoding="utf-8")) if path.is_file() else {}


def _norm(name: str) -> str:
    return "".join(ch for ch in (name or "").casefold() if ch.isalnum())


def find_district(data: Dict[str, Any], name: str) -> Optional[Dict[str, Any]]:
    wanted = _norm(name)
    for district in data.get("districts") or []:
        if wanted in (_norm(district["name"]), _norm(district.get("slug", ""))):
            return district
    return None


def district_entities(
    data: Dict[str, Any], name: str, interests: Optional[List[str]] = None, limit: int = 12,
) -> Dict[str, Any]:
    """Entities of ONE district, ranked by how many selected interests they match.

    With no interests every entity qualifies. With interests, only entities matching at least one
    are returned (the dataset's own retrieval rule); `unmatchedInterests` and `fallbackDistricts`
    tell the caller what the district cannot offer, so it can say so instead of inventing."""
    district = find_district(data, name)
    if district is None:
        return {"found": False, "district": name, "entities": [], "unmatchedInterests": list(interests or []),
                "fallbackDistricts": []}

    wanted = {i.casefold(): i for i in (interests or [])}
    ranked = []
    for index, entity in enumerate(district.get("tourism_entities") or []):
        if not entity.get("name"):
            continue
        matched = [i for i in entity.get("interests") or [] if i.casefold() in wanted]
        if wanted and not matched:
            continue
        ranked.append((-len(matched), index, entity, matched))
    ranked.sort(key=lambda r: (r[0], r[1]))

    summary = district.get("interest_summary") or {}
    unmatched = [i for i in wanted.values() if not (summary.get(i) or {}).get("has_match")]
    fallback: List[str] = []
    for interest in unmatched:
        for other in (summary.get(interest) or {}).get("fallback_districts") or []:
            if other not in fallback:
                fallback.append(other)

    return {
        "found": True,
        "district": district["name"],
        "division": district.get("division"),
        "entities": [
            {
                "key": _entity_key(district, index),
                "name": entity["name"],
                "area": entity.get("area"),
                "entityType": entity.get("entity_type"),
                "interests": entity.get("interests") or [],
                "matchedInterests": matched,
                "description": entity.get("description"),
                "tags": entity.get("tags") or [],
            }
            for _, index, entity, matched in ranked[:max(1, limit)]
        ],
        "unmatchedInterests": unmatched,
        "fallbackDistricts": fallback[:5],
    }
