"""HTTP client for the backend's internal product-index feed (no database access, only the shared key)."""

from typing import Any, Dict, List, Optional

import httpx

from products import settings


class ProductIndexApi:
    def __init__(self, base_url: str = settings.API_URL, api_key: Optional[str] = settings.API_KEY, timeout: float = 60.0):
        if not api_key:
            raise RuntimeError("ProductIndex__ApiKey is not set in the root .env; the backend feed needs it.")
        self._http = httpx.Client(base_url=f"{base_url}/internal/product-index", headers={"X-Internal-Key": api_key}, timeout=timeout)

    def pending(self, limit: int) -> Dict[str, Any]:
        response = self._http.get("/pending", params={"limit": limit})
        response.raise_for_status()
        return response.json()

    def ack(self, items: List[Dict[str, Any]]) -> None:
        if items:
            self._http.post("/ack", json={"items": items}).raise_for_status()

    def stats(self) -> Dict[str, int]:
        response = self._http.get("/stats")
        response.raise_for_status()
        return response.json()

    def requeue_all(self) -> int:
        response = self._http.post("/requeue-all")
        response.raise_for_status()
        return int(response.json().get("requeued", 0))

    def submit_suggestion(self, product_id: str, model: str, suggested: Dict[str, Any]) -> Dict[str, Any]:
        """AI proposes attributes. They stay PENDING in the backend until the producer confirms them."""
        response = self._http.post("/suggestions", json={"productId": product_id, "model": model, "suggested": suggested})
        response.raise_for_status()
        return response.json()

    def close(self) -> None:
        self._http.close()
