"""STEP 4 - Aspect chunking (no generic text splitter).

Each craft record becomes up to three chunks, one per aspect (built in step 3):

    overview    category, description, heritage status, notes, skill transmission, sources
    production  steps, materials, tools, techniques, products, time required
    geography   regions / associations with confidence, primary hub, endangerment

and each reference document (REF-UNESCO, REF-SITES, REF-TAXONOMY, REF-SOURCES) becomes
chunks of its own lines. Every chunk must stay under MAX_CHUNK_TOKENS (200) - the embedding
model reads at most 256 tokens and silently ignores the rest - so an aspect that does not
fit is packed into several chunks of the same aspect, splitting only at natural seams:
between parts, then between lines, then between sentences, and between words only as a
last resort.

Every chunk starts with a header, so it still makes sense on its own once embedded:

    Craft: Jamdani (জামদানি) | Source: craft.json | Aspect: production
    Reference: UNESCO Intangible Cultural Heritage of Bangladesh | Source: GEO.json | Aspect: reference

Tokens are counted with the embedding model's own tokenizer (header included), so "under 200"
is measured, not estimated. With a non-local embedding provider there is no local tokenizer
and a conservative character estimate is used instead.
"""

import math
import re
from typing import Any, Callable, Dict, List

from langchain_core.documents import Document

import config

_SENTENCE_END = re.compile(r"(?<=[.;!?।])\s+")


def get_token_counter() -> Callable[[str], int]:
    """Token counter for the configured embedding model."""
    if config.EMBEDDING_PROVIDER == "huggingface":
        try:
            from transformers import AutoTokenizer

            tokenizer = AutoTokenizer.from_pretrained(config.EMBEDDING_MODEL)
            print(f"    tokens     : counted with the {config.EMBEDDING_MODEL} tokenizer")
            return lambda text: len(tokenizer.encode(text, add_special_tokens=False))
        except Exception as exc:  # noqa: BLE001 - fall back rather than fail ingestion
            print(f"    ! tokenizer unavailable ({exc}); using a character estimate")
    return lambda text: math.ceil(len(text) / 3)      # deliberately pessimistic


def _split_part(part: str, fits: Callable[[str], bool]) -> List[str]:
    """Break one over-long part at its natural seams: lines, then sentences, then words."""
    pieces = [part]
    for split in (lambda t: t.split("\n"), lambda t: _SENTENCE_END.split(t)):
        pieces = [p for piece in pieces for p in ([piece] if fits(piece) else split(piece)) if p.strip()]
        if all(fits(p) for p in pieces):
            return pieces

    out: List[str] = []                               # last resort: fill slices word by word
    for piece in pieces:
        if fits(piece):
            out.append(piece)
            continue
        current: List[str] = []
        for word in piece.split():
            if current and not fits(" ".join([*current, word])):
                out.append(" ".join(current))
                current = []
            current.append(word)
        if current:
            out.append(" ".join(current))
    return out


def _pack(parts: List[str], fits: Callable[[str], bool]) -> List[str]:
    """Greedily join parts, in order, into bodies that each satisfy `fits`."""
    bodies: List[str] = []
    current: List[str] = []
    for part in parts:
        for piece in ([part] if fits(part) else _split_part(part, fits)):
            if current and not fits("\n".join([*current, piece])):
                bodies.append("\n".join(current))
                current = []
            current.append(piece)
    if current:
        bodies.append("\n".join(current))
    return bodies


_DOC_TYPE_LABELS = {"craft": "Craft", "reference": "Reference"}


def _header(doc: Dict[str, Any], aspect: str) -> str:
    label = _DOC_TYPE_LABELS.get(doc["doc_type"], doc["doc_type"].replace("_", " ").title())
    name = doc["name_en"] + (f" ({doc['name_bn']})" if doc.get("name_bn") else "")
    return f"{label}: {name} | Source: {doc['source_file']} | Aspect: {aspect}"


def _mentioned(values: List[str], text: str) -> List[str]:
    """The values that actually appear in this chunk's text (a reference document covers
    many places; each of its chunks should carry only its own)."""
    lowered = text.lower()
    return [v for v in values if v.lower() in lowered]


def chunk_documents(docs: List[Dict[str, Any]], max_tokens: int = None, count_tokens=None) -> List[Document]:
    max_tokens = max_tokens or config.MAX_CHUNK_TOKENS
    count_tokens = count_tokens or get_token_counter()

    chunks: List[Document] = []
    split_aspects = 0

    for doc in docs:
        is_ref = doc["doc_type"] == "reference"
        for aspect, parts in doc["aspects"].items():
            header = _header(doc, aspect)
            bodies = _pack(parts, lambda body: count_tokens(f"{header}\n{body}") <= max_tokens)
            split_aspects += len(bodies) > 1

            for n, body in enumerate(bodies):
                text = f"{header}\n{body}"
                districts, divisions, source_ids = doc["districts"], doc["divisions"], doc["source_ids"]
                if is_ref:
                    districts = _mentioned(districts, text)
                    divisions = _mentioned(divisions, text)
                    source_ids = _mentioned(source_ids, text)

                chunks.append(Document(page_content=text, metadata={
                    "doc_id": doc["doc_id"],
                    "doc_type": doc["doc_type"],
                    "aspect": aspect,
                    "source_file": doc["source_file"],
                    "craft_key": doc["craft_key"],
                    "name_en": doc["name_en"],
                    "name_bn": doc["name_bn"],
                    "category_norm": doc["category_norm"],
                    "category_raw": doc["category_raw"],
                    "districts": districts,
                    "divisions": divisions,
                    "confidences": doc["confidences"],
                    "source_ids": source_ids,
                    "risk_level": doc["risk_level"],
                    "is_unesco": doc["is_unesco"],
                    "is_gi": doc["is_gi"],
                    # Optional, non-craft metadata (e.g. Travel Planner places): absent/None on
                    # every existing craft/reference document, so this is a no-op for them.
                    "area": doc.get("area"),
                    "themes": doc.get("themes") or [],
                    "chunk_key": f"{doc['doc_id']}#{aspect}#{n}",   # stable identity -> stable Qdrant id
                    "chunk_index": n,
                }))

    for i, chunk in enumerate(chunks):
        chunk.metadata["chunk_id"] = i

    sizes = [count_tokens(c.page_content) for c in chunks] or [0]
    by_aspect: Dict[str, int] = {}
    for c in chunks:
        by_aspect[c.metadata["aspect"]] = by_aspect.get(c.metadata["aspect"], 0) + 1
    print(f"[4] Chunked    : {len(chunks)} chunks from {len(docs)} documents "
          f"({', '.join(f'{k}: {v}' for k, v in by_aspect.items())})")
    print(f"    tokens/chunk: avg={sum(sizes) / len(sizes):.0f}, max={max(sizes)} (limit {max_tokens}); "
          f"{sum(1 for s in sizes if s > max_tokens)} over limit; {split_aspects} aspect(s) needed more than one chunk")
    return chunks
