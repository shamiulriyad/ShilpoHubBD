"""STEP 2 - Normalize the JSON datasets into one document format.

craft.json, craftDetails.json and GEO.json describe crafts with three different schemas
(different field names, structured vs free-text regions, different id schemes). This step
maps them onto ONE shape and keeps the structured fields - step 3 cleans and renders them,
step 4 chunks them by aspect.

A craft document (one per record, 94 in all):

    {
      "doc_id": "craftDetails.json:jamdani",   "doc_type": "craft",
      "source_file": "craftDetails.json",      "record_id": "jamdani",
      "craft_key": "jamdani",                  # shared by the craft's records in every file
      "name_en": "Jamdani",                    "name_bn": "...",
      "districts": ["Narayanganj"],            "divisions": ["Dhaka"],
      "confidences": ["verified"],             "source_ids": ["SRC_UNESCO_JAMDANI"],
      "is_unesco": True,                       "is_gi": True,       # craft-level, see below
      "fields": {"overview": {...}, "production": {...}, "geography": {...}},
    }

plus four reference documents from GEO.json: REF-UNESCO, REF-SITES, REF-TAXONOMY,
REF-SOURCES (doc_type "reference", no craft_key).

Districts and divisions
  * structured regions (craft.json, GEO.json) give the district / division directly;
  * craftDetails.json regions are prose, so the 64 district names from GEO.json
    (geographic_coverage) are matched inside the text, old spellings included;
  * a division is added for every district found (from the same GEO.json table).

`is_unesco` / `is_gi` are facts about the craft, so they are set on every record of the craft:
`is_unesco` from GEO.json's UNESCO list only, `is_gi` when "GI" appears in a record's
description / notes / heritage status.
"""

import re
from collections import OrderedDict
from typing import Any, Dict, List

from rag import craft_keys

# Old / alternate spellings -> the name GEO.json uses. (Step 9 reuses this table.)
DISTRICT_ALIASES = {
    "chittagong": "Chattogram",
    "comilla": "Cumilla",
    "barisal": "Barishal",
    "jessore": "Jashore",
    "bogra": "Bogura",
    "khagrachari": "Khagrachhari",
    "netrakona": "Netrokona",
    "cox bazar": "Cox's Bazar",
    "coxs bazar": "Cox's Bazar",
    "chapai nawabganj": "Chapainawabganj",
    "maulvibazar": "Moulvibazar",
    "hobiganj": "Habiganj",
    "jhalakathi": "Jhalokathi",
}
# A region, not a district: "Chattogram Hill Tracts (Rangamati, Bandarban...)" must not
# also tag the Chattogram district.
_HILL_TRACTS = (" chattogram hill tracts ", " chittagong hill tracts ")
_GI = re.compile(r"\bGI\b")

_words = craft_keys.words


class _Places:
    """The 64 districts and 8 divisions of GEO.json, and recognition of them in text."""

    def __init__(self, datasets: Dict[str, Any]):
        divisions = ((datasets.get("GEO.json") or {}).get("geographic_coverage") or {}).get("divisions") or {}
        self._divisions = set(divisions)
        self._canon: Dict[str, str] = {}
        self._division_of: Dict[str, str] = {}
        for division, districts in divisions.items():
            for district in districts:
                self._canon[_words(district)] = district
                self._division_of[district] = division
        for alias, target in DISTRICT_ALIASES.items():
            if _words(target) in self._canon:
                self._canon[_words(alias)] = self._canon[_words(target)]

    def district(self, name: Any) -> str:
        text = str(name or "").strip()
        return self._canon.get(_words(text), text)

    def division_of(self, district: str) -> str:
        return self._division_of.get(district, "")

    def is_district(self, name: Any) -> bool:
        return _words(name) in self._canon

    def is_division(self, name: Any) -> bool:
        return name in self._divisions

    def region(self, division: Any, district: Any, area: Any) -> Dict[str, Any]:
        """A region entry with only REAL districts and divisions in those fields.

        craft.json and GEO.json write district "Nationwide" and division "Bangladesh" for a craft
        practised everywhere. That is a scope, not a place: left as a district it would read
        "Nationwide district, Bangladesh division" and pollute the districts / divisions the query
        router filters on. So it moves into `area` ("Nationwide") and out of the two fields."""
        district = self.district(district) if district else None
        if district and not self.is_district(district):
            area = area if area and district.lower() in str(area).lower() else \
                f"{district} - {area}" if area else district
            district = None
        return {"division": division if division and self.is_division(division) else None,
                "district": district, "area": area}

    def find(self, *texts: Any) -> List[str]:
        """Districts named in the text, in order of appearance."""
        padded = f" {_words(' ; '.join(str(t) for t in texts if t))} "
        for phrase in _HILL_TRACTS:
            padded = padded.replace(phrase, " ")
        hits = sorted((padded.find(f" {key} "), name) for key, name in self._canon.items()
                      if f" {key} " in padded)
        return _unique(name for _pos, name in hits)


