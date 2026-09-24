"""Rebuild data/travel_facilities.json (the Travel Planner RAG's hotel/restaurant/attraction
input) from the backend's sourced tourism data, so the database and the RAG share one source:

    backend/src/ShilpoHubBD.Data/Seed/TourismData/<district>.json  ->  rag/data/travel_facilities.json
    python sync_facilities.py && python ingest_travel.py --recreate

Only records with coordinates are included (same rule as the backend seeder). A record counts as
verified in the RAG only when its status is "Verified" (an official source); anything else is
indexed as unverified, and every missing price/fee/hours is rendered as "not verified".
"""

import json
from pathlib import Path

import config

SEED_DIR = Path(__file__).resolve().parents[1] / "backend/src/ShilpoHubBD.Data/Seed/TourismData"
OUT = Path(config.DATA_DIR) / "travel_facilities.json"
KIND = {"Hotel": "hotel", "Resort": "resort", "Hostel": "hostel", "Restaurant": "restaurant",
        "TouristPlace": "attraction", "HeritageSite": "attraction", "Attraction": "attraction"}


def main() -> None:
    old = json.loads(OUT.read_text(encoding="utf-8")) if OUT.exists() else {}
    facilities = []
    for file in sorted(SEED_DIR.glob("*.json")):
        data = json.loads(file.read_text(encoding="utf-8"))
        for n, loc in enumerate(data["locations"], 1):
            if loc.get("latitude") is None or loc["type"] not in KIND:
                continue
            verified = loc.get("verificationStatus") == "Verified"
            facilities.append({
                "id": f"FAC-{file.stem.upper()}-{n:03d}",
                "name": loc["name"],
                "kind": KIND[loc["type"]],
                "district": data["district"],
                "area": loc.get("area"),
                "lat": loc["latitude"],
                "lng": loc["longitude"],
                "coordinates_precision": loc.get("coordinatesPrecision"),
                "price_bdt_per_night": loc.get("price"),
                "entry_fee_bdt": loc.get("entryFee"),
                "opening_hours": loc.get("openingHours"),
                "cuisine_or_facilities": [f.strip() for f in (loc.get("facilities") or "").split(",") if f.strip()],
                "contact": loc.get("contact"),
                "source": loc.get("sourceUrl") if verified else None,
                "verified_on": data["retrievedOn"] if verified else None,
            })
    out = {"_instructions": old.get("_instructions"), "_example": old.get("_example"),
           "_generated_by": "sync_facilities.py from the backend Seed/TourismData files - edit those, not this file",
           "facilities": facilities}
    OUT.write_text(json.dumps(out, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"wrote {len(facilities)} facilities ({sum(1 for f in facilities if f['verified_on'])} verified)")


if __name__ == "__main__":
    main()
