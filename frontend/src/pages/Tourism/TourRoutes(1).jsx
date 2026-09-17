import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useHeritageRoutes } from '../../hooks/useHeritageRoutes';

export default function TourRoutes() {
  const { data, isLoading, isError, error } = useHeritageRoutes({ status: 'Published', pageSize: 20 });
  const [expandedId, setExpandedId] = useState(null);
  const routes = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Tour Routes' },
        ]}
        title="Tour Routes"
        description="Guided multi-day heritage travel routes."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2">
          {routes.map((route) => (
            <div key={route.id} className="rounded-xl border border-border bg-surface p-5">
              <div className="mb-3 flex aspect-video items-center justify-center rounded-lg bg-background text-xs text-body/40">
                Route Map Placeholder
              </div>
              <p className="text-sm font-semibold text-heading">{route.name}</p>
              <p className="mt-1 text-xs text-body/60">
                {Math.round(route.estimatedDurationMinutes / 60)}h · {route.stops.length} stops · {route.totalDistanceKm.toFixed(1)} km
              </p>
              <Button variant="secondary" className="mt-4" onClick={() => setExpandedId(expandedId === route.id ? null : route.id)}>
                {expandedId === route.id ? 'Hide Itinerary' : 'View Itinerary'}
              </Button>
              {expandedId === route.id && (
                <ol className="mt-4 space-y-2 border-t border-border pt-4">
                  {route.stops.map((stop) => (
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
          {routes.length === 0 && <p className="col-span-full text-sm text-body/60">No published tour routes yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