def _unique(items) -> List[str]:
    out: List[str] = []
    for item in items:
        if item and item not in out:
            out.append(item)
    return out


def _doc_places(places: _Places, districts: List[str], divisions: List[str] = ()) -> Dict[str, List[str]]:
    districts = _unique(districts)
    return {"districts": districts,
            "divisions": _unique([*divisions, *(places.division_of(d) for d in districts)])}


def _craft_doc(*, source_file, record_id, name_en, name_bn, category_raw, places_info,
               confidences, source_ids, is_gi, fields) -> Dict[str, Any]:
    return {
        "doc_id": f"{source_file}:{record_id}",
        "doc_type": "craft",
        "source_file": source_file,
        "record_id": str(record_id),
        "craft_key": craft_keys.craft_key_for(name_en, record_id, source_file),
        "name_en": name_en,
        "name_bn": name_bn,
        "category_raw": category_raw,
        **places_info,
        "confidences": _unique(str(c) for c in confidences if c),
        "source_ids": _unique(source_ids),
        "is_gi": is_gi,
        "is_unesco": False,           # set craft-wide in _link_crafts
        "fields": fields,
    }


# --- craft.json --------------------------------------------------------------------

def _from_craft_json(record: dict, source_file: str, places: _Places, registry: dict) -> Dict[str, Any]:
    location = [
        {**places.region(r.get("division"), r.get("district"), r.get("area")),
         "association": r.get("association"), "confidence": r.get("confidence")}
        for r in (record.get("regions") or []) if isinstance(r, dict)
    ]
    labels = record.get("sources") or []
    return _craft_doc(
        source_file=source_file, record_id=record.get("id"), name_en=record["name_en"],
        name_bn=record.get("name_bn"), category_raw=record.get("category"),
        places_info=_doc_places(places, [loc["district"] for loc in location if loc["district"]],
                                [loc["division"] for loc in location if loc["division"]]),
        confidences=[record.get("confidence"), *(loc["confidence"] for loc in location)],
        source_ids=[craft_keys.resolve_source_id(s, registry) or s for s in labels],
        is_gi=False,
        fields=OrderedDict([
            ("overview", OrderedDict([("category_raw", record.get("category")),
                                      ("sources", labels),
                                      ("data_confidence", record.get("confidence"))])),
            ("production", OrderedDict([("how_made", record.get("how_made")),
                                        ("materials", record.get("materials")),
                                        ("tools", record.get("traditional_tools")),
                                        ("techniques", record.get("traditional_techniques")),
                                        ("time_required", record.get("time_required"))])),
            ("geography", OrderedDict([("location", location)])),
        ]),
    )


# --- craftDetails.json -----------------------------------------------------------------

def _from_craft_details(record: dict, source_file: str, places: _Places, registry: dict) -> Dict[str, Any]:
    regions = [r for r in (record.get("regions") or []) if r]
    hub = record.get("primary_hub")
    text_gi = " ".join(str(record.get(k) or "") for k in ("description", "notes"))
    return _craft_doc(
        source_file=source_file, record_id=record.get("id"), name_en=record["name_en"],
        name_bn=record.get("name_bn"), category_raw=record.get("category"),
        # Regions are prose ("Narayanganj - Rupganj, Sonargaon..."): find the districts in the text.
        places_info=_doc_places(places, places.find(*regions, hub)),
        confidences=[record.get("source_confidence")],
        source_ids=[],
        is_gi=bool(_GI.search(text_gi)),
        fields=OrderedDict([
            ("overview", OrderedDict([("category_raw", record.get("category")),
                                      ("description", record.get("description")),
                                      ("notes", record.get("notes")),
                                      ("skill_transmission", record.get("skill_transmission")),
                                      ("data_confidence", record.get("source_confidence"))])),
            ("production", OrderedDict([("how_made", record.get("technique_steps")),
                                        ("materials", record.get("materials")),
                                        ("tools", record.get("tools")),
                                        ("time_required", record.get("time_required"))])),
            ("geography", OrderedDict([("location", [{"text": r} for r in regions]),
                                       ("primary_hub", hub),
                                       ("endangerment", record.get("endangerment_level"))])),
        ]),
    )


