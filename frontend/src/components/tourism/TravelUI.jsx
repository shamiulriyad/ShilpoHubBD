import { Link } from 'react-router-dom';
import SafeImage from '../media/SafeImage';
import { mapLink } from '../../utils/tourismAdapters';

export const fieldClass = 'mt-2 block min-h-11 w-full rounded-lg border border-border bg-background px-3 py-2 text-sm font-normal text-heading focus:outline-primary';
export function DirectoryFilters({ search, onSearch, filters = [], onClear, placeholder = 'Search names, places or interests…' }) {
  return <section aria-label="Search and filters" className="mb-6 rounded-xl border border-border bg-surface p-5">
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
      <label className="text-sm font-medium text-heading">Search<input type="search" placeholder={placeholder} value={search} onChange={e=>onSearch(e.target.value)} className={fieldClass}/></label>
      {filters.map(f=><label key={f.label} className="text-sm font-medium text-heading">{f.label}<select value={f.value} onChange={e=>f.onChange(e.target.value)} className={fieldClass}><option value="">{f.all || 'All '+({District:'districts',Experience:'experiences',Division:'divisions',Craft:'crafts',Category:'categories'}[f.label] || f.label.toLowerCase())}</option>{f.options.map(option=>{const [value,label]=Array.isArray(option)?option:[option,option];return <option key={value} value={value}>{label}</option>;})}</select></label>)}
    </div>
    {onClear && <button type="button" onClick={onClear} className="mt-4 text-sm font-semibold text-primary underline underline-offset-4">Reset filters</button>}
  </section>;
}
function PhotoCaption({ image }) {
  // Credit is stored with the record as "Photo: author · licence · source URL".
  if (image.credit) {
    const parts = image.credit.split(' · ');
    const last = parts[parts.length - 1];
    const isUrl = /^https?:\/\//.test(last);
    return <figcaption className="px-4 py-2 text-[10px] leading-4 text-body/65">{(isUrl ? parts.slice(0, -1) : parts).join(' · ')}{isUrl && <> · <a href={last} target="_blank" rel="noreferrer" className="underline">Source</a></>}</figcaption>;
  }
  return null;
}
export function TravelPhoto({ image, className = 'aspect-[16/9] w-full', eager = false }) {
  if (!image?.url) return null;
  return <figure><SafeImage src={image.url} alt={image.alt} loading={eager?'eager':'lazy'} className={className+' object-cover'} fallbackLabel="Photograph could not load"/><PhotoCaption image={image}/></figure>;
}
export function DestinationCard({ place }) {
  return <article className="flex flex-col overflow-hidden rounded-xl border border-border bg-surface">
    <TravelPhoto image={place.image}/>
    <div className="flex flex-1 flex-col p-5"><p className="text-xs font-semibold uppercase tracking-wide text-primary">{place.districtName} · {place.placeType}</p><h3 className="mt-2 text-xl font-semibold text-heading">{place.name}</h3><p className="mt-3 text-sm leading-6 text-body/75">{place.knownFor || place.description}</p><Link to={mapLink(place)} className="mt-auto pt-5 text-sm font-semibold text-link">Explore on map →</Link></div>
  </article>;
}
export function EmptyResults({ onClear }) {
  return <div className="rounded-xl border border-border bg-surface p-8 text-center"><h2 className="text-lg font-semibold">No matching results</h2><p className="mt-2 text-sm text-body/70">Try a different name, district or category.</p><button onClick={onClear} className="mt-4 text-sm font-semibold text-primary underline">Reset filters</button></div>;
}
export function LiveDataNotice({ query, label }) {
  if (query.isError) return <p role="alert" className="mb-5 rounded-lg border border-border bg-surface p-4 text-sm">{label} could not load. <button className="font-semibold text-link underline" onClick={()=>query.refetch()}>Retry</button></p>;
  if (query.isLoading) return <p role="status" className="mb-4 text-sm text-body/60">Checking {label.toLowerCase()}…</p>;
  return null;
}
