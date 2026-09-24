"""Maps the Travel Planner tourism dataset onto the same generic intermediate document shape
`rag/step04_chunking.py` expects -- the tourism equivalent of `rag/step02_normalize_json.py`,
kept separate because the source schema, and the rules for interpreting it, are entirely
different from the craft datasets. Reuses `rag/step01_load_json.py` unchanged for loading.
"""

import json
from pathlib import Path
from typing import Any, Dict, List

import config

TOURISM_FILE = "bangladesh_heritage_tourism_dataset.json"


COORDINATES_FILE = "travel_place_coordinates.json"
# A district-centre match would put a pin on the wrong spot for a specific place; only
# place/spot/area-level matches (see geocode_places.py) are merged in.
_PINPOINT_PRECISIONS = ("place", "spot", "area")


def _load_coordinates(data_dir=None) -> Dict[str, Dict[str, Any]]:
    path = Path(data_dir or config.DATA_DIR) / COORDINATES_FILE
    if not path.is_file():
        return {}
    raw = json.loads(path.read_text(encoding="utf-8"))
    return {k: v for k, v in raw.items() if v.get("precision") in _PINPOINT_PRECISIONS}


def _place_doc(record: Dict[str, Any], coordinates: Dict[str, Any] = None) -> Dict[str, Any]:
    district = record.get("district")
    division = record.get("division")
    themes = record.get("themes") or []
    place_type = record.get("place_type") or ""

    return {
        "doc_id": f"{TOURISM_FILE}:{record['id']}",
        "doc_type": "place",
        "source_file": TOURISM_FILE,
        "record_id": record["id"],
        "craft_key": record["id"],           # generic entity key, step04's name for it
        "name_en": record["place_name"],
        "name_bn": None,
        "category_raw": place_type,
        "districts": [district] if district else [],
        "divisions": [division] if division else [],
        "area": record.get("upazila_or_area"),
        "themes": themes,
        "confidences": [],
        "source_ids": record.get("source_ids") or [],
        "is_gi": False,
        "is_unesco": "unesco" in " ".join(themes).lower() or "unesco" in place_type.lower(),
        # The raw record travels through as one "place" field group; rag/travel/clean.py
        # renders it into aspects (step 3's job for the craft datasets).
        "fields": {"place": {**record, "coordinates": (coordinates or {}).get(record["id"])}},
    }


def normalize_travel_json(datasets: Dict[str, Any]) -> List[Dict[str, Any]]:
    data = datasets.get(TOURISM_FILE) or {}
    places = data.get("places") or []
    coordinates = _load_coordinates()
    docs = [_place_doc(p, coordinates) for p in places if p.get("place_name") and p.get("id")]
    print(f"[2] Normalized : {len(docs)} tourism place record(s) from {TOURISM_FILE} "
          f"({sum(1 for p in places if p.get('id') in coordinates)} with approximate coordinates)")
    return docs
