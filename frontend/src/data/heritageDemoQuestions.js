/**
 * Before-login AI Assistant demo dataset.
 *
 * Static, offline knowledge used by `AIAssistantWidget` while a visitor is not
 * signed in — no API, RAG, embedding, Qdrant or database call is made here.
 *
 * Every entry carries the same metadata a real RAG answer will carry, so the
 * citation line under each reply keeps working unchanged once the static
 * resolver is swapped for an API call:
 *
 *   { answer, source, category, confidence }
 *
 * `source` is deliberately the dataset itself — never an invented URL or
 * external citation.
 */

/** Marks a reply that matched nothing in the dataset. */
export const HERITAGE_DEMO_FALLBACK_ID = 'fallback';

/** The only source these preview answers can honestly claim. */
export const HERITAGE_DEMO_SOURCE = 'ShilpoHub Heritage Dataset';

export const heritageDemoQuestions = [
  {
    id: 'jamdani',
    question: 'What is Jamdani?',
    // Lowercase terms; a question matches when it contains any of them.
    match: ['jamdani'],
    answer:
      'Jamdani is a hand-woven muslin textile from the Dhaka region, traditionally made on a pit loom by two weavers working side by side. The motifs are never printed or embroidered — each one is picked into the weft by hand as the cloth grows, which is why a single sari can take months. UNESCO inscribed the art of Jamdani weaving on its Representative List of Intangible Cultural Heritage in 2013.',
    source: HERITAGE_DEMO_SOURCE,
    category: 'Textile Heritage',
    confidence: 'High',
  },
  {
    id: 'shital-pati',
    question: 'How is Shital Pati made?',
    match: ['shital pati', 'shitalpati', 'sital pati', 'shitol pati'],
    answer:
      'Shital Pati is a cooling mat woven from the soft inner bark of the murta plant, most closely associated with Sylhet. Artisans harvest and boil the cane, split it into fine strips, sun-dry and soften them, then weave the strips into dense geometric patterns on the floor. The finished mat stays cool against the skin, which is where the name comes from. The craft is on UNESCO’s Representative List of Intangible Cultural Heritage.',
    source: HERITAGE_DEMO_SOURCE,
    category: 'Handicraft Heritage',
    confidence: 'High',
  },
  {
    id: 'narayanganj-crafts',
    question: 'Which crafts are associated with Narayanganj?',
    match: ['narayanganj', 'naryanganj', 'sonargaon', 'rupganj'],
    answer:
      'Narayanganj is best known for Jamdani weaving — the looms of Rupganj, Noapara and Sonargaon form the heart of the Jamdani Palli, where most authentic Jamdani saris are still made. The district also has a long hand-loom and textile tradition around Demra, and Sonargaon remains a centre for folk crafts such as nakshi kantha, pottery, brass and wood carving, kept alive through the craft village and museum there.',
    source: HERITAGE_DEMO_SOURCE,
    category: 'Regional Heritage',
    // A multi-craft regional summary is a broader claim than the two
    // single-craft answers above, so it is not stated as confidently.
    confidence: 'Medium',
  },
];

/** Suggested-question chips — kept in sync with the dataset by construction. */
export const heritageDemoSuggestions = heritageDemoQuestions.map((entry) => entry.question);

/** Shown when a typed question is outside the three demo answers. */
export const heritageDemoFallback = {
  id: HERITAGE_DEMO_FALLBACK_ID,
  answer:
    'I can only answer a few sample questions in this preview — the full ShilpoHub Heritage AI will be available once you sign in. For now, try: “What is Jamdani?”, “How is Shital Pati made?” or “Which crafts are associated with Narayanganj?”',
  source: HERITAGE_DEMO_SOURCE,
  category: 'General',
  confidence: 'Low',
};

/**
 * Pure lookup over the demo dataset. Returns `{ id, answer, source, category,
 * confidence }` — always a value, never null, so callers never have to
 * special-case the miss. On a miss `id` is `HERITAGE_DEMO_FALLBACK_ID`, which
 * is how the widget tells an explored topic from an unanswered question.
 */
export function findHeritageDemoAnswer(question) {
  const normalized = String(question ?? '').trim().toLowerCase();
  if (!normalized) return heritageDemoFallback;

  const hit = heritageDemoQuestions.find((entry) => entry.match.some((term) => normalized.includes(term)));
  if (!hit) return heritageDemoFallback;

  return {
    id: hit.id,
    answer: hit.answer,
    source: hit.source,
    category: hit.category,
    confidence: hit.confidence,
  };
}
