import { useSearchParams, Link } from 'react-router-dom';
import { PageHeader, QueryState } from '../../components/ui';
import { useTouristServices } from '../../hooks/useTouristServices';
import SafeImage from '../../components/media/SafeImage';
import { useSiteContent } from '../../hooks/useSiteContent';
import { DirectoryFilters } from '../../components/tourism/TravelUI';
import { routePaths } from '../../routes/routePaths';

const types={GuideBooking:'Local guide',WorkshopBooking:'Craft workshop',ArtisanHomeVisit:'Artisan visit',HomestayBooking:'Accommodation',TransportationBooking:'Transport'};
export default function TouristServices() {
  const [params,setParams]=useSearchParams();
  // Admin › CMS › Site Content › travel-resource; `extra` is "<service type>|<button text>", a link starting with / stays in the app.
  const travelResources=useSiteContent().group('travel-resource').map(i=>{const [resourceType,action]=(i.extra||'').split('|');return {name:i.title,label:i.subtitle,description:i.body,type:resourceType,action:action||'Open',...(i.linkUrl?.startsWith('/')?{to:i.linkUrl}:{url:i.linkUrl})};});
  const type=params.get('type')||'',search=params.get('q')||'';
  const query=useTouristServices({type:type||undefined,pageSize:100});
  const change=(key,value)=>{const next=new URLSearchParams(params);if(value)next.set(key,value);else next.delete(key);setParams(next,{replace:true});};
  const services=(query.data?.items||[]).filter(s=>[s.title,s.producerName,s.districtName].join(' ').toLowerCase().includes(search.trim().toLowerCase()));
  const resources=travelResources.filter(r=>(!type||r.type===type)&&[r.name,r.label,r.description].join(' ').toLowerCase().includes(search.trim().toLowerCase()));
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Plan a thoughtful journey" description="Find published local experiences and official resources for accommodation, transport and travel planning." breadcrumbs={[{label:'Home',path:'/'},{label:'Tourist Services'}]}/>
    <DirectoryFilters search={search} onSearch={v=>change('q',v)} onClear={()=>setParams({})} filters={[{label:'Service type',value:type,onChange:v=>change('type',v),options:Object.entries(types)}]}/>
    <section className="mb-9"><h2 className="mb-2 text-xl font-semibold">Local experiences</h2><p className="mb-5 text-sm text-body/70">Provider listings with live details and booking availability.</p>
      <QueryState query={query} isEmpty={()=>false}>{()=>services.length?<div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{services.map(s=><Link key={s.id} to={routePaths.tourismServiceDetails.replace(':serviceId',s.id)} className="overflow-hidden rounded-xl border border-border bg-surface transition hover:border-primary">{s.imageUrl&&<SafeImage src={s.imageUrl} alt={s.title} className="aspect-video w-full object-cover"/>}<div className="p-5"><p className="text-xs uppercase tracking-wide text-primary">{types[s.type]||s.type}</p><h3 className="mt-2 text-lg font-semibold">{s.title}</h3><p className="mt-2 text-sm text-body/70">{s.producerName} · {s.districtName}</p><p className="mt-4 font-semibold">৳ {Number(s.price).toLocaleString('en-BD')}</p><p className="mt-4 text-sm font-semibold text-link">View availability →</p></div></Link>)}</div>:<div className="rounded-xl border border-border bg-surface p-6"><h3 className="font-semibold">No matching local experiences are published yet</h3><p className="mt-2 text-sm leading-6 text-body/75">Explore the travel resources below. Local booking options will appear when providers publish their services.</p></div>}</QueryState>
    </section>
    <section><h2 className="text-xl font-semibold">Travel resources & visit planning</h2><p className="mb-5 mt-2 text-sm text-body/70">Official external services and community guides. External reservations are handled by the linked provider.</p><div className="grid gap-5 md:grid-cols-2">{resources.map(r=><article key={r.name} className="flex flex-col rounded-xl border border-border bg-surface p-6"><p className="text-xs font-semibold uppercase tracking-wide text-primary">{r.label}</p><h3 className="mt-3 text-xl font-semibold">{r.name}</h3><p className="mb-5 mt-3 text-sm leading-7 text-body/75">{r.description}</p>{r.to?<Link to={r.to} className="mt-auto text-sm font-semibold text-link">{r.action} →</Link>:<a href={r.url} target="_blank" rel="noreferrer" className="mt-auto text-sm font-semibold text-link">{r.action} ↗</a>}</article>)}</div>{!resources.length&&<p className="rounded-lg border border-border p-5 text-sm">No resources match these filters. <button onClick={()=>setParams({})} className="text-link underline">Reset filters</button></p>}</section>
  </div>;
}
