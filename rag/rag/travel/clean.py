"""Cleans and renders the normalized Travel Planner documents into aspect text, in the same
output shape `rag/step03_clean_data.py` produces for the craft datasets (so step04's chunker
works unmodified). Reuses `clean_text`/`clean_value` from step03 -- those are pure text/JSON
cleaning utilities with no craft-specific knowledge -- but the aspect renderers below are new:
the craft ones (`_overview_parts` etc.) are written against fields this dataset doesn't have.
"""

import hashlib
import re
from collections import OrderedDict
from typing import Any, Dict, List

from rag.step03_clean_data import clean_text, clean_value

_SPACES_RE = re.compile(r"\s+")


def _end(text: str) -> str:
    return text if text.endswith((".", "!", "?", "।")) else text + "."


def _pretty(value: str) -> str:
    return str(value).replace("_", " ").strip()


def _join(values: List[Any]) -> str:
    return "; ".join(str(v) for v in values if v)


def _overview_parts(place: Dict[str, Any]) -> List[str]:
    parts: List[str] = []
    place_type = place.get("place_type")
    if place_type:
        parts.append(f"Place type: {_pretty(place_type)}")
    if place.get("historical_or_cultural_context"):
        parts.append(_end(place["historical_or_cultural_context"]))
    if place.get("heritage"):
        parts.append(f"Heritage tags: {_join(place['heritage'])}.")
    if place.get("themes"):
        parts.append(f"Suits travellers interested in: {_join([_pretty(t) for t in place['themes']])}.")
    return parts


def _experience_parts(place: Dict[str, Any]) -> List[str]:
    parts: List[str] = []
    if place.get("tourist_experiences"):
        parts.append("What you can do here:\n" + "\n".join(f"- {e}" for e in place["tourist_experiences"]))
    if place.get("suggested_visit_hours"):
        parts.append(f"Suggested visit duration: about {place['suggested_visit_hours']} hour(s).")
    if place.get("suggested_season"):
        parts.append(f"Best season: {_end(place['suggested_season'])}")
    if place.get("cautions"):
        parts.append("Cautions: " + " ".join(_end(c) for c in place["cautions"]))
    return parts


def _access_parts(place: Dict[str, Any]) -> List[str]:
    parts: List[str] = []
    location_bits = [p for p in (place.get("upazila_or_area"), place.get("district"), place.get("division")) if p]
    if location_bits:
        parts.append(f"Location: {', '.join(location_bits)}.")
    if place.get("heritage_spots"):
        parts.append(f"Notable spots nearby: {_join(place['heritage_spots'])}.")
    if place.get("route_clusters"):
        parts.append(f"Part of suggested route(s): {_join(place['route_clusters'])}.")
    local_products = place.get("local_products") or []
    if local_products:
        names = [p.get("name") for p in local_products if isinstance(p, dict) and p.get("name")]
        if names:
            parts.append(f"Local products/food associated with this area: {_join(names)}.")
    coords = place.get("coordinates")
    if coords:
        parts.append(f"Approximate map position ({coords['precision']}-level OpenStreetMap match, unverified): "
                     f"{coords['lat']}, {coords['lng']}.")
    artisan = place.get("living_art_community") or {}
    artisan_places = artisan.get("artisan_places") or []
    if artisan_places:
        note = " (on-site demonstration not guaranteed)" if not artisan.get("demonstration_guaranteed") else ""
        parts.append(f"Living-artisan access: {_join(artisan_places)}{note}.")
    return parts


def _heritage_overview_parts(rec: Dict[str, Any]) -> List[str]:
    item = rec["item"]
    parts: List[str] = []
    if item.get("heritage_type"):
        kind = item["heritage_type"] + (f" / {item['subcategory']}" if item.get("subcategory") else "")
        parts.append(f"Heritage type: {kind}")
    if item.get("what_it_is"):
        parts.append(_end(item["what_it_is"]))
    if item.get("what_makes_it_special"):
        parts.append(_end(item["what_makes_it_special"]))
    recognition = item.get("recognition") or {}
    if recognition.get("unesco_ich"):
        ich = recognition["unesco_ich"]
        parts.append(f"UNESCO Intangible Cultural Heritage: {ich.get('element')} ({ich.get('year')}).")
    if recognition.get("gi"):
        parts.append(f"Geographical Indication: {recognition['gi'] if isinstance(recognition['gi'], str) else 'registered'}.")
    return parts


def _heritage_visit_parts(rec: Dict[str, Any]) -> List[str]:
    geo, occ = rec["geo"], rec["occurrence"]
    parts: List[str] = []
    bits = [b for b in (geo.get("locality"), geo.get("upazila_or_area"), geo.get("district"), geo.get("division")) if b]
    if bits:
        parts.append(f"Location: {', '.join(bits)}.")
    detail = occ.get("production_and_place_detail") or occ.get("specific_local_importance")
    if detail:
        parts.append(_end(detail))
    if occ.get("additional_place_history"):
        parts.append(_end(occ["additional_place_history"]))
    activity = occ.get("activity_at_source_date")
    if activity and activity != "unknown":
        parts.append(f"Craft activity status in source: {_pretty(activity)}.")
    if geo.get("location_type") in ("district_or_wider_region", "cross_district_or_unresolved", "regional_area"):
        parts.append("The exact site is not specified in the source (district or region level only).")
    if occ.get("special_cautions"):
        parts.append("Cautions: " + " ".join(_end(c) for c in occ["special_cautions"]))
    return parts


