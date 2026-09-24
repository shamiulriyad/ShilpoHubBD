"""Maps `travel_facilities.json` -- hand-curated hotels, restaurants and attractions with the
facts the two heritage datasets do not carry (price, entry fee, opening hours, coordinates) --
onto the same intermediate document shape as `normalize.py`.

The file is optional and ships empty. Every field that is missing is rendered as "not
verified" rather than left out, so retrieval can never hand Gemini a hotel whose price it then
has to guess.
"""

from pathlib import Path
from typing import Any, Dict, List

FACILITIES_FILE = "travel_facilities.json"

_KIND_THEMES = {
    "hotel": ["accommodation"], "resort": ["accommodation"], "hostel": ["accommodation"],
    "guest_house": ["accommodation"], "restaurant": ["food", "dining"], "attraction": ["attraction"],
}


def facilities_available(data_dir) -> bool:
    return (Path(data_dir) / FACILITIES_FILE).is_file()


def normalize_facilities_json(datasets: Dict[str, Any]) -> List[Dict[str, Any]]:
    docs: List[Dict[str, Any]] = []
    for rec in (datasets.get(FACILITIES_FILE) or {}).get("facilities") or []:
        if not (rec.get("id") and rec.get("name") and rec.get("district") and rec.get("kind")):
            continue
        kind = rec["kind"]
        verified = bool(rec.get("source") and rec.get("verified_on"))
        docs.append({
            "doc_id": f"{FACILITIES_FILE}:{rec['id']}",
            "doc_type": "facility",
            "source_file": FACILITIES_FILE,
            "record_id": rec["id"],
            "craft_key": rec["id"],
            "name_en": rec["name"],
            "name_bn": None,
            "category_raw": kind,
            "category_norm": kind,
            "districts": [rec["district"]],
            "divisions": [],
            "area": rec.get("area"),
            "themes": _KIND_THEMES.get(kind, []) + ([] if verified else ["unverified"]),
            "confidences": [],
            "source_ids": [rec["source"]] if rec.get("source") else [],
            "is_gi": False,
            "is_unesco": False,
            "fields": {"facility": {**rec, "_verified": verified}},
        })
    print(f"[2] Normalized : {len(docs)} facility record(s) from {FACILITIES_FILE}")
    return docs
