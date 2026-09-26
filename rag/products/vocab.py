"""The vocabulary a product query may refer to (crafts, product types, materials, districts), read from the backend's
PUBLIC endpoints. Nothing is hard-coded here: when an admin adds a product type or a category, search understands
it after the cache expires. No database access, no credentials."""

import time
from dataclasses import dataclass, field
from typing import Dict, List, Optional

import httpx

from products import settings

TTL_SECONDS = 300


@dataclass
class Vocabulary:
    categories: List[Dict[str, str]] = field(default_factory=list)      # {slug, name}
    product_types: List[Dict[str, str]] = field(default_factory=list)   # {slug, name, name_bn}
    materials: List[Dict[str, str]] = field(default_factory=list)       # {slug, name, name_bn}
    districts: List[Dict[str, str]] = field(default_factory=list)       # {name, division}
    fetched_at: float = 0.0

    @property
    def divisions(self) -> List[str]:
        return sorted({d["division"] for d in self.districts if d.get("division")})

    def category_slugs(self) -> set:
        return {c["slug"] for c in self.categories}

    def type_slugs(self) -> set:
        return {t["slug"] for t in self.product_types}

    def material_slugs(self) -> set:
        return {m["slug"] for m in self.materials}

    def canonical_district(self, name: Optional[str]) -> Optional[str]:
        return next((d["name"] for d in self.districts if name and d["name"].lower() == name.strip().lower()), None)

    def canonical_division(self, name: Optional[str]) -> Optional[str]:
        return next((d for d in self.divisions if name and d.lower() == name.strip().lower()), None)


_CACHE: Optional[Vocabulary] = None


def _get(http: httpx.Client, path: str):
    response = http.get(path)
    response.raise_for_status()
    data = response.json()
    return data if isinstance(data, list) else data.get("items", [])


def load_vocabulary(force: bool = False, base_url: str = settings.API_URL) -> Vocabulary:
    """Cached for TTL_SECONDS. If the backend is briefly unreachable the last good vocabulary is kept."""
    global _CACHE
    if _CACHE and not force and time.time() - _CACHE.fetched_at < TTL_SECONDS:
        return _CACHE
    try:
        with httpx.Client(base_url=base_url, timeout=15.0) as http:
            vocab = Vocabulary(
                categories=[{"slug": c["slug"], "name": c["name"]} for c in _get(http, "/categories")],
                product_types=[{"slug": t["slug"], "name": t["name"], "name_bn": t.get("nameBn") or ""} for t in _get(http, "/product-types")],
                materials=[{"slug": m["slug"], "name": m["name"], "name_bn": m.get("nameBn") or ""} for m in _get(http, "/materials")],
                districts=[{"name": d["name"], "division": d.get("division") or ""} for d in _get(http, "/districts")],
                fetched_at=time.time(),
            )
        _CACHE = vocab
    except Exception as exc:  # noqa: BLE001
        if _CACHE is None:
            raise RuntimeError(f"Could not load the product vocabulary from {base_url}: {exc}") from exc
        print(f"[vocab] backend unreachable ({exc}); using the cached vocabulary")
    return _CACHE
