import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useHeritagePlaces } from '../../hooks/useHeritagePlaces';

export default function HeritageMap() {
  const [districtId, setDistrictId] = useState(null);
  const [search, setSearch] = useState('');
  const districtsQuery = useDistricts();
  const placesQuery = useHeritagePlaces({ districtId: districtId || undefined, pageSize: 20 });
  const places = useMemo(() => (placesQuery.data?.items || []).filter(place => !search.trim() || `${place.name} ${place.description} ${place.districtName}`.toLowerCase().includes(search.trim().toLowerCase())), [placesQuery.data, search]);
  const point = (place) => ({ left: `${Math.max(4, Math.min(96, ((place.longitude - 88) / 5) * 100))}%`, top: `${Math.max(4, Math.min(96, ((26.8 - place.latitude) / 6.3) * 100))}%` });

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Heritage Map' },
        ]}
        title="Heritage Map"
        description="Explore heritage sites, villages and events across Bangladesh."
      />
      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-3">
          <label className="block text-sm font-medium">Search places<input type="search" value={search} onChange={event => setSearch(event.target.value)} placeholder="Name, district or description" className="mt-2 block min-h-11 w-full rounded-lg border border-border bg-surface px-4" /></label>
          <div className="relative aspect-[16/10] overflow-hidden rounded-2xl border border-border bg-gradient-to-br from-sky-50 via-amber-50 to-emerald-50" role="img" aria-label={`Map visualization with ${places.length} heritage places`}><div className="absolute inset-[7%_20%] rounded-[45%_55%_62%_38%/38%_42%_58%_62%] border-2 border-emerald-200 bg-emerald-100/70 shadow-inner" />{places.filter(place => Number.isFinite(place.latitude) && Number.isFinite(place.longitude)).map(place => <Link key={place.id} title={`${place.name}, ${place.districtName}`} aria-label={`Open ${place.name} details`} to={routePaths.tourismPlaceDetails.replace(':placeId',place.id)} style={point(place)} className="absolute z-10 -translate-x-1/2 -translate-y-1/2 rounded-full border-2 border-white bg-primary p-2 shadow-lg transition hover:scale-125 focus:outline focus:outline-2 focus:outline-primary"><span className="sr-only">{place.name}</span></Link>)}</div>
          <AsyncState isLoading={placesQuery.isLoading} isError={placesQuery.isError} error={placesQuery.error}>
            <div className="grid gap-3 sm:grid-cols-2">
              {places.map((place) => (
                <Link to={routePaths.tourismPlaceDetails.replace(':placeId', place.id)} key={place.id} className="rounded-xl border border-border bg-surface p-4 transition hover:-translate-y-0.5 hover:shadow-md">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-semibold text-heading">{place.name}</p>
                    <Badge tone="secondary">{place.placeType}</Badge>
                  </div>
                  <p className="mt-1 text-xs text-body/60">{place.districtName}</p>
                  {place.averageRating > 0 && (
                    <p className="mt-1 text-xs text-secondary">★ {place.averageRating.toFixed(1)} ({place.reviewCount})</p>
                  )}
                  <p className="mt-3 text-sm font-medium text-primary">View place details →</p>
                </Link>
              ))}
              {places.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No heritage places found for this district.</p>
              )}
            </div>
          </AsyncState>
        </div>
        <div className="space-y-2">
          <p className="mb-2 text-sm font-semibold text-heading">Districts</p>
          <button
            type="button"
            onClick={() => setDistrictId(null)}
            className={`block w-full rounded-lg border px-3 py-2 text-left text-sm ${
              !districtId ? 'border-primary text-primary' : 'border-border bg-surface text-body hover:border-primary hover:text-primary'
            }`}
          >
            All Districts
          </button>
          {(districtsQuery.data || []).map((district) => (
            <button
              key={district.id}
              type="button"
              onClick={() => setDistrictId(district.id)}
              className={`block w-full rounded-lg border px-3 py-2 text-left text-sm ${
                districtId === district.id ? 'border-primary text-primary' : 'border-border bg-surface text-body hover:border-primary hover:text-primary'
              }`}
            >
              {district.name}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
