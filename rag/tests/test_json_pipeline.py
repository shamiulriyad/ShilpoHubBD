"""Offline tests for the ShilpoHub RAG pipeline. No embedding model, Gemini or on-disk Qdrant:
the router (step 9) runs against an in-memory Qdrant filled with the real chunks and cheap
deterministic fake embeddings, so routing, filters, fallback and expansion are tested for real.

    python -m unittest tests.test_json_pipeline -v      # from the rag/ folder
"""

import contextlib
import io
import re
import sys
import unittest
import zlib
from pathlib import Path
from types import SimpleNamespace

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import numpy as np
from langchain_core.documents import Document
from qdrant_client import QdrantClient
from qdrant_client.http.models import (
    Distance, Modifier, PointStruct, SparseVector, SparseVectorParams, VectorParams,
)

import config
from rag import prompts
from rag.step01_load_json import load_json
from rag.step02_normalize_json import _Places, normalize_json
from rag.step03_clean_data import CATEGORIES, classify_risk, clean_data, clean_value, normalize_category
from rag.step04_chunking import _pack, chunk_documents, get_token_counter
from rag.step06_vector_store import DENSE_VECTOR, SPARSE_VECTOR, _point_id
from rag.step07_user_question import fallback_analysis, normalize_analysis
from rag.step09_retrieve import Block, Hit, Retrieval, build_filter, retrieve
from rag.step10_generate import FIXED_REPLIES, NOT_AVAILABLE_TOKEN, format_context, generate_answer
from rag.step11_answer import build_response, cited_docs, strip_citations

REQUIRED_METADATA = {"doc_id", "doc_type", "aspect", "source_file", "craft_key", "name_en", "name_bn",
                     "category_norm", "districts", "divisions", "confidences", "source_ids",
                     "risk_level", "is_unesco", "is_gi"}


def quiet(fn, *args, **kwargs):
    with contextlib.redirect_stdout(io.StringIO()):
        return fn(*args, **kwargs)


# --- ingestion: steps 1-4 -----------------------------------------------------------------------------

