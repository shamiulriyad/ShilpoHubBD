import { useSearchParams, Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { districtReference } from '../../data/tourismGuides';
import { DirectoryFilters, TravelPhoto, EmptyResults } from '../../components/tourism/TravelUI';

export default function Districts() {
  const query=useDistricts();
  const [params,setParams]=useSearchParams();
  const search=params.get('q')||'',division=params.get('division')||'';
  const change=(key,value)=>{const next=new URLSearchParams(params);if(value)next.set(key,value);else next.delete(key);setParams(next,{replace:true});};
  const records=(query.data||[]).map(d=>({...d,reference:districtReference(d.name)}));
  const filtered=records.filter(d=>(!division||d.division===division)&&[d.name,d.division,d.reference?.knownFor,d.reference?.description].join(' ').toLowerCase().includes(search.toLowerCase().trim()));
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Explore Bangladesh by district" description="Discover regional identity, historic places and landscapes. Start with a district, then explore its story." breadcrumbs={[{label:'Home',path:'/'},{label:'Explore',path:routePaths.explore},{label:'Districts'}]}/>
    <DirectoryFilters search={search} onSearch={v=>change('q',v)} filters={[{label:'Division',value:division,onChange:v=>change('division',v),options:[...new Set(records.map(d=>d.division).filter(Boolean))].sort()}]} onClear={()=>setParams({})}/>
    <QueryState query={query} emptyLabel="District information is not available.">
      {()=> <><p role="status" className="mb-4 text-sm text-body/65">{filtered.length} districts</p><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{filtered.map(d=><article key={d.id} className="flex flex-col overflow-hidden rounded-xl border border-border bg-surface"><TravelPhoto image={d.reference?.image}/><div className="flex flex-1 flex-col p-5"><p className="text-xs font-semibold uppercase tracking-wide text-primary">{d.division}</p><h2 className="mt-2 text-xl font-semibold text-heading"><Link to={routePaths.exploreDistrictDetails.replace(':districtId',d.id)} className="hover:text-primary">{d.name}</Link></h2><p className="mt-3 line-clamp-3 text-sm leading-6 text-body/75">{d.reference?.knownFor || d.reference?.description || 'Explore the district’s places and local identity.'}</p><Link to={routePaths.exploreDistrictDetails.replace(':districtId',d.id)} className="mt-auto pt-5 text-sm font-semibold text-link">Discover {d.name} →</Link></div></article>)}</div>{!filtered.length&&<EmptyResults onClear={()=>setParams({})}/>}</>}
    </QueryState>
  </div>;
}