# --- GEO.json ------------------------------------------------------------------------------

def _from_geo_craft(record: dict, source_file: str, places: _Places, registry: dict) -> Dict[str, Any]:
    names = record.get("name") or {}
    name_en = names.get("en") if isinstance(names, dict) else str(names)
    name_bn = names.get("bn") if isinstance(names, dict) else None

    location = [
        {**places.region(a.get("division"), a.get("district"), a.get("upazila_or_area")),
         "village": a.get("village_or_cluster"),
         "association": a.get("association_type"), "confidence": a.get("confidence")}
        for a in (record.get("geographical_associations") or []) if isinstance(a, dict)
    ]
    ids = record.get("sources") or []
    titles = [f"{registry[i]['title']} (tier {registry[i].get('tier', '?')})" if i in registry else i for i in ids]
    verification = (record.get("verification") or {}).get("overall")

    return _craft_doc(
        source_file=source_file, record_id=record.get("id"), name_en=name_en, name_bn=name_bn,
        category_raw=record.get("category"),
        places_info=_doc_places(places, [loc["district"] for loc in location if loc["district"]],
                                [loc["division"] for loc in location if loc["division"]]),
        confidences=[loc["confidence"] for loc in location],
        source_ids=ids,
        is_gi=bool(_GI.search(str(record.get("heritage_status") or ""))),
        fields=OrderedDict([
            ("overview", OrderedDict([("category_raw", record.get("category")),
                                      ("subcategory", record.get("subcategory")),
                                      ("heritage_status", record.get("heritage_status")),
                                      ("sources", titles),
                                      ("verification", str(verification or "").replace("_", " "))])),
            ("production", OrderedDict([("materials", record.get("materials")),
                                        ("techniques", record.get("techniques")),
                                        ("products", record.get("products_or_outputs"))])),
            ("geography", OrderedDict([("location", location)])),
        ]),
    )


# --- GEO.json reference sections ---------------------------------------------------------------

def _ref_doc(ref_id: str, title: str, places: _Places, lines: List[str], districts: List[str],
             divisions: List[str] = (), source_ids: List[str] = (), is_unesco: bool = False) -> Dict[str, Any]:
    return {
        "doc_id": ref_id, "doc_type": "reference", "source_file": "GEO.json", "record_id": ref_id,
        "craft_key": None, "name_en": title, "name_bn": None, "category_raw": None,
        **_doc_places(places, districts, divisions),
        "confidences": [], "source_ids": list(source_ids), "is_gi": False, "is_unesco": is_unesco,
        # One line per item: step 4 packs whole lines into chunks, so an item is never cut in half.
        "fields": OrderedDict([("reference", OrderedDict([("lines", lines)]))]),
    }


