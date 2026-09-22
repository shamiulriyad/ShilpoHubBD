"""STEP 10 - Gemini (Step D: answer from the retrieved context).

The prompt (prompts/answer_prompt.txt) receives {context}, {question}, {question_type} and
{language}. The context is one block per retrieved chunk, in the spec's format:

    [Doc 3 | source_file: craftDetails.json | doc_id: craftDetails.json:jamdani | craft: Jamdani | aspect: geography]
    <chunk text>

(with a "=== Craft name ===" heading line above each craft's blocks for grouped and compare
questions).

Cases that never reach the model, or whose reply is replaced:
  * out_of_scope question  -> fixed out-of-scope reply, no retrieval, no generation
  * nothing retrieved      -> fixed "not available" reply, no generation (nothing to ground it in)
  * the model answers NOT_AVAILABLE -> the same fixed "not available" reply
The fixed replies exist in English and Bangla and follow the question's language.
"""

import re
from typing import Any, Dict, Tuple

from langchain_google_genai import ChatGoogleGenerativeAI

import config
from rag import prompts
from rag.step09_retrieve import Retrieval

NOT_AVAILABLE_TOKEN = "NOT_AVAILABLE"

FIXED_REPLIES = {
    "not_available": {
        "English": "This information isn't in the ShilpoHub dataset yet.",
        "Bangla": "এই তথ্যটি এখনো ShilpoHub ডেটাসেটে নেই।",
    },
    "out_of_scope": {
        "English": "I can only answer questions about Bangladeshi traditional crafts and heritage from the "
                   "ShilpoHub knowledge base. Try asking about a craft, a district or a heritage site.",
        "Bangla": "আমি শুধু ShilpoHub-এর জ্ঞানভাণ্ডারে থাকা বাংলাদেশের ঐতিহ্যবাহী কারুশিল্প ও ঐতিহ্য বিষয়ক প্রশ্নের "
                  "উত্তর দিতে পারি। কোনো কারুশিল্প, জেলা বা ঐতিহ্যবাহী স্থান সম্পর্কে জিজ্ঞাসা করুন।",
    },
}
_TOKEN_ONLY = re.compile(r"^\W*NOT_AVAILABLE\W*$", re.I)


def get_llm() -> ChatGoogleGenerativeAI:
    config.require_api_key()
    return ChatGoogleGenerativeAI(
        model=config.LLM_MODEL,
        google_api_key=config.GOOGLE_API_KEY,
        temperature=config.TEMPERATURE,
    )


def format_context(retrieval: Retrieval) -> str:
    """The context text. Doc numbers follow retrieval.hits, so [Doc N] maps back to hit N."""
    parts, number = [], 0
    for block in retrieval.blocks:
        if block.heading:
            parts.append(f"=== {block.heading} ===")
        for hit in block.hits:
            number += 1
            meta = hit.metadata
            craft = meta["name_en"] if meta["doc_type"] == "craft" else "n/a (reference)"
            parts.append(f"[Doc {number} | source_file: {meta['source_file']} | doc_id: {meta['doc_id']} "
                         f"| craft: {craft} | aspect: {meta['aspect']}]\n{hit.text}")
    return "\n\n".join(parts)


def fixed_reply(kind: str, language: str) -> str:
    return FIXED_REPLIES[kind].get(language, FIXED_REPLIES[kind]["English"])


def generate_answer(analysis: Dict[str, Any], retrieval: Retrieval, llm=None) -> Tuple[str, bool]:
    """(answer text, refused). `refused` is True for every reply that is not a grounded answer,
    so step 11 knows to cite no sources for it."""
    language = analysis["language"]
    if retrieval.skipped:
        return fixed_reply("out_of_scope", language), True
    if not retrieval.hits:
        return fixed_reply("not_available", language), True

    prompt = prompts.render(
        prompts.load_prompt("answer_prompt.txt"),
        context=format_context(retrieval),
        question=analysis["question"],
        question_type=analysis["question_type"],
        language=language,
    )
    try:
        answer = prompts.invoke_llm(llm or get_llm(), prompt)
    except Exception as exc:  # noqa: BLE001 - re-raised with context
        raise RuntimeError(
            f"Gemini call failed (model '{config.LLM_MODEL}'). Check GOOGLE_API_KEY, its quota, "
            f"and that LLM_MODEL is a valid model name.\nOriginal error: {exc}"
        ) from exc

    print(f"[10] Gemini    : {config.LLM_MODEL} answered")
    if not answer or _TOKEN_ONLY.match(answer):
        return fixed_reply("not_available", language), True
    return answer, False
