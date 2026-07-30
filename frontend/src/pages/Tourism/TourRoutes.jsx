import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { tourRoutes } from '../../data/mockData';

export default function TourRoutes() {
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
      <div className="grid gap-4 sm:grid-cols-2">
        {tourRoutes.map((route) => (
          <div key={route.id} className="rounded-xl border border-border bg-surface p-5">
            <div className="mb-3 flex aspect-video items-center justify-center rounded-lg bg-background text-xs text-body/40">
              Route Map Placeholder
            </div>
            <p className="text-sm font-semibold text-heading">{route.name}</p>
            <p className="mt-1 text-xs text-body/60">
              {route.duration} · {route.stops} stops
            </p>
            <Button variant="secondary" className="mt-4">
              View Itinerary
            </Button>
          </div>
        ))}
      </div>
    </div>
  );
}