class IngestionData(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.datasets = load_json()
        cls.docs = quiet(lambda: clean_data(normalize_json(cls.datasets)))
        cls.count = staticmethod(get_token_counter())        # a plain function on a class would become a method
        cls.chunks = quiet(chunk_documents, cls.docs, count_tokens=cls.count)
        cls.crafts = [d for d in cls.docs if d["doc_type"] == "craft"]

    def by_key(self, key, **match):
        return [d for d in self.crafts if d["craft_key"] == key and all(d[k] == v for k, v in match.items())]

    def test_all_records_and_the_four_reference_docs_are_kept(self):
        expected = sum(len(self.datasets[f]["crafts"]) for f in config.DATASET_FILES)
        self.assertEqual(len(self.crafts), expected)
        self.assertEqual({d["doc_id"] for d in self.docs if d["doc_type"] == "reference"},
                         {"REF-UNESCO", "REF-SITES", "REF-TAXONOMY", "REF-SOURCES"})

    def test_every_chunk_has_the_required_metadata(self):
        for chunk in self.chunks:
            self.assertTrue(REQUIRED_METADATA <= set(chunk.metadata), chunk.metadata.keys())
            self.assertTrue(chunk.page_content.strip())

    def test_every_chunk_is_under_200_tokens_including_its_header(self):
        over = [(c.metadata["doc_id"], self.count(c.page_content)) for c in self.chunks
                if self.count(c.page_content) > config.MAX_CHUNK_TOKENS]
        self.assertEqual(over, [])

    def test_header_format(self):
        craft = re.compile(r"^Craft: .+ \| Source: (craft|craftDetails|GEO)\.json \| Aspect: (overview|production|geography)$")
        for chunk in self.chunks:
            first = chunk.page_content.split("\n", 1)[0]
            if chunk.metadata["doc_type"] == "craft":
                self.assertRegex(first, craft)
                self.assertIn(chunk.metadata["name_en"], first)
            else:
                self.assertTrue(first.startswith("Reference: "), first)

    def test_bangla_name_is_in_the_header(self):
        jamdani = next(c for c in self.chunks if c.metadata["doc_id"] == "craftDetails.json:jamdani")
        self.assertIn("জামদানি", jamdani.page_content.split("\n")[0])

    def test_craft_key_links_the_same_craft_across_files(self):
        self.assertEqual({d["source_file"] for d in self.by_key("jamdani")}, set(config.DATASET_FILES))
        # names that differ per file fold into one key; judgement calls are documented in craft_keys.py
        self.assertEqual({d["source_file"] for d in self.by_key("rickshaw_art")}, set(config.DATASET_FILES))
        self.assertEqual({d["source_file"] for d in self.by_key("hand_fans")}, {"craft.json", "craftDetails.json", "GEO.json"})
        known = {d["craft_key"] for d in self.crafts if d["source_file"] == "craftDetails.json"}
        loose = {d["craft_key"] for d in self.crafts if d["source_file"] != "craftDetails.json"} - known
        self.assertEqual(loose, {"satrangi", "folk_instruments", "baul_instruments"})   # no clear counterpart

    def test_category_norm_uses_exactly_the_twelve_categories(self):
        self.assertEqual({d["category_norm"] for d in self.crafts}, set(CATEGORIES))

    def test_category_norm_decides_by_name_when_the_label_is_too_broad(self):
        pick = lambda key, source="craftDetails.json": self.by_key(key, source_file=source)[0]["category_norm"]
        self.assertEqual(pick("shital_pati"), "Natural Fibre Crafts")            # "A. Handicrafts"
        self.assertEqual(pick("hand_fans"), "Natural Fibre Crafts")
        self.assertEqual(pick("wooden_furniture_crafts"), "Wood Crafts")
        self.assertEqual(pick("coconut_crafts"), "Other Traditional Crafts")
        self.assertEqual(pick("nakshi_kantha"), "Textile Heritage")              # "A. / C. Folk Art / B. Textile"
        self.assertEqual(pick("traditional_boat_building"), "Fisheries")         # its own file says "I. Fisheries"
        self.assertEqual(pick("traditional_boat_building", "craft.json"), "Wood Crafts")
        self.assertEqual(pick("monipuri_weaving", "GEO.json"), "Textile Heritage")   # "Indigenous Craft"

    def test_risk_level_is_keyword_based_and_the_earliest_keyword_wins(self):
        cases = {
            "Critically endangered / revival-stage": "critically_endangered",
            "Endangered — rising metal costs": "endangered",
            "Vulnerable — small population base": "vulnerable",
            "Moderate risk from plastic/metal vessel competition": "at_risk",
            "Stable but shrinking vs power-loom competition": "at_risk",
            "Declining vs plastic toys": "at_risk",
            "Stable as a broad, evolving category rather than a single endangered craft": "stable",
            "Growing modern cottage-industry sector rather than an endangered heritage craft": "growing",
            "Relatively stable; power-loom copies undercut authentic handloom producers": "stable",
            "": "unknown", None: "unknown", "no keyword here": "unknown",
        }
        for text, level in cases.items():
            self.assertEqual(classify_risk(text), level, text)

    def test_original_endangerment_text_stays_in_the_chunk(self):
        chunk = next(c for c in self.chunks if c.metadata["doc_id"] == "craftDetails.json:rickshaw_art"
                     and c.metadata["aspect"] == "geography")
        self.assertIn("Endangered — declining rickshaw numbers", chunk.page_content)
        self.assertIn("Risk level: endangered", chunk.page_content)
        self.assertEqual(chunk.metadata["risk_level"], "endangered")

    def test_is_unesco_follows_the_unesco_list_only(self):
        unesco = {d["craft_key"] for d in self.crafts if d["is_unesco"]}
        self.assertEqual(unesco, {"jamdani", "shital_pati", "rickshaw_art", "tangail_saree"})
        for key in ("brass_bell_metal", "traditional_boat_building"):             # "pending files" is not inscribed
            self.assertFalse(any(d["is_unesco"] for d in self.by_key(key)))
        self.assertTrue(all(d["is_unesco"] for d in self.by_key("jamdani")))     # every file's record, not just one

    def test_is_gi_needs_the_letters_GI_in_notes_status_or_description(self):
        gi = {d["craft_key"] for d in self.crafts if d["is_gi"]}
        self.assertTrue({"jamdani", "rajshahi_silk", "bogurar_doi"} <= gi)
        self.assertNotIn("tangail_saree", gi)                                    # the data does not say GI

    def test_districts_come_from_structured_regions_and_from_region_prose(self):
        self.assertEqual(self.by_key("jamdani", source_file="craftDetails.json")[0]["districts"], ["Narayanganj"])  # prose
        self.assertEqual(self.by_key("jamdani", source_file="craft.json")[0]["districts"], ["Dhaka", "Narayanganj"])  # structured
        bamboo = self.by_key("bamboo_crafts", source_file="craftDetails.json")[0]
        self.assertIn("Rangamati", bamboo["districts"])
        self.assertNotIn("Chattogram", bamboo["districts"])                      # "Chattogram Hill Tracts" is a region
        self.assertIn("Chattogram", bamboo["divisions"])                         # ...whose districts belong to that division

    def test_nationwide_is_a_scope_not_a_district(self):
        # craft.json / GEO.json write district "Nationwide" + division "Bangladesh" for crafts practised everywhere
        for doc in self.crafts:
            self.assertNotIn("Nationwide", doc["districts"], doc["doc_id"])
            self.assertNotIn("Bangladesh", doc["divisions"], doc["doc_id"])
        cane = self.by_key("cane_crafts", source_file="craft.json")[0]
        text = next(c.page_content for c in self.chunks
                    if c.metadata["doc_id"] == cane["doc_id"] and c.metadata["aspect"] == "geography")
        self.assertIn("Region: Nationwide", text)
        self.assertNotIn("Nationwide district", text)
        self.assertNotIn("Bangladesh division", text)

    def test_old_district_spellings_are_normalized(self):
        places = _Places(self.datasets)
        for old, new in {"Chittagong": "Chattogram", "Comilla": "Cumilla", "Barisal": "Barishal",
                         "Jessore": "Jashore", "Bogra": "Bogura"}.items():
            self.assertEqual(places.district(old), new)
            self.assertEqual(places.find(f"made in {old} and nearby"), [new])

    def test_confidences_and_source_ids(self):
        geo = next(c for c in self.chunks if c.metadata["doc_id"] == "craft.json:BDCP-001" and c.metadata["aspect"] == "geography")
        self.assertEqual(geo.metadata["confidences"], ["high", "medium"])
        self.assertEqual(geo.metadata["source_ids"], ["SRC_UNESCO_JAMDANI"])
        tangail = self.by_key("tangail_saree", source_file="craft.json")[0]
        self.assertEqual(tangail["source_ids"], ["SRC_UNESCO_TANGAIL", "SRC_TANGAIL_GOV"])
        self.assertEqual(self.by_key("jamdani", source_file="GEO.json")[0]["source_ids"], ["SRC_UNESCO_JAMDANI"])

    def test_reference_chunks_carry_only_their_own_places(self):
        sites = [c for c in self.chunks if c.metadata["doc_id"] == "REF-SITES"]
        self.assertTrue(sites)
        self.assertIn("Narayanganj", {d for c in sites for d in c.metadata["districts"]})
        for chunk in sites:
            for district in chunk.metadata["districts"]:
                self.assertIn(district.lower(), chunk.page_content.lower())

    def test_an_aspect_that_does_not_fit_is_split_at_natural_seams_not_mid_sentence(self):
        fits = lambda text: len(text.split()) <= 12
        parts = ["How it is made:\n1. Wash the yarn in clean water.\n2. Starch and dry it in the sun.\n3. Warp the loom.",
                 "Materials: cotton yarn; dyes; starch."]
        bodies = _pack(parts, fits)
        self.assertGreater(len(bodies), 1)
        self.assertTrue(all(fits(b) for b in bodies))
        joined = "\n".join(bodies)
        for line in ("1. Wash the yarn in clean water.", "2. Starch and dry it in the sun.", "3. Warp the loom."):
            self.assertIn(line, joined)                                          # no step was cut in half

    def test_chunk_keys_are_unique_so_qdrant_ids_are_stable(self):
        keys = [c.metadata["chunk_key"] for c in self.chunks]
        self.assertEqual(len(keys), len(set(keys)))
        self.assertEqual(_point_id(self.chunks[0], "c"), _point_id(self.chunks[0], "c"))


class Cleaning(unittest.TestCase):
    def test_null_like_values_and_duplicates_are_removed(self):
        self.assertIsNone(clean_value("  N/A "))
        self.assertIsNone(clean_value([None, "", "null", []]))
        self.assertEqual(clean_value(["Cotton", "cotton ", "  jute  "]), ["Cotton", "jute"])
        self.assertEqual(clean_value([["a", "b"], "c"]), ["a", "b", "c"])
        self.assertEqual(dict(clean_value({"a": "x", "b": None, "c": {"d": ""}})), {"a": "x"})

    def test_whitespace_is_normalized_but_bangla_joiners_survive(self):
        self.assertEqual(clean_value("a \n\t  b c"), "a b c")
        self.assertEqual(clean_value("র‍য"), "র‍য")

    def test_category_letters_alone_are_never_enough(self):
        self.assertEqual(normalize_category("A. Handicrafts", "Leather Handicrafts"), "Other Traditional Crafts")
        self.assertEqual(normalize_category("A. Handicrafts", "Bamboo Crafts"), "Natural Fibre Crafts")


# --- query time: prompts, analysis, routing, answering --------------------------------------------------------

class Prompts(unittest.TestCase):
    def test_both_prompt_files_exist_with_the_placeholders_the_spec_names(self):
        answer = prompts.load_prompt("answer_prompt.txt")
        for name in ("{context}", "{question}", "{question_type}", "{language}"):
            self.assertIn(name, answer)
        analysis = prompts.load_prompt("query_analysis.txt")
        for name in ("{question}", "{craft_keys}", "{categories}", "{risk_levels}"):
            self.assertIn(name, analysis)

    def test_rendering_is_one_pass_and_leaves_json_braces_alone(self):
        out = prompts.render('{"a": 1} {question} {other}', question="what is {context}?", context="SECRET")
        self.assertEqual(out, '{"a": 1} what is {context}? {other}')             # the question cannot inject


class QueryAnalysis(unittest.TestCase):
    CATALOG = {"crafts": {"jamdani": {}, "shital_pati": {}}, "districts": ["Chattogram", "Sylhet"], "divisions": ["Dhaka", "Sylhet"]}

    def test_the_model_json_is_validated(self):
        raw = {"question_type": "location_crafts", "language": "English", "craft_keys": ["jamdani", "made_up"],
               "districts": ["Chittagong", "barguna"], "divisions": ["sylhet", "Atlantis"], "category_norm": "textile heritage",
               "risk_levels": ["endangered", "unknown", "bogus"], "is_unesco": "yes", "is_gi": True, "english_query": " crafts in Chattogram "}
        out = normalize_analysis(raw, "q", self.CATALOG)
        self.assertEqual(out["craft_keys"], ["jamdani"])                          # unknown key dropped
        self.assertEqual(out["districts"], ["Chattogram", "Barguna"])             # old spelling mapped; real district kept
        self.assertEqual(out["divisions"], ["Sylhet"])
        self.assertEqual(out["category_norm"], "Textile Heritage")
        self.assertEqual(out["risk_levels"], ["endangered"])
        self.assertIsNone(out["is_unesco"])                                       # not a real boolean
        self.assertIs(out["is_gi"], True)
        self.assertEqual(out["english_query"], "crafts in Chattogram")

    def test_a_bad_type_falls_back_and_bangla_script_forces_the_language(self):
        out = normalize_analysis({"question_type": "banana"}, "q", self.CATALOG)
        self.assertEqual((out["question_type"], out["craft_keys"]), ("describe", []))
        self.assertTrue(out["source"].startswith("fallback"))
        out = normalize_analysis({"question_type": "describe", "language": "English"}, "জামদানি কী?", self.CATALOG)
        self.assertEqual(out["language"], "Bangla")
        self.assertEqual(fallback_analysis("hello", "x")["english_query"], "hello")


class FakeDense:
    """Hashed bag-of-words vectors: enough similarity structure to exercise the router."""
    def _vec(self, text):
        v = np.zeros(64)
        for token in re.findall(r"[a-z0-9]+", text.lower()):
            v[zlib.crc32(token.encode()) % 64] += 1
        return (v / (np.linalg.norm(v) or 1)).tolist()

    def embed_documents(self, texts):
        return [self._vec(t) for t in texts]

    def embed_query(self, text):
        return self._vec(text)


def _sparse(text):
    counts = {}
    for token in re.findall(r"[a-z0-9]+", text.lower()):
        index = zlib.crc32(token.encode()) % 5000
        counts[index] = counts.get(index, 0) + 1
    return SparseVector(indices=list(counts), values=[float(v) for v in counts.values()])


class Router(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        docs = quiet(lambda: clean_data(normalize_json(load_json())))
        cls.chunks = quiet(chunk_documents, docs, count_tokens=get_token_counter())
        cls.client = QdrantClient(":memory:")
        cls.client.create_collection("t", vectors_config={DENSE_VECTOR: VectorParams(size=64, distance=Distance.COSINE)},
                                     sparse_vectors_config={SPARSE_VECTOR: SparseVectorParams(modifier=Modifier.IDF)})
        dense = FakeDense()
        cls.client.upsert("t", points=[
            PointStruct(id=_point_id(c, "t"), vector={DENSE_VECTOR: dense._vec(c.page_content), SPARSE_VECTOR: _sparse(c.page_content)},
                        payload={"page_content": c.page_content, "metadata": c.metadata}) for c in cls.chunks])
        cls.dense = dense

    def run_route(self, qtype, english_query="query", min_score=None, **analysis):
        base = {"question": english_query, "language": "English", "question_type": qtype, "craft_keys": [], "districts": [],
                "divisions": [], "category_norm": None, "risk_levels": [], "is_unesco": None, "is_gi": None,
                "english_query": english_query}
        base.update(analysis)
        return quiet(retrieve, self.client, "t", base, self.dense.embed_query(english_query), _sparse(english_query), min_score)

    def test_describe_filters_the_craft_and_adds_siblings_from_the_other_files(self):
        r = self.run_route("describe", "what is jamdani", craft_keys=["jamdani"])
        self.assertEqual(r.craft_keys, ["jamdani"])
        self.assertEqual({h.metadata["aspect"] for h in r.hits}, {"overview", "geography"})
        self.assertEqual({h.metadata["source_file"] for h in r.hits}, set(config.DATASET_FILES))
        self.assertLessEqual(len(r.hits), 10)
        self.assertEqual(len({h.metadata["chunk_key"] for h in r.hits}), len(r.hits))    # no duplicates

    def test_how_made_and_materials_tools_search_production_only(self):
        for qtype in ("how_made", "materials_tools"):
            r = self.run_route(qtype, "how is shital pati made", craft_keys=["shital_pati"])
            self.assertEqual({h.metadata["aspect"] for h in r.hits}, {"production"})
            self.assertEqual(r.craft_keys, ["shital_pati"])

    def test_craft_location_searches_geography_and_expands(self):
        r = self.run_route("craft_location", "where is jamdani made", craft_keys=["jamdani"])
        self.assertEqual({h.metadata["aspect"] for h in r.hits}, {"geography"})
        self.assertEqual({h.metadata["source_file"] for h in r.hits}, set(config.DATASET_FILES))

    def test_location_crafts_filters_the_district_and_groups_by_craft(self):
        r = self.run_route("location_crafts", "crafts in sylhet", districts=["Sylhet"])
        self.assertTrue(all("Sylhet" in h.metadata["districts"] and h.metadata["aspect"] == "geography" for h in r.hits))
        self.assertTrue({"shital_pati", "monipuri_weaving", "tea_cultivation"} <= set(r.craft_keys))
        self.assertEqual(len(r.blocks), len(r.craft_keys))                               # one block per craft
        self.assertTrue(all(len({h.metadata["craft_key"] for h in b.hits}) == 1 for b in r.blocks))
        by_division = self.run_route("location_crafts", "crafts in sylhet division", divisions=["Sylhet"])
        self.assertIn("shital_pati", by_division.craft_keys)

    def test_a_district_with_no_data_falls_back_once_and_the_floor_then_applies(self):
        r = self.run_route("location_crafts", "crafts in barguna", districts=["Barguna"], min_score=-1)
        self.assertTrue(r.fallback)
        self.assertTrue(r.hits)                                                          # unfiltered results, nothing dropped
        strict = self.run_route("location_crafts", "crafts in barguna", districts=["Barguna"], min_score=1.1)
        self.assertTrue(strict.fallback)
        self.assertEqual(strict.hits, [])                                                # nothing relevant enough -> not found

    def test_a_filter_that_only_picks_an_aspect_is_not_evidence_of_relevance(self):
        strict = self.run_route("location_crafts", "crafts somewhere", min_score=1.1)    # no district: aspect filter only
        self.assertEqual(strict.hits, [])

    def test_status_endangered_returns_every_endangered_craft_from_the_risk_filter(self):
        r = self.run_route("status", "endangered crafts", risk_levels=["critically_endangered", "endangered"])
        self.assertEqual(set(r.craft_keys), {"rickshaw_art", "copper_products", "shell_crafts", "brass_bell_metal",
                                             "muslin_revival", "potchitra_scroll_painting", "tabla_making"})
        self.assertTrue(all(h.metadata["risk_level"] in ("critically_endangered", "endangered") for h in r.hits))

    def test_status_unesco_filters_the_flag_and_includes_ref_unesco(self):
        r = self.run_route("status", "unesco inscribed crafts", is_unesco=True)
        self.assertEqual(set(r.craft_keys) - {None}, {"jamdani", "shital_pati", "rickshaw_art", "tangail_saree"})
        self.assertEqual(r.blocks[0].hits[0].metadata["doc_id"], "REF-UNESCO")

    def test_status_gi_filters_the_flag(self):
        r = self.run_route("status", "gi crafts", is_gi=True)
        self.assertTrue(r.hits and all(h.metadata["is_gi"] for h in r.hits))
        self.assertIn("jamdani", r.craft_keys)

    def test_list_filters_the_category_and_includes_the_taxonomy(self):
        r = self.run_route("list", "musical instrument crafts", category_norm="Musical Crafts")
        self.assertEqual(r.blocks[0].hits[0].metadata["doc_id"], "REF-TAXONOMY")
        crafts = [h for b in r.blocks[1:] for h in b.hits]
        self.assertTrue(all(h.metadata["category_norm"] == "Musical Crafts" for h in crafts))
        self.assertTrue({"dotara_making", "tabla_making", "harmonium_craft"} <= set(r.craft_keys))

    def test_compare_retrieves_each_craft_separately_under_its_own_heading(self):
        r = self.run_route("compare", "jamdani tangail saree", craft_keys=["jamdani", "tangail_saree"])
        self.assertEqual([b.heading for b in r.blocks], ["Jamdani", "Tangail Saree"])
        for block, key in zip(r.blocks, ("jamdani", "tangail_saree")):
            self.assertTrue(all(h.metadata["craft_key"] == key for h in block.hits))
            self.assertLessEqual(len(block.hits), 10)

    def test_time_always_includes_time_required_from_both_craft_json_and_craft_details(self):
        r = self.run_route("time", "how long to make jamdani", craft_keys=["jamdani"])
        timed = {h.metadata["source_file"] for h in r.hits if "Time required" in h.text}
        self.assertTrue({"craft.json", "craftDetails.json"} <= timed)
        self.assertEqual({h.metadata["aspect"] for h in r.hits}, {"production"})

    def test_heritage_includes_ref_sites(self):
        r = self.run_route("heritage", "heritage sites terracotta", craft_keys=["terracotta"])
        self.assertEqual(r.blocks[0].hits[0].metadata["doc_id"], "REF-SITES")
        self.assertIn("terracotta", r.craft_keys)

    def test_out_of_scope_skips_retrieval_entirely(self):
        r = self.run_route("out_of_scope", "who won the world cup")
        self.assertTrue(r.skipped)
        self.assertEqual(r.hits, [])

    def test_build_filter_targets_the_metadata_payload_keys(self):
        self.assertIsNone(build_filter())
        keys = [c.key for c in build_filter(craft_key=["a", "b"], is_unesco=True, aspect=["overview"]).must]
        self.assertEqual(keys, ["metadata.craft_key", "metadata.is_unesco", "metadata.aspect"])


def _hit(name="Jamdani", source="craft.json", doc="craft.json:BDCP-001", aspect="geography", ids=None, key="jamdani"):
    return Hit(text=f"Craft: {name} | Source: {source} | Aspect: {aspect}\nbody",
               metadata={"name_en": name, "source_file": source, "doc_id": doc, "aspect": aspect, "doc_type": "craft",
                         "craft_key": key, "source_ids": ids or [], "chunk_key": f"{doc}#{aspect}#0"}, score=0.5)


class FakeLLM:
    def __init__(self, reply):
        self.reply, self.prompt = reply, None

    def invoke(self, prompt):
        self.prompt = prompt
        return SimpleNamespace(text=self.reply)


class Answering(unittest.TestCase):
    ANALYSIS = {"question": "Where is Jamdani made?", "language": "English", "question_type": "craft_location"}

    def retrieval(self, *hits):
        return Retrieval("craft_location", blocks=[Block(None, list(hits))])

    def test_context_uses_the_spec_block_format(self):
        block = format_context(self.retrieval(_hit(), _hit("Tangail Saree", "GEO.json", "GEO.json:BDV2-002", "overview", key="tangail_saree")))
        self.assertIn("[Doc 1 | source_file: craft.json | doc_id: craft.json:BDCP-001 | craft: Jamdani | aspect: geography]", block)
        self.assertIn("[Doc 2 | source_file: GEO.json | doc_id: GEO.json:BDV2-002 | craft: Tangail Saree | aspect: overview]", block)

    def test_the_prompt_receives_context_question_type_and_language(self):
        llm = FakeLLM("It is made in Narayanganj [Doc 1].")
        answer, refused = generate_answer(self.ANALYSIS, self.retrieval(_hit()), llm=llm)
        self.assertFalse(refused)
        self.assertIn("[Doc 1 | source_file: craft.json", llm.prompt)
        self.assertIn("Where is Jamdani made?", llm.prompt)
        self.assertIn("Question type: craft_location", llm.prompt)
        self.assertIn("Answer language: English", llm.prompt)

    def test_nothing_retrieved_or_out_of_scope_never_calls_the_llm(self):
        class Boom:
            def invoke(self, _):
                raise AssertionError("the LLM must not be called")
        empty = Retrieval("craft_location")
        self.assertEqual(generate_answer(self.ANALYSIS, empty, llm=Boom()), (FIXED_REPLIES["not_available"]["English"], True))
        skipped = Retrieval("out_of_scope", skipped=True)
        self.assertEqual(generate_answer({**self.ANALYSIS, "language": "Bangla"}, skipped, llm=Boom()),
                         (FIXED_REPLIES["out_of_scope"]["Bangla"], True))

    def test_the_not_available_token_becomes_the_fixed_reply_in_the_questions_language(self):
        for language in ("English", "Bangla"):
            for reply in (NOT_AVAILABLE_TOKEN, f" `{NOT_AVAILABLE_TOKEN}`. "):
                answer, refused = generate_answer({**self.ANALYSIS, "language": language}, self.retrieval(_hit()), llm=FakeLLM(reply))
                self.assertEqual((answer, refused), (FIXED_REPLIES["not_available"][language], True))

    def test_every_citation_style_is_parsed_and_stripped(self):
        text = "Woven by hand [Doc 1, Doc 2]. Made in Narayanganj [Doc 3] and Dhaka [Doc 1][Doc 2] [Doc 3, 4]."
        self.assertEqual(cited_docs(text), {1, 2, 3, 4})
        self.assertEqual(strip_citations(text), "Woven by hand. Made in Narayanganj and Dhaka.")
        self.assertEqual(strip_citations("One [Doc 1] two"), "One two")                 # the space after a marker survives

    def test_response_has_exactly_the_spec_shape(self):
        hits = [_hit(ids=["SRC_A"]), _hit(aspect="overview", ids=["SRC_A", "SRC_B"]),
                _hit("Tangail Saree", "GEO.json", "GEO.json:BDV2-002", ids=["SRC_C"], key="tangail_saree")]
        response = build_response("Made in Narayanganj [Doc 1][Doc 2].", False, self.ANALYSIS, self.retrieval(*hits))
        self.assertEqual(set(response), {"answer", "question_type", "sources"})
        self.assertEqual(response["answer"], "Made in Narayanganj.")
        self.assertEqual(response["question_type"], "craft_location")
        self.assertEqual(response["sources"], [{"source_file": "craft.json", "doc_id": "craft.json:BDCP-001",
                                                "source_ids": ["SRC_A", "SRC_B"]}])       # Doc 3 not cited; Doc 1+2 collapse

    def test_uncited_answers_list_every_retrieved_document_and_refusals_list_none(self):
        hits = [_hit(), _hit("Tangail Saree", "GEO.json", "GEO.json:BDV2-002", key="tangail_saree")]
        self.assertEqual(len(build_response("No markers.", False, self.ANALYSIS, self.retrieval(*hits))["sources"]), 2)
        self.assertEqual(build_response("not here", True, self.ANALYSIS, self.retrieval(*hits))["sources"], [])


if __name__ == "__main__":
    unittest.main()
