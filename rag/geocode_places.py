"""One-off enrichment: look up approximate coordinates for the Travel Planner tourism places
(the dataset states that it has none) from OpenStreetMap Nominatim, and write them to
data/travel_place_coordinates.json for rag/travel/normalize.py to merge in.

    python geocode_places.py            # resumable: places already in the output are skipped

Each place is tried, in order, as: the place name, its first named heritage spot, then its
upazila/area -- always qualified by district + Bangladesh. The result is only accepted if it
falls inside Bangladesh, and is labelled with how it was found ("place", "spot" or "area") so a
district-level guess is never presented as a pinpoint. Nothing here is verified by a human:
every entry is stored as approximate/unverified. Respects Nominatim's 1 request/second policy.
"""

import json
import time
import urllib.parse
import urllib.request
from pathlib import Path

import config

SOURCE = Path(config.DATA_DIR) / "bangladesh_heritage_tourism_dataset.json"
OUT = Path(config.DATA_DIR) / "travel_place_coordinates.json"
USER_AGENT = "ShilpoHubBD-TravelPlanner/1.0 (dataset enrichment)"
LAT_RANGE, LNG_RANGE = (20.5, 26.7), (88.0, 92.8)      # Bangladesh bounding box


def _search(query: str):
    url = "https://nominatim.openstreetmap.org/search?" + urllib.parse.urlencode(
        {"q": query, "format": "jsonv2", "limit": 1, "countrycodes": "bd"})
    with urllib.request.urlopen(urllib.request.Request(url, headers={"User-Agent": USER_AGENT}), timeout=20) as resp:
        hits = json.load(resp)
    time.sleep(1.1)
    if not hits:
        return None
    lat, lng = float(hits[0]["lat"]), float(hits[0]["lon"])
    if not (LAT_RANGE[0] <= lat <= LAT_RANGE[1] and LNG_RANGE[0] <= lng <= LNG_RANGE[1]):
        return None
    return {"lat": round(lat, 5), "lng": round(lng, 5), "matched": hits[0].get("display_name")}


def main() -> None:
    config.ensure_utf8_console()
    places = json.loads(SOURCE.read_text(encoding="utf-8-sig"))["places"]
    done = json.loads(OUT.read_text(encoding="utf-8")) if OUT.exists() else {}

    for n, place in enumerate(places, 1):
        if place["id"] in done:
            continue
        district = place["district"]
        attempts = [("place", f"{place['place_name']}, {district}, Bangladesh")]
        if place.get("heritage_spots"):
            attempts.append(("spot", f"{place['heritage_spots'][0]}, {district}, Bangladesh"))
        if place.get("upazila_or_area"):
            attempts.append(("area", f"{place['upazila_or_area']}, {district}, Bangladesh"))
        attempts.append(("district", f"{district}, Bangladesh"))

        entry = {"status": "not_found"}
        for precision, query in attempts:
            try:
                found = _search(query)
            except Exception as exc:  # noqa: BLE001 - keep going; the run is resumable
                print(f"  ! {place['id']} {precision}: {exc}")
                continue
            if found:
                entry = {"status": "approximate_unverified", "precision": precision, "query": query, **found}
                break
        done[place["id"]] = entry
        OUT.write_text(json.dumps(done, ensure_ascii=False, indent=1), encoding="utf-8")
        print(f"[{n}/{len(places)}] {place['id']} {entry.get('precision', '-')} {entry.get('lat', '')} {entry.get('lng', '')}")

    counts = {}
    for entry in done.values():
        key = entry.get("precision", "not_found")
        counts[key] = counts.get(key, 0) + 1
    print("Done:", counts)


if __name__ == "__main__":
    main()
