import { useEffect, useState } from 'react';
import { ProductCard } from '../cards';
import { Pagination } from '../ui';
import { routePaths } from '../../routes/routePaths';
import { useProductSearch } from '../../hooks/useProductSearch';
import { getApiErrorMessage } from '../../utils/apiError';

const PAGE_SIZE = 12;
const EXAMPLES = [
  'Jamdani khuje dao',
  '10,000 takar moddhe ekta bhalo Jamdani dao',
  'Wedding er jonno traditional saree khujchi',
  'Highest rated Jamdani gula dekhao',
  'Dhakar moddhe available craft products ki ache?',
];
const SORT_LABELS = { rating: 'Highest rated first', price_asc: 'Lowest price first', price_desc: 'Highest price first', newest: 'Newest first', popular: 'Most popular' };
const MODE_NOTES = {
  semantic: 'Matched by meaning, then checked against live prices and stock.',
  filter: 'Filtered exactly by what you asked for.',
  fallback: 'The AI helper is unavailable right now, so these are plain keyword matches.',
};
const money = (value) => `৳${Number(value).toLocaleString('en-BD')}`;
const titleCase = (slug) => slug.replace(/[-_]/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase());

// "How I understood you" chips, so the shopper can see (and correct) what the AI inferred.
function interpretationChips(interpretation, categories) {
  if (!interpretation) return [];
  const chips = [];
  const category = categories.find((c) => c.slug === interpretation.categorySlug);
  if (interpretation.categorySlug) chips.push(`Craft: ${category?.name || titleCase(interpretation.categorySlug)}`);
  if (interpretation.productType) chips.push(`Type: ${titleCase(interpretation.productType)}`);
  interpretation.materials?.forEach((m) => chips.push(`Material: ${titleCase(m)}`));
  if (interpretation.minPrice != null && interpretation.maxPrice != null) chips.push(`${money(interpretation.minPrice)} – ${money(interpretation.maxPrice)}`);
  else if (interpretation.maxPrice != null) chips.push(`Under ${money(interpretation.maxPrice)}`);
  else if (interpretation.minPrice != null) chips.push(`Above ${money(interpretation.minPrice)}`);
  if (interpretation.minRating != null) chips.push(`Rated ${interpretation.minRating}+`);
  if (interpretation.district) chips.push(`District: ${interpretation.district}`);
  else if (interpretation.division) chips.push(`${interpretation.division} division`);
  if (interpretation.inStockOnly) chips.push('In stock only');
  interpretation.occasions?.forEach((o) => chips.push(`For ${o}`));
  interpretation.colors?.forEach((c) => chips.push(`Colour: ${c}`));
  if (interpretation.sort && interpretation.sort !== 'relevance') chips.push(SORT_LABELS[interpretation.sort] || interpretation.sort);
  return chips;
}

const toCard = (item) => ({
  id: item.id,
  name: item.name,
  price: item.effectivePrice,
  category: item.categoryName,
  producer: item.producerName,
  district: item.districtName,
  image: item.primaryImageUrl,
});

/**
 * "Ask AI" product search. The query lives in the URL (`?ai=`), so results can be shared and the back button works.
 * `onActiveChange` tells the page whether to hide the classic listing while AI results are shown.
 */
