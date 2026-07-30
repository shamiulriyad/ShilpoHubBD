import { PageHeader } from '../../components/ui';
import { FestivalCard } from '../../components/cards';
import { festivals, tourRoutes } from '../../data/mockData';

export default function DashboardTourism() {
  return (
    <div>
      <PageHeader title="Tourism" description="Saved festivals, events and travel routes." />
      <div className="mb-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {festivals.slice(0, 3).map((festival) => (
          <FestivalCard key={festival.id} festival={festival} />
        ))}
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        {tourRoutes.map((route) => (
          <div key={route.id} className="rounded-xl border border-border bg-surface p-4">
            <p className="text-sm font-semibold text-heading">{route.name}</p>
            <p className="mt-1 text-xs text-body/60">
              {route.duration} · {route.stops} stops
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
