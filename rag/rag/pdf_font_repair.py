"""Font-encoding repair for PDFs with broken glyph maps.

Some PDFs (this project's `-Free-English-Grammar.pdf` among them - watermarked by
pdforall.com) embed subset fonts with *no* ToUnicode CMap. Every PDF text
extractor - pypdf and PyMuPDF alike - then returns the raw glyph indices, which
come out as `& ? ! & & ? !` garbage.

The scramble is a plain, document-wide substitution cipher, so it is fully
recoverable without OCR. Two subset fonts carry the body:

    TTE1AEB1B8t00  regular body text  (~93 % of characters)
    TTE1AEB0C8t00  bold headings / emphasised grammar terms

The maps below were reconstructed by decoding known words. Anything that is not
one of those two fonts (Times-Roman digits, the Helvetica watermark) already
extracts correctly and is passed through untouched.

`repair_page` rebuilds a page's text span by span, so page boundaries - and
therefore the page numbers cited in step 11 - stay exactly where PyMuPDF puts
them.
"""

from __future__ import annotations

import re

BODY_FONT = "TTE1AEB1B8t00"
BOLD_FONT = "TTE1AEB0C8t00"

# --- regular body font -------------------------------------------------------
BODY_MAP = {
    0x01: " ", 0x02: "b", 0x03: "y", 0x04: "S", 0x05: "e", 0x06: "c",
    0x07: "o", 0x08: "n", 0x09: "d", 0x0A: "i", 0x0B: "t", 0x0C: ".",
    0x0D: "C", 0x0E: "p", 0x0F: "r", 0x10: "g", 0x11: "h", 0x13: "2",
    0x14: "0", 0x15: "T", 0x16: "m", 0x17: "a", 0x18: "l", 0x19: "s",
    0x1A: "k", 0x1B: "u", 0x1C: "f", 0x1F: "w", 0x20: "v",
    0x21: ",", 0x22: "x", 0x23: "I", 0x24: ":", 0x26: "A", 0x27: "E",
    0x28: "F", 0x29: "G", 0x2A: "N", 0x2B: "z", 0x2C: "H", 0x2D: "U",
    0x2E: "D", 0x2F: "O", 0x30: "R", 0x31: "M", 0x32: "L", 0x33: "V",
    0x34: "B", 0x35: "W", 0x36: "P", 0x37: "1", 0x38: "3", 0x39: "4",
    0x3A: "Q", 0x3B: "q", 0x3C: "5", 0x3D: "6", 0x3E: "7", 0x3F: "j",
    0x40: "8", 0x42: "9", 0x43: "/", 0x44: "(", 0x45: ")", 0x46: "?",
    0x47: "’", 0x48: "Y", 0x49: " ", 0x4A: '"', 0x4B: '"', 0x4C: "J",
    0x4E: "!", 0x25: "•",
}

# --- bold headings / emphasis font ----------------------------------------
BOLD_MAP = {
    0x01: " ", 0x02: "s", 0x03: "e", 0x04: "y", 0x05: "f", 0x06: "i",
    0x08: "o", 0x09: "c", 0x0A: "a", 0x0B: "w", 0x0D: "s", 0x0E: "h",
    0x0F: "m", 0x11: "n", 0x13: "l", 0x15: "r", 0x1A: "a", 0x1F: "t",
    0x22: "C", 0x24: "f", 0x26: "t", 0x27: "b", 0x29: "v", 0x2C: "d",
    0x30: "u", 0x36: "p", 0x37: "’", 0x38: "W", 0x39: ",", 0x3A: "u",
    0x3B: "v", 0x3C: "j", 0x3E: "k", 0x3F: "Q", 0x41: "Q", 0x46: ")",
}

# Capital glyphs the font draws as "<capital><phantom 0x20>"; the trailing
# glyph 0x20 (otherwise 'v') must be swallowed.
_CAP_ABSORB_V = {0x29, 0x2C, 0x2D, 0x2F, 0x31}

# characters worth keeping verbatim when a glyph is not in the map
_KEEP = set(" .,;:!?()[]{}\"'‘’“”-–—/&%$#@*+=<>0123456789\n\t")

_PHANTOM_V = re.compile(r"\b([HOMGU])v(?=[a-z])")


def _decode_span(raw: str, fmap: dict, absorb_v: bool) -> str:
    out = []
    i, n = 0, len(raw)
    while i < n:
        cp = ord(raw[i])
        if cp in fmap:
            out.append(fmap[cp])
            if absorb_v and cp in _CAP_ABSORB_V and i + 1 < n and ord(raw[i + 1]) == 0x20:
                i += 1  # swallow the phantom 'v' that trails this capital
        elif cp < 0x20:
            pass  # unmapped control glyph - drop it rather than emit noise
        elif raw[i] in _KEEP or raw[i].isalnum():
            out.append(raw[i])
        i += 1
    return "".join(out)


def repair_page(page) -> str:
    """Return the readable text of one ``pymupdf`` page.

    Broken subset fonts are decoded via the substitution maps above; every other
    font is passed through as PyMuPDF extracted it.
    """
    data = page.get_text("rawdict")
    lines = []
    for block in data["blocks"]:
        for line in block.get("lines", []):
            buf = []
            for span in line["spans"]:
                raw = "".join(ch["c"] for ch in span["chars"])
                font = span["font"]
                if font == BODY_FONT:
                    buf.append(_decode_span(raw, BODY_MAP, absorb_v=True))
                elif font == BOLD_FONT:
                    buf.append(_decode_span(raw, BOLD_MAP, absorb_v=False))
                else:
                    buf.append(raw)
            lines.append("".join(buf))
    return _PHANTOM_V.sub(r"\1", "\n".join(lines))


# --- quality scoring -------------------------------------------------------
_WORD = re.compile(r"[A-Za-z][A-Za-z'’-]*")
_VOWEL = re.compile(r"[aeiouyAEIOUY]")


def text_quality(text: str) -> tuple[float, float]:
    """(alphabetic-character ratio, plausible-English-word ratio) for `text`."""
    stripped = [c for c in text if not c.isspace()]
    if not stripped:
        return 0.0, 0.0
    alpha_ratio = sum(c.isalpha() for c in stripped) / len(stripped)

    words = _WORD.findall(text)
    if not words:
        return alpha_ratio, 0.0
    good = sum(
        1 for w in words
        if 1 <= len(w) <= 20 and (len(w) <= 2 or _VOWEL.search(w))
    )
    return alpha_ratio, good / len(words)


def is_readable(text: str, min_alpha: float = 0.70, min_words: float = 0.80) -> bool:
    alpha_ratio, word_ratio = text_quality(text)
    return alpha_ratio >= min_alpha and word_ratio >= min_words
