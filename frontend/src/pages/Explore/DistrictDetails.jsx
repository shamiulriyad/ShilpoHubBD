import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { districtReference, placesForDistrict, villageGuides, cuisineGuides, normaliseDistrict } from '../../data/tourismGuides';
import { TravelPhoto, DestinationCard } from '../../components/tourism/TravelUI';

export default function DistrictDetails() {
  const {districtId}=useParams();
  const query=useDistricts();
  const district=(query.data||[]).find(d=>d.id===districtId);
  const reference=districtReference(district?.name);
  const places=placesForDistrict(district?.name);
  const villages=villageGuides.filter(v=>normaliseDistrict(v.districtName)===normaliseDistrict(district?.name));
  const cuisines=cuisineGuides.filter(v=>normaliseDistrict(v.districtName)===normaliseDistrict(district?.name));
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <QueryState query={query} isEmpty={()=>!district} emptyLabel="District not found.">
      {()=> <><PageHeader title={district.name} description={district.division+' Division · A guide to place, culture and local identity'} breadcrumbs={[{label:'Home',path:'/'},{label:'Districts',path:routePaths.exploreDistricts},{label:district.name}]}/>
        <section className="mb-8 overflow-hidden rounded-2xl border border-border bg-surface">
          <TravelPhoto image={reference?.image} className="max-h-[380px] w-full" eager/>
          <div className="p-6 sm:p-8"><p className="text-xs font-semibold uppercase tracking-widest text-primary">Discover {district.name}</p><h2 className="mt-3 text-2xl font-semibold text-heading">{reference?.knownFor ? 'Known for & places of interest' : 'The district at a glance'}</h2>{reference?.knownFor&&<p className="mt-3 max-w-3xl text-lg leading-8 text-heading">{reference.knownFor}</p>}<p className="mt-4 max-w-3xl text-sm leading-7 text-body/80">{reference?.description || district.description || 'Explore district information and local places through the references below.'}</p>{reference?.source&&<a href={reference.source} target="_blank" rel="noreferrer" className="mt-4 inline-block text-xs text-link underline">District background · Wikipedia, CC BY-SA ↗</a>}
            {reference?.heritageSource&&<a href={reference.heritageSource} target="_blank" rel="noreferrer" className="ml-4 mt-4 inline-block text-xs text-link underline">Heritage highlights source ↗</a>}
            <div className="mt-6 flex flex-wrap gap-3">{places.length>0&&<Link className="rounded-lg bg-primary px-4 py-3 text-sm font-semibold text-white" to={`${routePaths.tourismMap}?district=${encodeURIComponent(district.name)}`}>Explore district on map</Link>}<a href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(district.name+' Bangladesh')}`} target="_blank" rel="noreferrer" className="rounded-lg border border-border px-4 py-3 text-sm font-semibold text-link">Open district map ↗</a></div>
          </div>
        </section>
        {places.length>0&&<section className="mb-9"><h2 className="mb-4 text-xl font-semibold text-heading">What to explore</h2><div className="grid gap-5 sm:grid-cols-2">{places.map(place=><DestinationCard key={place.id} place={place}/>)}</div></section>}
        <div className="grid gap-5 md:grid-cols-2">
          <section className="rounded-xl border border-border bg-surface p-6"><h2 className="text-lg font-semibold">Crafts & community</h2><p className="mt-3 text-sm leading-7 text-body/75">{villages.length?villages.map(v=>v.name+' · '+v.craft).join('; '):'Discover Bangladesh’s making traditions and arrange community visits with local hosts. District-specific workshop availability should be confirmed directly.'}</p><Link to={routePaths.tourismVillages} className="mt-5 inline-block text-sm font-semibold text-link">Explore village guides →</Link></section>
          <section className="rounded-xl border border-border bg-surface p-6"><h2 className="text-lg font-semibold">Taste & travel</h2><p className="mt-3 text-sm leading-7 text-body/75">{cuisines.length?cuisines.map(c=>c.name+': '+c.description).join(' '):'Explore regional food guides and travel resources. Check current opening hours and transport connections before setting out.'}</p><Link to={routePaths.tourismCuisines} className="mt-5 inline-block text-sm font-semibold text-link">Explore local cuisine →</Link></section>
        </div>
      </>}
    </QueryState>
  </div>;
}