def _heritage_background_parts(rec: Dict[str, Any]) -> List[str]:
    item = rec["item"]
    parts: List[str] = []
    if item.get("history"):
        parts.append(_end(item["history"]))
    if item.get("heritage_story"):
        parts.append(_end(item["heritage_story"]))
    if item.get("typical_products"):
        parts.append(f"Typical products/food: {_join(item['typical_products'])}.")
    if item.get("materials_or_ingredients"):
        parts.append(f"Materials/ingredients: {_join(item['materials_or_ingredients'])}.")
    if item.get("traditional_methods"):
        parts.append(f"Traditional methods: {_join(item['traditional_methods'])}.")
    return parts


def _unverified(label: str, value: Any, unit: str = "") -> str:
    return f"{label}: {value}{unit}." if value not in (None, "", []) else f"{label}: not verified."


def _facility_overview_parts(rec: Dict[str, Any]) -> List[str]:
    where = ", ".join(b for b in (rec.get("area"), rec.get("district")) if b)
    parts = [f"{_pretty(rec['kind']).title()} in {where}."]
    if rec.get("cuisine_or_facilities"):
        parts.append(f"Cuisine/facilities: {_join(rec['cuisine_or_facilities'])}.")
    if not rec["_verified"]:
        parts.append("Listing has no recorded source/verification date - treat every detail as unconfirmed.")
    return parts


def _facility_practical_parts(rec: Dict[str, Any]) -> List[str]:
    parts = [
        _unverified("Price per night", rec.get("price_bdt_per_night"), " BDT") if rec["kind"] in
        ("hotel", "resort", "hostel", "guest_house") else None,
        _unverified("Entry fee", rec.get("entry_fee_bdt"), " BDT") if rec["kind"] == "attraction" else None,
        _unverified("Opening hours", rec.get("opening_hours")),
        _unverified("Contact", rec.get("contact")),
    ]
    if rec.get("lat") is not None and rec.get("lng") is not None:
        precision = {"geocoded_approximate": " (approximate, geocoded from OpenStreetMap)",
                     "community_listing": " (from a community listing, unverified)",
                     "official_listing": " (official listing)"}.get(rec.get("coordinates_precision"), "")
        parts.append(f"Coordinates: {rec['lat']}, {rec['lng']}{precision}.")
    if rec.get("verified_on"):
        parts.append(f"Verified on {rec['verified_on']} (source: {rec.get('source')}).")
    return [p for p in parts if p]


_BUILDERS = {
    "facility": (("overview", _facility_overview_parts), ("practical", _facility_practical_parts)),
    "place": (("overview", _overview_parts), ("experience", _experience_parts), ("access", _access_parts)),
    "heritage_place": (("overview", _heritage_overview_parts), ("visit", _heritage_visit_parts),
                       ("background", _heritage_background_parts)),
}


def _content_hash(doc: Dict[str, Any]) -> str:
    text = _SPACES_RE.sub(" ", doc["content"]).casefold()
    return hashlib.sha1(f"{doc['doc_type']}|{doc['craft_key']}|{text}".encode("utf-8")).hexdigest()


def clean_travel_data(docs: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    cleaned: List[Dict[str, Any]] = []
    seen_ids, seen_content = set(), set()
    empty = duplicates = 0

    for doc in docs:
        place = doc["fields"][doc["doc_type"]]
        aspects: "OrderedDict[str, List[str]]" = OrderedDict()
        for aspect, builder in _BUILDERS[doc["doc_type"]]:
            parts = [clean_text(p) for p in builder(place) if p and clean_text(p)]
            if parts:
                aspects[aspect] = parts

        name_en = clean_value(doc["name_en"])
        if not name_en or not aspects:
            empty += 1
            continue

        doc = dict(doc)
        doc.update({
            "name_en": name_en,
            "name_bn": clean_value(doc.get("name_bn")),
            "category_raw": clean_value(doc.get("category_raw")),
            "category_norm": doc.get("category_norm") or doc.get("category_raw"),   # place_type / heritage_type = filterable category
            "risk_level": None,
            "districts": clean_value(doc.get("districts")) or [],
            "divisions": clean_value(doc.get("divisions")) or [],
            "confidences": [],
            "source_ids": clean_value(doc.get("source_ids")) or [],
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

    print(f"[3] Cleaned    : {len(cleaned)} tourism place document(s) kept, {empty} empty dropped, "
          f"{duplicates} duplicate(s) dropped")
    return cleaned
