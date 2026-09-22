import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useHeritageRoutes } from '../../hooks/useHeritageRoutes';
import SafeImage from '../../components/media/SafeImage';
import TravelEmptyState from '../../components/ui/TravelEmptyState';

export default function TourRoutes() {
  const { data, isLoading, isError, error, refetch } = useHeritageRoutes({ status: 'Published', pageSize: 20 });
  const [search, setSearch] = useState('');
  const [expandedId, setExpandedId] = useState(null);
  const routes = (data?.items || []).filter(route => `${route.name} ${(route.stops || []).map(stop => stop.heritagePlaceName).join(' ')}`.toLowerCase().includes(search.trim().toLowerCase()));

  return (
    <div className="mx-auto max-w-7xl">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Tour Routes' },
        ]}
        title="Tour Routes"
        description="Compare travel time, distance and heritage stops before planning your journey."
      />
      <section className="mb-6 overflow-hidden rounded-2xl border border-border bg-surface md:grid md:grid-cols-[1fr_240px]"><div className="p-6 sm:p-8"><p className="text-xs font-semibold uppercase tracking-widest text-primary">Discover at your own pace</p><h2 className="mt-3 text-2xl font-semibold tracking-tight text-heading">A thoughtful route. A richer journey.</h2><p className="mt-3 max-w-xl text-sm leading-6 text-body/70">Explore published journeys through Bangladesh’s craft communities and cultural landmarks. Open any itinerary to review its stops.</p></div><SafeImage src="/images/bangladesh-river.jpg" alt="Riverside scenery in Bangladesh" className="h-44 w-full object-cover md:h-full" /></section>
      <div className="mb-6 flex flex-wrap items-end justify-between gap-4 rounded-xl border border-border bg-surface p-4"><label className="flex w-full flex-col gap-2 text-xs font-semibold text-heading sm:max-w-sm">Find a route or heritage stop<input type="search" value={search} onChange={event => setSearch(event.target.value)} placeholder="Search published routes" className="rounded-lg border border-border bg-background px-3 py-2.5 text-sm font-normal" /></label><p role="status" className="text-sm text-body/70">{isLoading ? 'Loading routes…' : isError ? 'Routes unavailable' : `${routes.length} routes on this page`}</p></div>
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2">
          {routes.map((route) => (
            <div key={route.id} className="rounded-xl border border-border bg-surface p-5">
              <p className="mb-3 text-xs font-semibold uppercase tracking-widest text-primary">Heritage itinerary</p>
              <p className="text-sm font-semibold text-heading">{route.name}</p>
              <p className="mt-1 text-xs text-body/60">
                {Math.round((route.estimatedDurationMinutes || 0) / 60)}h · {route.stops?.length || 0} stops · {Number(route.totalDistanceKm || 0).toFixed(1)} km
              </p>
              <Button variant="secondary" className="mt-4" aria-expanded={expandedId === route.id} aria-controls={`itinerary-${route.id}`} onClick={() => setExpandedId(expandedId === route.id ? null : route.id)}>
                {expandedId === route.id ? 'Hide Itinerary' : 'View Itinerary'}
              </Button>
              {expandedId === route.id && (
                <ol id={`itinerary-${route.id}`} className="mt-4 space-y-4 border-t border-border pt-4">
                  {[...(route.stops || [])].sort((a,b) => a.order - b.order).map((stop) => (
                    <li key={stop.id} className="text-sm text-body/70">
                      {stop.order}. {stop.heritagePlaceName}
                      {stop.distanceFromPreviousKm != null && (
                        <span className="text-xs text-body/50"> — {stop.distanceFromPreviousKm.toFixed(1)} km from previous</span>
                      )}
                    </li>
                  ))}
                </ol>
              )}
            </div>
          ))}
          {routes.length === 0 && <TravelEmptyState title={search ? 'No routes match your search' : 'Your next journey starts here'} description={search ? 'Try another place name or clear your search.' : 'Tour itineraries have not been published yet. Explore individual heritage places while new journeys are being prepared.'} onReset={search ? () => setSearch('') : undefined} />}
        </div>
      </AsyncState>
      {isError && <Button variant="secondary" className="mt-4" onClick={() => refetch()}>Try again</Button>}
    </div>
  );
}
