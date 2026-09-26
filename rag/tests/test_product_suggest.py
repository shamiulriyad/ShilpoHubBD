"""Offline tests for AI attribute suggestions: validation against the vocabulary, no Gemini."""

import unittest

from products.suggest import normalize, suggest
from products.vocab import Vocabulary

VOCAB = Vocabulary(
    categories=[],
    product_types=[{"slug": "saree", "name": "Saree", "name_bn": "শাড়ি"}, {"slug": "mat", "name": "Mat", "name_bn": ""}],
    materials=[{"slug": "cotton", "name": "Cotton", "name_bn": ""}, {"slug": "jute", "name": "Jute", "name_bn": ""}],
    districts=[],
)


class FakeLlm:
    def __init__(self, reply):
        self.reply = reply
        self.prompt = None

    def invoke(self, prompt):
        self.prompt = prompt
        return self.reply


class SuggestTests(unittest.TestCase):
    def test_invented_values_are_dropped(self):
        out = normalize({"productTypeSlug": "spaceship", "materialSlugs": ["cotton", "unobtainium"], "productionMethod": "machine made",
                         "occasions": ["Wedding", "moon landing"], "tags": ["Jamdani", "jamdani", " "], "notes": "x" * 500}, VOCAB)
        self.assertIsNone(out["productTypeSlug"])
        self.assertEqual(out["materialSlugs"], ["cotton"])
        self.assertIsNone(out["productionMethod"])
        self.assertEqual(out["occasions"], ["wedding"])
        self.assertEqual(out["tags"], ["jamdani"])
        self.assertEqual(len(out["notes"]), 200)

    def test_never_returns_price_size_or_weight(self):
        out = normalize({"price": 9000, "dimensions": "6 m", "weightGrams": 400, "lengthCm": 600}, VOCAB)
        self.assertFalse({"price", "dimensions", "weightGrams", "lengthCm", "dimensionsText"} & set(out))

    def test_valid_suggestion_round_trip_and_fenced_json(self):
        reply = '```json\n{"productTypeSlug":"saree","materialSlugs":["cotton"],"tags":["Jamdani"],"keywords":["জামদানি","shari"],"occasions":["wedding"],"colors":["Indigo"],"productionMethod":"handloom","notes":"From the description."}\n```'
        llm = FakeLlm(reply)
        out = suggest({"name": "Jamdani Saree", "description": "Wedding saree in cotton", "category": "Dhakai Jamdani"}, VOCAB, llm)
        self.assertEqual(out["productTypeSlug"], "saree")
        self.assertEqual(out["productionMethod"], "Handloom")
        self.assertEqual(out["colors"], ["indigo"])
        self.assertEqual(out["keywords"], ["জামদানি", "shari"])
        self.assertIn("Wedding saree in cotton", llm.prompt)
        self.assertIn("saree: Saree", llm.prompt)

    def test_model_reply_without_json_is_an_error(self):
        with self.assertRaises(ValueError):
            suggest({"name": "x"}, VOCAB, FakeLlm("I cannot help with that"))


if __name__ == "__main__":
    unittest.main()
