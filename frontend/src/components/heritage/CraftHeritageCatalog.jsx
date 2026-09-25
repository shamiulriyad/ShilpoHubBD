import { Link, useSearchParams } from 'react-router-dom';
import { filterCraftHeritage, heritageForCategory } from '../../data/craftHeritage';
import { useCraftHeritage } from '../../hooks/useSiteContent';
import { routePaths } from '../../routes/routePaths';
import CraftHeritageDetails from './CraftHeritageDetails';

export default function CraftHeritageCatalog({ categories = [] }) {
  const [params, setParams] = useSearchParams();
  const { records: craftHeritage } = useCraftHeritage();
  const query = params.get('q') || '';
  const recognition = params.get('recognition') || 'all';
  const selected = craftHeritage.find(c => c.slug === params.get('craft'));
  const results = filterCraftHeritage(craftHeritage, query, recognition);
  const urlFor = (slug) => { const next = new URLSearchParams(params); if (slug) next.set('craft', slug); else next.delete('craft'); return `?${next}`; };
  const update = (key, value) => { const next = new URLSearchParams(params); if (value) next.set(key, value); else next.delete(key); setParams(next, { replace: true }); };
  const category = selected && categories.find(c => heritageForCategory(c, craftHeritage)?.slug === selected.slug);
  if (selected) return <div><Link to={urlFor(null)} className="mb-5 inline-flex rounded-lg border border-border bg-surface px-4 py-2 text-sm font-medium text-link">← All crafts</Link><CraftHeritageDetails craft={selected} />{category && <Link className="inline-flex rounded-lg bg-primary px-5 py-3 text-sm font-semibold text-white" to={routePaths.exploreCraftDetails.replace(':craftId', category.id)}>Browse {selected.name} products →</Link>}</div>;
  return (
    <section aria-label="Craft heritage directory">
      <div className="mb-6 rounded-2xl border border-border bg-surface p-5 sm:p-7">
        <p className="text-xs font-semibold uppercase tracking-[0.18em] text-primary">The heritage collection</p>
        <h2 className="mt-2 text-2xl font-semibold text-heading">Discover the stories behind the craft.</h2>
        <p className="mt-2 max-w-2xl text-sm leading-6 text-body/75">Explore materials, making traditions and regional identity. GI registrations and UNESCO inscriptions are identified separately.</p>
        <div className="mt-6 grid gap-4 sm:grid-cols-[1fr_auto]">
          <label className="text-sm font-medium">Search crafts<input type="search" value={query} onChange={e => update('q', e.target.value)} placeholder="Name, region or material…" className="mt-2 block w-full rounded-lg border border-border bg-background px-4 py-3 font-normal" /></label>
          <label className="text-sm font-medium">Recognition<select value={recognition} onChange={e => update('recognition', e.target.value)} className="mt-2 block w-full rounded-lg border border-border bg-background px-4 py-3 font-normal"><option value="all">All traditions</option><option value="gi">GI registered heritage</option><option value="unesco">UNESCO listed traditions</option></select></label>
        </div>
      </div>
      <p role="status" className="mb-4 text-sm text-body/65">{results.length} craft{results.length === 1 ? '' : 's'} found</p>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {results.map(c => <Link key={c.slug} to={urlFor(c.slug)} className="group flex flex-col rounded-xl border border-border bg-surface p-6 transition hover:border-primary/50 hover:shadow-md focus-visible:outline focus-visible:outline-2 focus-visible:outline-primary">
          <div className="mb-5 flex flex-wrap gap-2 text-[11px] font-semibold uppercase tracking-wide"><span className="text-body/60">{c.type}</span>{c.giName && <span className="rounded bg-primary/10 px-2 text-primary">GI registered</span>}{c.unesco && <span className="rounded bg-emerald-50 px-2 text-emerald-800">UNESCO</span>}</div>
          <h3 className="text-xl font-semibold text-heading group-hover:text-primary">{c.name}</h3>
          <p className="mt-2 text-xs text-primary">{c.region}</p>
          <p className="mb-6 mt-3 text-sm leading-6 text-body/75">{c.summary}</p>
          <span className="mt-auto text-sm font-semibold text-link">Explore craft <span aria-hidden="true">→</span></span>
        </Link>)}
      </div>
      {results.length === 0 && <div className="rounded-xl border border-border bg-surface p-8 text-center"><h3 className="font-semibold">No matching crafts</h3><p className="mt-2 text-sm">Try another name or clear the recognition filter.</p><button type="button" onClick={() => { const next = new URLSearchParams(params); next.delete('q'); next.delete('recognition'); setParams(next); }} className="mt-4 text-sm font-semibold text-link underline">Clear filters</button></div>}
    </section>
  );
}