export default function AiProductSearch({ params, setParams, categories = [], productPath = routePaths.marketplaceProductDetails }) {
  const active = params.get('ai') || '';
  const page = Math.max(1, Number.parseInt(params.get('aiPage'), 10) || 1);
  const [text, setText] = useState(active);
  useEffect(() => setText(active), [active]);
  const query = useProductSearch(active, page, PAGE_SIZE);

  const submit = (value) => {
    const q = (value ?? text).trim();
    if (q.length < 2) return;
    const next = new URLSearchParams();
    next.set('ai', q);
    setParams(next);
  };
  const clear = () => { setText(''); setParams({}); };

  const data = query.data;
  const chips = interpretationChips(data?.interpretation, categories);
  const totalPages = data ? Math.ceil((data.totalCount || 0) / PAGE_SIZE) : 0;

  return (
    <section aria-label="AI product search" className="mb-7 rounded-2xl border border-primary/25 bg-primary-soft/40 p-5 sm:p-6">
      <form role="search" onSubmit={(event) => { event.preventDefault(); submit(); }} className="flex flex-col gap-3 sm:flex-row">
        <label className="sr-only" htmlFor="ai-product-search">Describe what you are looking for</label>
        <div className="relative flex-1">
          <span aria-hidden="true" className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-primary">✦</span>
          <input
            id="ai-product-search"
            type="search"
            value={text}
            onChange={(e) => setText(e.target.value)}
            maxLength={300}
            placeholder="Ask in Bangla, Banglish or English — e.g. “10,000 takar moddhe ekta bhalo Jamdani dao”"
            className="w-full rounded-full border border-border bg-surface py-3 pl-10 pr-4 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/25"
          />
        </div>
        <button type="submit" disabled={text.trim().length < 2 || query.isFetching} className="rounded-full bg-primary px-6 py-3 text-sm font-semibold text-surface transition hover:bg-primary-dark disabled:opacity-50">
          {query.isFetching && active ? 'Searching…' : 'Ask AI'}
        </button>
        {active && <button type="button" onClick={clear} className="rounded-full border border-border bg-surface px-5 py-3 text-sm font-medium text-heading hover:border-primary/40">Show all products</button>}
      </form>

      {!active && (
        <div className="mt-4 flex flex-wrap items-center gap-2 text-xs">
          <span className="text-body/60">Try:</span>
          {EXAMPLES.map((example) => (
            <button key={example} type="button" onClick={() => submit(example)} className="rounded-full border border-border bg-surface px-3 py-1.5 text-body hover:border-primary/40 hover:text-primary">{example}</button>
          ))}
        </div>
      )}

      {active && (
        <div className="mt-5" aria-live="polite">
          {query.isPending && <p role="status" className="text-sm text-body/70">Understanding your request and finding products…</p>}
          {query.isError && <p role="alert" className="rounded-md border border-error/30 bg-error/5 px-3 py-2 text-sm text-error">{getApiErrorMessage(query.error, 'Search is unavailable right now. Please try again.')}</p>}

          {data && (
            <>
              <div className="flex flex-wrap items-center gap-2">
                <p role="status" className="text-sm font-medium text-heading">{data.totalCount} {data.totalCount === 1 ? 'product' : 'products'} for “{data.query}”</p>
                {chips.map((chip) => <span key={chip} className="rounded-full border border-primary/30 bg-surface px-2.5 py-1 text-xs font-medium text-primary">{chip}</span>)}
              </div>
              <p className="mt-1 text-xs text-body/60">{MODE_NOTES[data.mode] || ''}</p>
              {data.notice && <p role="note" className="mt-3 rounded-md border border-secondary/40 bg-secondary/10 px-3 py-2 text-sm text-heading">{data.notice}</p>}

              {data.items.length === 0 ? (
                <p className="mt-6 rounded-xl border border-border bg-surface p-8 text-center text-sm text-body/70">No products matched. Try a different wording, a higher budget, or <button type="button" onClick={clear} className="font-semibold text-primary underline">browse all products</button>.</p>
              ) : (
                <ul className="mt-5 grid gap-5 sm:grid-cols-2 xl:grid-cols-3" aria-busy={query.isFetching}>
                  {data.items.map((item) => (
                    <li key={item.id} className="flex flex-col">
                      <ProductCard product={toCard(item)} to={productPath.replace(':productId', item.id)} />
                      <div className="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1 px-1 text-xs text-body/70">
                        <span className={item.inStock ? 'font-medium text-success' : item.madeToOrder ? 'font-medium text-secondary' : 'font-medium text-error'}>
                          {item.inStock ? 'In stock' : item.madeToOrder ? 'Made to order' : 'Out of stock'}
                        </span>
                        {item.reviewCount > 0 ? <span>★ {Number(item.averageRating).toFixed(1)} ({item.reviewCount})</span> : <span>No reviews yet</span>}
                        {item.discountPrice != null && <span className="line-through">{money(item.price)}</span>}
                        {item.reasons?.map((reason) => <span key={reason} className="rounded bg-primary/10 px-1.5 py-0.5 text-primary">{reason}</span>)}
                      </div>
                    </li>
                  ))}
                </ul>
              )}

              {totalPages > 1 && (
                <div className="mt-8">
                  <Pagination currentPage={page} totalPages={totalPages} onPageChange={(value) => { const next = new URLSearchParams(params); next.set('aiPage', String(value)); setParams(next); }} />
                </div>
              )}
            </>
          )}
        </div>
      )}
    </section>
  );
}
