import { useMemo } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { PageHeader } from '../../components/ui';
import { useHeritagePlaces } from '../../hooks/useHeritagePlaces';
import HeritageLeafletMap from '../../components/tourism/HeritageLeafletMap';
import { DirectoryFilters, TravelPhoto, LiveDataNotice, EmptyResults } from '../../components/tourism/TravelUI';
import { destinationReferences, normaliseDistrict, hasCoordinates } from '../../data/tourismGuides';
import { routePaths } from '../../routes/routePaths';

export default function HeritageMap() {
  const [params,setParams]=useSearchParams();
  const query=useHeritagePlaces({pageSize:100});
  const search=params.get('q')||'', district=params.get('district')||'', type=params.get('type')||'';
  const places=useMemo(()=>{
    const live=(query.data?.items||[]).map(p=>({...p,reference:false}));
    return [...live,...destinationReferences.filter(p=>!live.some(l=>l.name.toLowerCase()===p.name.toLowerCase()))];
  },[query.data]);
  const filtered=useMemo(()=>places.filter(p=>(!district||normaliseDistrict(p.districtName)===normaliseDistrict(district))&&(!type||p.placeType===type)&&[p.name,p.districtName,p.description,p.knownFor].join(' ').toLowerCase().includes(search.trim().toLowerCase())),[places,district,type,search]);
  const selected=filtered.find(p=>p.id===params.get('place'));
  const change=(key,value)=>{const next=new URLSearchParams(params);if(value)next.set(key,value);else next.delete(key);if(key!=='place')next.delete('place');setParams(next,{replace:key==='q'});};
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Heritage Map" description="Find the places behind Bangladesh’s stories. Explore real locations, then plan your visit." breadcrumbs={[{label:'Dashboard',path:routePaths.tourist},{label:'Heritage Map'}]}/>
    <DirectoryFilters search={search} onSearch={v=>change('q',v)} onClear={()=>setParams({})} filters={[{label:'District',value:district,onChange:v=>change('district',v),options:[...new Set(places.map(p=>p.districtName).filter(Boolean))].sort()},{label:'Experience',value:type,onChange:v=>change('type',v),options:[...new Set(places.map(p=>p.placeType).filter(Boolean))].sort()}]}/>
    <LiveDataNotice query={query} label="Published places"/>
    <div className="grid items-start gap-5 xl:grid-cols-[minmax(0,1.6fr)_minmax(260px,1fr)]">
      <HeritageLeafletMap places={filtered} selectedId={selected?.id} onSelect={id=>change('place',id)}/>
      <aside aria-label="Place details" className="overflow-hidden rounded-xl border border-border bg-surface">
        {selected ? <><TravelPhoto image={selected.image}/><div className="p-5"><p className="text-xs font-semibold uppercase tracking-wide text-primary">{selected.districtName} · {selected.placeType}</p><h2 className="mt-2 text-2xl font-semibold text-heading">{selected.name}</h2><p className="mt-3 text-sm leading-7 text-body/80">{selected.description || selected.knownFor}</p>{hasCoordinates(selected)&&<a target="_blank" rel="noreferrer" className="mt-5 inline-flex rounded-lg bg-primary px-4 py-3 text-sm font-semibold text-white" href={`https://www.openstreetmap.org/?mlat=${selected.latitude}&mlon=${selected.longitude}#map=15/${selected.latitude}/${selected.longitude}`}>Open larger map ↗</a>}{selected.reference?<a href={selected.source} target="_blank" rel="noreferrer" className="mt-4 block text-sm text-link underline">Read destination reference ↗</a>:<Link to={routePaths.tourismPlaceDetails.replace(':placeId',selected.id)} className="mt-4 block text-sm text-link underline">View published place details →</Link>}<button onClick={()=>change('place','')} className="mt-5 block text-sm text-body/70 underline">Clear selection</button></div></>:<div className="p-7"><p className="text-xs font-semibold uppercase tracking-widest text-primary">Start exploring</p><h2 className="mt-3 text-2xl font-semibold">A place for every curiosity.</h2><p className="mt-4 text-sm leading-7 text-body/75">Select a marker or a place below to see its story, photograph and location. Filter for architecture, nature, craft communities and more.</p><p className="mt-6 border-t border-border pt-4 text-sm">{filtered.filter(hasCoordinates).length} locations on the map</p></div>}
      </aside>
    </div>
    <div className="mb-4 mt-8 flex items-center justify-between"><h2 className="text-xl font-semibold text-heading">Explore places</h2><p role="status" className="text-sm text-body/65">{filtered.length} places</p></div>
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{filtered.map(p=><button key={p.id} onClick={()=>change('place',p.id)} aria-pressed={selected?.id===p.id} className={`rounded-xl border p-5 text-left transition hover:border-primary ${selected?.id===p.id?'border-primary bg-primary/5':'border-border bg-surface'}`}><span className="text-xs text-primary">{p.districtName} · {p.placeType}</span><span className="mt-2 block font-semibold text-heading">{p.name}</span><span className="mt-2 block text-sm text-body/70">{p.knownFor || 'Select for details'}</span>{!hasCoordinates(p)&&<span className="mt-2 block text-xs">Location not yet mapped</span>}</button>)}</div>
    {!filtered.length&&<EmptyResults onClear={()=>setParams({})}/>}
  </div>;
}