def _reference_docs(geo: dict, places: _Places) -> List[Dict[str, Any]]:
    """REF-UNESCO, REF-SITES, REF-TAXONOMY, REF-SOURCES. The 64 `district_records` are empty
    scaffolds (no craft records in any of them) and are not indexed."""
    docs: List[Dict[str, Any]] = []

    unesco = [u for u in geo.get("unesco_bangladesh_intangible_heritage") or [] if isinstance(u, dict)]
    if unesco:
        lines = [f"{u.get('name')} (inscribed {u.get('year')})" if u.get("year") else str(u.get("name"))
                 for u in unesco]
        docs.append(_ref_doc("REF-UNESCO", "UNESCO Intangible Cultural Heritage of Bangladesh", places,
                             lines, places.find(*lines), is_unesco=True))

    sites = [s for s in geo.get("heritage_sites") or [] if isinstance(s, dict)]
    if sites:
        lines = []
        for s in sites:
            place = ", ".join(p for p in (s.get("area"),
                                          f"{places.district(s['district'])} district" if s.get("district") else None,
                                          f"{s['division']} division" if s.get("division") else None) if p)
            line = f"{s.get('name_en')} ({str(s.get('type', 'site')).replace('_', ' ')}) - {place}"
            if s.get("associated_craft"):
                line += f"; associated craft: {s['associated_craft']}"
            lines.append(line)
        docs.append(_ref_doc("REF-SITES", "Heritage sites and institutions linked to Bangladeshi crafts", places,
                             lines, [places.district(s["district"]) for s in sites if s.get("district")],
                             [s["division"] for s in sites if s.get("division")]))

    taxonomy = geo.get("craft_taxonomy")
    if isinstance(taxonomy, dict) and taxonomy:
        lines = [f"{family}: {', '.join(str(c) for c in crafts)}" for family, crafts in taxonomy.items()]
        docs.append(_ref_doc("REF-TAXONOMY", "Bangladeshi craft families", places, lines, []))

    registry = [s for s in geo.get("source_registry") or [] if isinstance(s, dict) and s.get("id")]
    if registry:
        lines = [f"{s['id']}: {s.get('title')} - {s.get('authority', 'unknown authority')}, tier {s.get('tier', '?')}"
                 for s in registry]
        levels = geo.get("verification_levels") or {}
        lines += [f"Verification tier {tier}: {meaning}" for tier, meaning in levels.items()]
        docs.append(_ref_doc("REF-SOURCES", "ShilpoHub source registry and verification tiers", places,
                             lines, [], source_ids=[s["id"] for s in registry]))
    return docs


# --- linking, entry point -------------------------------------------------------------------

def _link_crafts(docs: List[Dict[str, Any]], geo: dict) -> None:
    """Set the craft-wide flags on every record of the craft."""
    unesco_keys = {craft_keys.UNESCO_ENTRY_KEYS.get(_words(u.get("name")))
                   for u in geo.get("unesco_bangladesh_intangible_heritage") or [] if isinstance(u, dict)}
    gi_keys = {d["craft_key"] for d in docs if d["doc_type"] == "craft" and d["is_gi"]}
    for doc in docs:
        if doc["doc_type"] == "craft":
            doc["is_unesco"] = doc["craft_key"] in unesco_keys
            doc["is_gi"] = doc["craft_key"] in gi_keys


def normalize_json(datasets: Dict[str, Any]) -> List[Dict[str, Any]]:
    places = _Places(datasets)
    geo = datasets.get("GEO.json") or {}
    registry = {s["id"]: s for s in geo.get("source_registry") or [] if isinstance(s, dict) and s.get("id")}

    docs: List[Dict[str, Any]] = []
    unmapped: List[str] = []
    skipped = 0

    def add(build, source_file: str, records) -> None:
        nonlocal skipped
        for record in records or []:
            names = record.get("name") if isinstance(record, dict) else None
            name = record.get("name_en") or (names.get("en") if isinstance(names, dict) else None) \
                if isinstance(record, dict) else None
            if not name:                      # nothing to title or cite it by
                skipped += 1
                continue
            doc = build(record, source_file, places, registry)
            if not craft_keys.is_mapped(name, source_file):
                unmapped.append(f"{source_file}:{name}->{doc['craft_key']}")
            docs.append(doc)

    add(_from_craft_json, "craft.json", (datasets.get("craft.json") or {}).get("crafts"))
    add(_from_craft_details, "craftDetails.json", (datasets.get("craftDetails.json") or {}).get("crafts"))
    add(_from_geo_craft, "GEO.json", geo.get("crafts"))
    if geo:
        docs.extend(_reference_docs(geo, places))
    _link_crafts(docs, geo)

    crafts = [d for d in docs if d["doc_type"] == "craft"]
    keys = {d["craft_key"] for d in crafts}
    print(f"[2] Normalized : {len(crafts)} craft records -> {len(keys)} distinct craft_keys, "
          f"{len(docs) - len(crafts)} reference docs; {sum(1 for d in crafts if d['districts'])} records tagged with a district")
    if unmapped:
        print(f"    ! no alias for {len(unmapped)} name(s), used a slug of the name: {', '.join(unmapped)}")
    if skipped:
        print(f"    ! {skipped} nameless record(s) skipped")
    return docs
