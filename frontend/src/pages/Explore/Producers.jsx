import { Link, useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { DirectoryFilters, EmptyResults } from '../../components/tourism/TravelUI';
import { useProducerDirectory } from '../../hooks/useProducerDirectory';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';

export default function Producers() {
  const query=useProducerDirectory(), categories=useCategories(),districts=useDistricts();
  const [params,setParams]=useSearchParams();
  const search=params.get('q')||'', craft=params.get('craft')||'',district=params.get('district')||'',sort=params.get('sort')||'name';
  const change=(key,value)=>{const next=new URLSearchParams(params);if(value)next.set(key,value);else next.delete(key);setParams(next,{replace:key==='q'});};
  const filtered=(query.data||[]).filter(p=>(!craft||p.crafts.some(c=>c.id===craft))&&(!district||p.districts.some(d=>d.id===district))&&[p.name,...p.crafts.map(c=>c.name),...p.districts.map(d=>d.name)].join(' ').toLowerCase().includes(search.trim().toLowerCase())).sort((a,b)=>sort==='products'?b.productCount-a.productCount:a.name.localeCompare(b.name));
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Meet the makers" description="Explore makers represented in the published product collection. Find their crafts, locations and available work." breadcrumbs={[{label:'Home',path:'/'},{label:'Explore',path:routePaths.explore},{label:'Producers'}]}/>
    <DirectoryFilters search={search} onSearch={v=>change('q',v)} onClear={()=>setParams({})} filters={[{label:'Craft',value:craft,onChange:v=>change('craft',v),options:(categories.data||[]).map(c=>[c.id,c.name])},{label:'District',value:district,onChange:v=>change('district',v),options:(districts.data||[]).map(d=>[d.id,d.name])}]}/>
    {(categories.isError||districts.isError)&&<p role="alert" className="mb-4 text-sm">Some filter options could not load. <button onClick={()=>{categories.refetch();districts.refetch();}} className="text-link underline">Retry filters</button></p>}
    <div className="mb-5 flex flex-wrap items-center justify-between gap-4"><p role="status" className="text-sm text-body/65">{filtered.length} makers</p><label className="flex items-center gap-3 text-sm">Sort by<select value={sort} onChange={e=>change('sort',e.target.value)} className="rounded-lg border border-border bg-surface px-3 py-2"><option value="name">Name A–Z</option><option value="products">Most published products</option></select></label></div>
    <QueryState query={query} emptyLabel="No makers have published products yet.">
      {()=> <><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{filtered.map(p=><article key={p.id} className="flex flex-col rounded-xl border border-border bg-surface p-6"><div className="flex items-center gap-4"><span aria-hidden="true" className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xl font-semibold text-primary">{p.name.trim().slice(0,2).toUpperCase()}</span><div><h2 className="text-xl font-semibold text-heading">{p.name}</h2><p className="mt-1 text-xs text-body/65">{p.districts.map(d=>d.name).join(' · ')}</p></div></div><div className="mt-5 flex flex-wrap gap-2">{p.crafts.map(c=><span key={c.id} className="rounded-md bg-background px-3 py-1.5 text-xs text-body">{c.name}</span>)}</div><dl className="mb-5 mt-6 grid grid-cols-2 gap-3 border-t border-border pt-4"><div><dt className="text-xs text-body/65">Published work</dt><dd className="mt-1 font-semibold">{p.productCount} products</dd></div><div><dt className="text-xs text-body/65">Prices from</dt><dd className="mt-1 font-semibold">৳ {p.minPrice.toLocaleString('en-BD')}</dd></div></dl><Link to={`${routePaths.marketplaceProducts}?producerId=${p.id}`} className="mt-auto text-sm font-semibold text-link">View maker’s collection →</Link></article>)}</div>{!filtered.length&&<EmptyResults onClear={()=>setParams({})}/>}</>}
    </QueryState>
  </div>;
}
