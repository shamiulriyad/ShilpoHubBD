import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useHeritagePlaces } from '../../hooks/useHeritagePlaces';

export default function HeritageMap() {
  const [districtId, setDistrictId] = useState(null);
  const districtsQuery = useDistricts();
  const placesQuery = useHeritagePlaces({ districtId: districtId || undefined, pageSize: 20 });

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
          <div className="flex aspect-[16/10] items-center justify-center rounded-2xl border border-dashed border-border bg-surface text-sm text-body/40">
            Interactive Bangladesh Map Placeholder
          </div>
          <AsyncState isLoading={placesQuery.isLoading} isError={placesQuery.isError} error={placesQuery.error}>
            <div className="grid gap-3 sm:grid-cols-2">
              {placesQuery.data?.items.map((place) => (
                <div key={place.id} className="rounded-xl border border-border bg-surface p-4">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-semibold text-heading">{place.name}</p>
                    <Badge tone="secondary">{place.placeType}</Badge>
                  </div>
                  <p className="mt-1 text-xs text-body/60">{place.districtName}</p>
                  {place.averageRating > 0 && (
                    <p className="mt-1 text-xs text-secondary">★ {place.averageRating.toFixed(1)} ({place.reviewCount})</p>
                  )}
                </div>
              ))}
              {placesQuery.data?.items.length === 0 && (
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
