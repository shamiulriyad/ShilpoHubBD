import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState, Pagination } from '../../components/ui';
import { LanguageToggle, PerformingArtCard } from '../../components/heritage/PerformingArtsParts';
import { useContentLanguage, useFeaturedPerformingArts, usePerformingArtFacets, usePerformingArts } from '../../hooks/usePerformingArts';
import { CATEGORIES, DIVISIONS, divisionLabel, text } from '../../utils/performingArtsAdapters';

const PAGE_SIZE = 12;
const selectClass = 'w-full rounded-lg border border-border bg-surface px-3 py-2.5 text-sm text-heading focus:outline-none focus:ring-2 focus:ring-primary/30';

// Records are curated in Admin › Heritage Management › Performing Arts; only Published ones reach this page.
export default function PerformingArts() {
  const [lang, setLang] = useContentLanguage();
  const t = text(lang);
  const [params, setParams] = useSearchParams();
  const search = params.get('q') || '';
  const category = params.get('category') || '';
  const division = params.get('division') || '';
  const districtId = params.get('district') || '';
  const page = Math.max(1, Number(params.get('page')) || 1);
  const [searchInput, setSearchInput] = useState(search);

  // Functional update so a debounced search never overwrites a filter chosen in the meantime.
  const update = (changes) => setParams((current) => {
    const next = new URLSearchParams(current);
    for (const [key, value] of Object.entries(changes)) value ? next.set(key, value) : next.delete(key);
    if (!('page' in changes)) next.delete('page');
    return next;
  }, { replace: true });

  // Follow back/forward navigation, but don't strip the space a visitor is in the middle of typing.
  useEffect(() => setSearchInput((current) => (current.trim() === search ? current : search)), [search]);
  useEffect(() => {
    if (searchInput.trim() === search) return undefined;
    const timer = setTimeout(() => update({ q: searchInput.trim() }), 350);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput]);

  const query = usePerformingArts({
    search: search || undefined, category: category || undefined, division: division || undefined,
    districtId: districtId || undefined, page, pageSize: PAGE_SIZE,
  });
  const facets = usePerformingArtFacets();
  const filtersActive = Boolean(search || category || division || districtId);
  const featured = useFeaturedPerformingArts(3);

  const categoryCounts = useMemo(() => Object.fromEntries((facets.data?.categories || []).map((c) => [c.category, c.count])), [facets.data]);
  const districtOptions = useMemo(
    () => (facets.data?.districts || []).filter((d) => !division || d.division === division),
    [facets.data, division],
  );
  const records = query.data?.items || [];
  const total = query.data?.totalCount ?? 0;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8" lang={lang}>
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Performing Arts' },
        ]}
        title={t.title}
        description={t.description}
      />

      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <LanguageToggle lang={lang} onChange={setLang} />
        {filtersActive && (
          <button type="button" onClick={() => { setSearchInput(''); update({ q: '', category: '', division: '', district: '' }); }} className="text-sm font-semibold text-primary hover:underline">
            {t.clear}
          </button>
        )}
      </div>

      <form role="search" onSubmit={(e) => { e.preventDefault(); update({ q: searchInput.trim() }); }} className="mb-5">
        <label htmlFor="pa-search" className="sr-only">{t.searchPlaceholder}</label>
        <input
          id="pa-search"
          type="search"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder={t.searchPlaceholder}
          className="w-full rounded-full border border-border bg-surface px-5 py-3 text-sm text-heading shadow-[0_12px_32px_rgba(23,59,53,0.09)] focus:border-primary/50 focus:outline-none focus:ring-4 focus:ring-primary/10"
        />
      </form>

      <div className="mb-5 flex flex-wrap gap-2" role="group" aria-label={t.allCategories}>
        {[{ value: '', en: text('en').allCategories, bn: text('bn').allCategories }, ...CATEGORIES].map((c) => {
          const active = category === c.value;
          const count = c.value ? categoryCounts[c.value] ?? 0 : null;
          return (
            <button
              key={c.value || 'all'}
              type="button"
              aria-pressed={active}
              onClick={() => update({ category: c.value })}
              className={`rounded-full border px-4 py-2 text-sm font-medium transition ${active ? 'border-primary bg-primary text-white' : 'border-border bg-surface text-heading hover:border-primary'}`}
            >
              {c[lang] || c.en}{count !== null && <span className="ml-1.5 opacity-70">({count})</span>}
            </button>
          );
        })}
      </div>

      <div className="mb-8 grid gap-3 sm:grid-cols-2 lg:max-w-xl">
        <div>
          <label htmlFor="pa-division" className="mb-1 block text-xs font-semibold uppercase tracking-wide text-body/60">{t.division}</label>
          <select id="pa-division" className={selectClass} value={division} onChange={(e) => update({ division: e.target.value, district: '' })}>
            <option value="">{t.allDivisions}</option>
            {DIVISIONS.map((d) => <option key={d.value} value={d.value}>{divisionLabel(d.value, lang)}</option>)}
          </select>
        </div>
        <div>
          <label htmlFor="pa-district" className="mb-1 block text-xs font-semibold uppercase tracking-wide text-body/60">{t.district}</label>
          <select id="pa-district" className={selectClass} value={districtId} onChange={(e) => update({ district: e.target.value })} disabled={facets.isLoading}>
            <option value="">{t.allDistricts}</option>
            {districtOptions.map((d) => <option key={d.id} value={d.id} lang="en">{d.name} ({d.count})</option>)}
          </select>
        </div>
      </div>

      {!filtersActive && page === 1 && featured.data?.length > 0 && (
        <section aria-labelledby="pa-featured" className="mb-10">
          <h2 id="pa-featured" className="mb-4 text-xl font-semibold text-heading">{t.featured}</h2>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {featured.data.map((record) => <PerformingArtCard key={record.id} record={record} lang={lang} />)}
          </div>
        </section>
      )}

      <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error} loadingText={t.loading}>
        <p className="mb-4 text-sm text-body/60" role="status" aria-live="polite">{t.results(total)}</p>
        {records.length === 0 ? (
          <p className="rounded-xl border border-border bg-surface p-8 text-center text-sm text-body/60">{filtersActive ? t.empty : t.emptyAll}</p>
        ) : (
          <div className={`grid gap-4 sm:grid-cols-2 lg:grid-cols-3 ${query.isFetching ? 'opacity-60 transition-opacity' : ''}`} aria-busy={query.isFetching}>
            {records.map((record) => <PerformingArtCard key={record.id} record={record} lang={lang} />)}
          </div>
        )}
        {(query.data?.totalPages || 0) > 1 && (
          <div className="mt-8">
            <Pagination currentPage={page} totalPages={query.data.totalPages} onPageChange={(next) => update({ page: String(next) })} />
          </div>
        )}
      </AsyncState>
    </div>
  );
}
