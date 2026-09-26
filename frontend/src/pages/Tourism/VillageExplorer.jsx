import { useSearchParams } from 'react-router-dom';
import { PageHeader } from '../../components/ui';
import { useVillageTourStops } from '../../hooks/useVillageTour';
import SafeImage from '../../components/media/SafeImage';
import { useVillages } from '../../hooks/useVillages';
import { toPhoto } from '../../utils/tourismAdapters';
import { DirectoryFilters, TravelPhoto, EmptyResults, LiveDataNotice } from '../../components/tourism/TravelUI';

export default function VillageExplorer() {
  const [params,setParams]=useSearchParams();
  const query=useVillageTourStops({pageSize:50});
  const villagesQuery=useVillages();
  const villageGuides=(villagesQuery.data||[]).map(v=>({id:v.id,name:v.name,districtName:v.districtName,craft:v.craft,description:v.description,visit:v.visitTips,image:toPhoto(v.imageUrl,v.name,v.imageCredit),source:v.sourceUrl,sourceLabel:v.sourceLabel||'Source',search:v.name+' '+v.districtName+' Bangladesh'}));
  const search=params.get('q')||'',district=params.get('district')||'';
  const change=(key,value)=>{const next=new URLSearchParams(params);if(value)next.set(key,value);else next.delete(key);setParams(next,{replace:true});};
  const guides=villageGuides.filter(v=>(!district||v.districtName===district)&&[v.name,v.craft,v.districtName].join(' ').toLowerCase().includes(search.toLowerCase().trim()));
  const stops=query.data?.items||[];
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Village & community explorer" description="Meet the places behind the craft. Discover weaving communities, rural heritage and practical ideas for respectful visits." breadcrumbs={[{label:'Home',path:'/'},{label:'Village Explorer'}]}/>
    <DirectoryFilters search={search} onSearch={v=>change('q',v)} filters={[{label:'District',value:district,onChange:v=>change('district',v),options:[...new Set(villageGuides.map(v=>v.districtName))].sort()}]} onClear={()=>setParams({})}/>
    <LiveDataNotice query={villagesQuery} label="Community guides"/>
    <p role="status" className="mb-4 text-sm text-body/65">{guides.length} community guides</p>
    <div className="grid gap-5 md:grid-cols-2">{guides.map(v=><article key={v.id} className="overflow-hidden rounded-xl border border-border bg-surface"><TravelPhoto image={v.image}/><div className="p-6"><p className="text-xs font-semibold uppercase tracking-wide text-primary">{v.districtName} · {v.craft}</p><h2 className="mt-3 text-xl font-semibold text-heading">{v.name}</h2><p className="mt-3 text-sm leading-7 text-body/80">{v.description}</p>{v.visit&&<div className="mt-5 rounded-lg bg-background p-4"><h3 className="text-sm font-semibold">Planning a visit</h3><p className="mt-2 text-sm leading-6 text-body/75">{v.visit}</p></div>}<div className="mt-5 flex flex-wrap gap-4 text-sm"><a href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(v.search)}`} target="_blank" rel="noreferrer" className="font-semibold text-link">Find the area ↗</a>{v.source&&<a href={v.source} target="_blank" rel="noreferrer" className="text-link underline">{v.sourceLabel} ↗</a>}</div></div></article>)}</div>
    {villagesQuery.isSuccess&&!guides.length&&<EmptyResults onClear={()=>setParams({})}/>}
    <section className="mt-9"><h2 className="mb-3 text-xl font-semibold">Published virtual visits</h2><LiveDataNotice query={query} label="Virtual visits"/>
      <div className="grid gap-5 md:grid-cols-2">{stops.map(stop=><article key={stop.id} className="overflow-hidden rounded-xl border border-border bg-surface">{['Video','Video360'].includes(stop.mediaType)?<video src={stop.mediaUrl} poster={stop.thumbnailUrl} controls preload="none" aria-label={stop.title} className="aspect-video w-full"/>:<SafeImage src={stop.thumbnailUrl||stop.mediaUrl} alt={stop.title} className="aspect-video w-full object-cover"/>}<div className="p-5"><h3 className="font-semibold">{stop.title}</h3><p className="mt-2 text-sm">{stop.heritagePlaceName}</p></div></article>)}</div>
      {query.isSuccess&&!stops.length&&<p className="rounded-xl border border-border bg-surface p-5 text-sm text-body/75">Virtual tours have not been published yet. Use the community guides above to plan a visit.</p>}
    </section>
  </div>;
}
