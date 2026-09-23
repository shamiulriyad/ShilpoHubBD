import { useSearchParams } from 'react-router-dom';
import { PageHeader } from '../../components/ui';
import { useLocalCuisines } from '../../hooks/useLocalCuisines';
import SafeImage from '../../components/media/SafeImage';
import { cuisineGuides } from '../../data/tourismGuides';
import { DirectoryFilters, EmptyResults, LiveDataNotice } from '../../components/tourism/TravelUI';

export default function LocalCuisines() {
  const query=useLocalCuisines({pageSize:100});
  const [params,setParams]=useSearchParams();
  const search=params.get('q')||'', district=params.get('district')||'',kind=params.get('kind')||'';
  const records=[...cuisineGuides,...(query.data?.items||[]).filter(c=>!cuisineGuides.some(g=>g.name.toLowerCase()===c.name.toLowerCase())).map(c=>({...c,kind:'Local listing'}))];
  // "Across Bangladesh" dishes (ilish, khichuri...) are available everywhere, so they match any
  // district choice; picking that value itself shows only the nationwide dishes.
  const NATIONWIDE='Across Bangladesh';
  const matches=(c,{d=district,k=kind}={})=>(!d||c.districtName===d||(d!==NATIONWIDE&&c.districtName===NATIONWIDE))&&(!k||c.kind===k)&&[c.name,c.districtName,c.description].join(' ').toLowerCase().includes(search.trim().toLowerCase());
  const filtered=records.filter(c=>matches(c));
  // Each dropdown only offers values that still have dishes under the OTHER current filter, with a
  // count, so a combination that can only give "0 dishes" is never presented as a choice.
  const withCounts=(values,countFor,selected)=>{const list=[...new Set([...values,...(selected?[selected]:[])])];return list.map(v=>[v,`${v===NATIONWIDE?'Nationwide dishes':v} (${countFor(v)})`]);};
  const districtOptions=withCounts(records.map(c=>c.districtName).filter(Boolean).sort((a,b)=>a===NATIONWIDE?-1:b===NATIONWIDE?1:a.localeCompare(b)),v=>records.filter(c=>matches(c,{d:v})).length,district);
  const kindOptions=withCounts([...new Set(records.map(c=>c.kind))].sort(),v=>records.filter(c=>matches(c,{k:v})).length,kind);
  const change=(key,value)=>{const next=new URLSearchParams(params);if(value)next.set(key,value);else next.delete(key);setParams(next,{replace:true});};
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="A taste of Bangladesh" description="Discover regional dishes, their ingredients and where to look for them on your journey." breadcrumbs={[{label:'Home',path:'/'},{label:'Local Cuisine'}]}/>
    <DirectoryFilters search={search} onSearch={v=>change('q',v)} onClear={()=>setParams({})} filters={[{label:'District',value:district,onChange:v=>change('district',v),options:districtOptions},{label:'Dish type',value:kind,onChange:v=>change('kind',v),options:kindOptions}]}/>
    <LiveDataNotice query={query} label="Local food listings"/>
    <p role="status" className="mb-4 text-sm text-body/65">{filtered.length} dishes</p>
    <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">{filtered.map(c=><article key={c.id} className="overflow-hidden rounded-xl border border-border bg-surface">{c.imageUrl&&<SafeImage src={c.imageUrl} alt={c.name} className="aspect-video w-full object-cover"/>}<div className="p-6"><p className="text-xs font-semibold uppercase tracking-wide text-primary">{c.districtName} · {c.kind}</p><h2 className="mt-3 text-xl font-semibold text-heading">{c.name}</h2><p className="mt-3 text-sm leading-7 text-body/80">{c.description}</p><dl className="mt-5 space-y-4 border-t border-border pt-4">{c.ingredients&&<div><dt className="text-xs font-semibold uppercase tracking-wide">Typical ingredients</dt><dd className="mt-2 text-sm leading-6 text-body/75">{c.ingredients}</dd></div>}{c.whereToTry&&<div><dt className="text-xs font-semibold uppercase tracking-wide">Where to try</dt><dd className="mt-2 text-sm leading-6 text-body/75">{c.whereToTry}</dd></div>}</dl>{c.source&&<a href={c.source} target="_blank" rel="noreferrer" className="mt-5 inline-block text-xs text-link underline">Bangladesh Tourism Board reference ↗</a>}</div></article>)}</div>
    {!filtered.length&&<EmptyResults onClear={()=>setParams({})}/>}
    <p className="mt-6 text-xs leading-6 text-body/65">Recipes vary. Ask the restaurant about ingredients and allergens; these guides do not guarantee any dish is allergen-free.</p>
  </div>;
}
