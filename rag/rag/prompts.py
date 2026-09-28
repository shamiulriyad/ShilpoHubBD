"""Prompt templates (prompts/*.txt) and the one place an LLM call is retried.

Templates use {name} placeholders. Rendering is a SINGLE regex pass over the template, so a
substituted value is never re-scanned: a user question containing the text "{context}" cannot
inject into the template, and the literal braces in the JSON examples inside a template
(`{"language": ...}`) are left alone because only `{word}` is a placeholder.
"""

import re
import time
from functools import lru_cache
from typing import Any

import config

_PLACEHOLDER = re.compile(r"\{(\w+)\}")
_RETRY_DELAYS = (10, 30, 60)          # seconds; Gemini's free tier rate-limits per minute


@lru_cache(maxsize=None)
def load_prompt(filename: str) -> str:
    path = config.PROMPTS_DIR / filename
    if not path.is_file():
        raise FileNotFoundError(f"Prompt template not found: {path}")
    return path.read_text(encoding="utf-8")


def render(template: str, **values: Any) -> str:
    return _PLACEHOLDER.sub(lambda m: str(values[m.group(1)]) if m.group(1) in values else m.group(0), template)


def _is_transient(exc: Exception) -> bool:
    """True for a Gemini failure that is explicitly documented as temporary -- free-tier per-minute
    rate limiting (429), the "high demand" capacity overload (503), a server-side deadline (504), and
    a client-side read/request timeout (httpcore.ReadTimeout, TimeoutError) from the fast-fail
    timeout=20 SDK setting -- so a real answer has a real chance instead of failing on the first
    attempt. The timeout exception classes are matched by type name, not message text: e.g.
    httpcore.ReadTimeout's message is "The read operation timed out", which never contains the
    literal word "readtimeout"."""
    text = str(exc).lower()
    type_name = type(exc).__name__.lower()
    return (
        "429" in text or "resource_exhausted" in text or "quota" in text or "rate limit" in text
        or "503" in text or "unavailable" in text or "overloaded" in text or "high demand" in text
        or "504" in text or "deadline_exceeded" in text
        or "readtimeout" in text or "timed out" in text
        or "timeout" in type_name
    )


def message_text(message: Any) -> str:
    """Plain text of a chat-model reply (content can be a string or a list of parts)."""
    text = getattr(message, "text", None)
    if isinstance(text, str):          # a property that is a str (newer langchain-core); it is also
        return str(text)               # callable, and calling it is deprecated - so test str first
    if callable(text):
        text = text()
        if isinstance(text, str):
            return text
    content = getattr(message, "content", message)
    if isinstance(content, list):
        return "".join(p.get("text", "") if isinstance(p, dict) else str(p) for p in content)
    return str(content)


def invoke_llm(llm, prompt: str, retries: int = len(_RETRY_DELAYS)) -> str:
    """Call the LLM, waiting and retrying when Gemini answers "rate limited" or "overloaded"."""
    for attempt in range(retries + 1):
        try:
            return message_text(llm.invoke(prompt)).strip()
        except Exception as exc:  # noqa: BLE001
            if attempt < retries and _is_transient(exc):
                delay = _RETRY_DELAYS[min(attempt, len(_RETRY_DELAYS) - 1)]
                print(f"    ... Gemini rate-limited/overloaded, retrying in {delay}s")
                time.sleep(delay)
                continue
            raise
    raise RuntimeError("unreachable")
