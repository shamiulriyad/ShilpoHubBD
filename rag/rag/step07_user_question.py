"""STEP 7 - User question."""

import sys
from typing import Optional


def get_question(argv: Optional[list] = None) -> str:
    argv = argv if argv is not None else sys.argv[1:]

    question = " ".join(argv).strip() if argv else input("\nYour question: ").strip()

    if not question:
        raise ValueError("Empty question.")

    print(f"[7] Question   : {question}")
    return question
