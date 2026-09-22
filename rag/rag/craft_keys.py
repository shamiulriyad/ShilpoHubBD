"""craft_key: one stable id per craft, shared by every record of that craft.

The same craft appears in up to three files under different names and ids (Jamdani is
BDCP-001 in craft.json, `jamdani` in craftDetails.json, BDV2-001 in GEO.json). Query time
filters on `craft_key`, so all of them must agree on it.

Rule: craftDetails.json already has clean slug ids for 55 crafts - those ARE the canonical
keys. craft.json and GEO.json name their crafts differently, so `ALIASES` folds each of
their names into a canonical key. A name that is not listed keeps a slug of its own name (and
step 2 prints it), so nothing is ever silently merged or dropped.

Merges that are a judgement call are marked below. Names with no clear counterpart
(Satrangi, Baul Instrument Craft, Traditional Folk Instruments) get their own key rather than
a doubtful merge; the query analysis is told it may return several related keys.
"""

import re
from typing import Any, Optional

# normalized name (lowercase words) -> canonical craft_key
ALIASES = {
    # exact counterparts of a craftDetails.json craft
    "jamdani": "jamdani",
    "tangail saree": "tangail_saree",
    "shital pati": "shital_pati",
    "nakshi kantha": "nakshi_kantha",
    "bamboo craft": "bamboo_crafts",
    "cane and rattan craft": "cane_crafts",
    "jute craft": "jute_crafts",
    "pottery": "pottery",
    "terracotta art": "terracotta",
    "clay toys and dolls": "clay_toys",
    "bell metal and brass craft": "brass_bell_metal",
    "alpana": "alpana",
    "traditional boat making": "traditional_boat_building",
    "patachitra": "potchitra_scroll_painting",
    "rajshahi silk": "rajshahi_silk",
    "khadi": "khadi",
    "manipuri weaving": "monipuri_weaving",
    "conch shell craft": "shell_crafts",
    "rickshaw painting": "rickshaw_art",
    "rickshaw art and rickshaw painting": "rickshaw_art",
    # judgement calls: same craft, different wording
    "woodwork and wood carving": "wooden_furniture_crafts",   # craftDetails: "Wooden Crafts & Furniture"
    "nakshi pakha": "hand_fans",                               # "nakshi" = decorated, "pakha" = fan
    # no clear counterpart: keep separate
    "satrangi": "satrangi",
    "traditional folk instruments": "folk_instruments",
    "baul instrument craft": "baul_instruments",
}

# GEO.json's UNESCO list (normalized entry name) -> the craft it inscribes. Entries that are
# not crafts (Baul songs, Mangal Shobhajatra) are deliberately absent. GEO's "pending files"
# notes on Bell Metal and Boat Making are NOT inscriptions, so they do not count.
UNESCO_ENTRY_KEYS = {
    "traditional art of jamdani weaving": "jamdani",
    "traditional art of shital pati weaving of sylhet": "shital_pati",
    "rickshaws and rickshaw painting in dhaka": "rickshaw_art",
    "traditional saree weaving art of tangail": "tangail_saree",
}

# craft.json's free-text source labels -> GEO.json source_registry ids (only where they
# clearly name the same source; anything else keeps its own text as its id).
SOURCE_ALIASES = {
    "unesco bangladesh intangible heritage list": "SRC_UNESCO_BD",
    "tangail district administration heritage information": "SRC_TANGAIL_GOV",
}


def words(text: Any) -> str:
    """Lowercase alphanumeric words, single-spaced - the key every name is matched on."""
    return " ".join(re.findall(r"[a-z0-9]+", str(text).lower()))


def slug(text: Any) -> str:
    return words(text).replace(" ", "_")


def craft_key_for(name_en: str, record_id: Any, source_file: str) -> str:
    """craftDetails.json: its own id. Other files: the alias for the name, else a slug."""
    if source_file == "craftDetails.json" and record_id:
        return str(record_id)
    return ALIASES.get(words(name_en)) or slug(name_en)


def is_mapped(name_en: str, source_file: str) -> bool:
    return source_file == "craftDetails.json" or words(name_en) in ALIASES


def resolve_source_id(label: str, registry: dict) -> Optional[str]:
    """Registry id for a source label, or None. `registry` is {id: {"title": ...}}."""
    key = words(label)
    if key in SOURCE_ALIASES:
        return SOURCE_ALIASES[key]
    for source_id, entry in registry.items():
        title = words(entry.get("title", ""))
        if title and title in key:
            return source_id
    return None
