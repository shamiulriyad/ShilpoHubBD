"""Fill missing coordinates in a tourism data file (backend/.../Seed/TourismData/<district>.json)
from OpenStreetMap Nominatim. Only records that have a `geocodeQuery` and no latitude/longitude
are touched; anything with source-listed coordinates is never overwritten.

    python database/tourism/geocode_tourism_data.py coxsbazar.json

Results are labelled coordinatesPrecision = "geocoded_approximate" -- an area/landmark match, not
a surveyed point -- and must not be presented as exact. A hit outside the district's bounding box
(--bbox lat_min lat_max lng_min lng_max, default Bangladesh) is rejected and the record is left
without coordinates (the seeder skips records that still have none). 1 request/second.
"""

import argparse
import json
import sys
import time
import urllib.parse
import urllib.request
from pathlib import Path

DATA_DIR = Path(__file__).resolve().parents[2] / "backend/src/ShilpoHubBD.Data/Seed/TourismData"
UA = "ShilpoHubBD-TravelPlanner/1.0 (tourism data enrichment)"


def search(query, bbox, cache):
    if query in cache:
        return cache[query]
    url = "https://nominatim.openstreetmap.org/search?" + urllib.parse.urlencode(
        {"q": query, "format": "jsonv2", "limit": 1, "countrycodes": "bd"})
    with urllib.request.urlopen(urllib.request.Request(url, headers={"User-Agent": UA}), timeout=20) as r:
        hits = json.load(r)
    time.sleep(1.1)
    result = None
    if hits:
        lat, lng = float(hits[0]["lat"]), float(hits[0]["lon"])
        if bbox[0] <= lat <= bbox[1] and bbox[2] <= lng <= bbox[3]:
            result = (round(lat, 5), round(lng, 5), hits[0].get("display_name", ""))
    cache[query] = result
    return result


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--bbox", nargs=4, type=float, default=[20.5, 26.7, 88.0, 92.8])
    args = ap.parse_args()
    path = DATA_DIR / args.file
    data = json.loads(path.read_text(encoding="utf-8"))
    cache, filled, missed = {}, 0, []
    for loc in data["locations"]:
        if loc.get("latitude") is not None or not loc.get("geocodeQuery"):
            continue
        hit = search(loc["geocodeQuery"], args.bbox, cache)
        if not hit:
            missed.append(loc["name"])
            continue
        loc["latitude"], loc["longitude"] = hit[0], hit[1]
        loc["coordinatesSource"] = f"OpenStreetMap Nominatim, query: {loc['geocodeQuery']}"
        loc["coordinatesPrecision"] = "geocoded_approximate"
        filled += 1
        print(f"{loc['name']}: {hit[0]}, {hit[1]}  <- {hit[2][:70]}")
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"\nfilled {filled}; no usable match: {missed or 'none'}")


if __name__ == "__main__":
    main()
