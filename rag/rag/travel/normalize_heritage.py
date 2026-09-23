"""Maps the second Travel Planner dataset, `Bangladesh_Heritage_Dataset.json`, onto the same
generic intermediate document shape as `normalize.py` -- one document per *place occurrence*
of a heritage item (craft-location rows and other-heritage-place rows), joined to its
`heritage_items` record by `heritage_id`. That is the shape a trip planner needs: "what
heritage exists in this district / locality", not "everything about Jamdani".

Nothing here invents data: the dataset states that all coordinates are null and that null
history means unresearched, so those are never rendered as facts.
"""

import re
from typing import Any, Dict, List

HERITAGE_FILE = "Bangladesh_Heritage_Dataset.json"

# heritage_type keyword -> theme tags, using the same vocabulary as the tourism dataset's
# `themes` so an interest filter matches places from both files.
_TYPE_THEMES = (
    ("craft", ["living_craft"]),
    ("natural", ["nature"]),
    ("food", ["food"]),
    ("river", ["river"]),
    ("archaeolog", ["archaeology", "history"]),
    ("architect", ["architecture", "history"]),
    ("historical", ["history"]),
    ("intangible", ["living_culture"]),
    ("agricultur", ["agriculture"]),
    ("market", ["market"]),
)


def _slug(text: str) -> str:
    return re.sub(r"[^a-z0-9]+", "_", (text or "").casefold()).strip("_")


def _themes(item: Dict[str, Any]) -> List[str]:
    heritage_type = (item.get("heritage_type") or "").casefold()
    themes: List[str] = []
    for keyword, tags in _TYPE_THEMES:
        if keyword in heritage_type:
            themes += [t for t in tags if t not in themes]
    recognition = item.get("recognition") or {}
    if recognition.get("gi"):
        themes.append("GI")
    if recognition.get("unesco_ich"):
        themes.append("UNESCO_intangible")
    return themes


def _doc(occurrence: Dict[str, Any], item: Dict[str, Any], *, kind: str, geo: Dict[str, Any]) -> Dict[str, Any]:
    district, division = geo.get("district"), geo.get("division")
    recognition = item.get("recognition") or {}
    return {
        "doc_id": f"{HERITAGE_FILE}:{occurrence['id']}",
        "doc_type": "heritage_place",
        "source_file": HERITAGE_FILE,
        "record_id": occurrence["id"],
        "craft_key": occurrence["id"],
        "name_en": item["name_en"],
        "name_bn": item.get("name_bn"),
        "category_raw": item.get("heritage_type"),
        "category_norm": _slug(item.get("heritage_type") or "") or None,
        "districts": [district] if district else [],
        "divisions": [division] if division else [],
        "area": geo.get("upazila_or_area"),
        "themes": _themes(item),
        "confidences": [],
        "source_ids": occurrence.get("source_ids") or item.get("source_ids") or [],
        "is_gi": bool(recognition.get("gi")),
        "is_unesco": bool(recognition.get("unesco_ich") or recognition.get("unesco_world")),
        "fields": {"heritage_place": {"kind": kind, "item": item, "occurrence": occurrence, "geo": geo}},
    }


def normalize_heritage_json(datasets: Dict[str, Any]) -> List[Dict[str, Any]]:
    data = datasets.get(HERITAGE_FILE) or {}
    items = {h["id"]: h for h in data.get("heritage_items") or [] if h.get("id") and h.get("name_en")}

    docs: List[Dict[str, Any]] = []
    for occ in data.get("craft_location_occurrences") or []:
        item = items.get(occ.get("heritage_id"))
        if item and occ.get("district"):
            geo = {
                "district": occ["district"], "division": occ.get("division"),
                "upazila_or_area": occ.get("upazila_or_area_as_reported"),
                "locality": occ.get("village_or_locality_as_reported"),
            }
            docs.append(_doc(occ, item, kind="craft_location", geo=geo))

    for occ in data.get("other_heritage_place_occurrences") or []:
        item = items.get(occ.get("heritage_id"))
        geo = occ.get("geography") or {}
        if item and geo.get("district"):
            geo = {**geo, "locality": geo.get("village_or_locality")}
            docs.append(_doc(occ, item, kind="heritage_place", geo=geo))

    print(f"[2] Normalized : {len(docs)} heritage place occurrence(s) from {HERITAGE_FILE}")
    return docs
