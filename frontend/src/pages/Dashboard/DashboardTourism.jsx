import { PageHeader, AsyncState } from '../../components/ui';
import { FestivalCard } from '../../components/cards';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useHeritageRoutes } from '../../hooks/useHeritageRoutes';

const listOf = (data) => data?.items || data || [];

const routeDuration = (minutes) => {
  if (!minutes) return '—';
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return h ? `${h}h${m ? ` ${m}m` : ''}` : `${m}m`;
};

export default function DashboardTourism() {
  const festivalsQuery = useHeritageFestivals({ pageSize: 3 });
  const routesQuery = useHeritageRoutes({ pageSize: 4 });

  const festivals = listOf(festivalsQuery.data);
  const routes = listOf(routesQuery.data);

  return (
    <div>
      <PageHeader title="Tourism" description="Saved festivals, events and travel routes." />

      <AsyncState isLoading={festivalsQuery.isLoading} isError={festivalsQuery.isError} error={festivalsQuery.error}>
        <div className="mb-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {festivals.map((festival) => (
            <FestivalCard
              key={festival.id}
              festival={{ name: festival.name, date: festival.startDate, district: festival.districtName }}
            />
          ))}
        </div>
      </AsyncState>

      <AsyncState isLoading={routesQuery.isLoading} isError={routesQuery.isError} error={routesQuery.error}>
        <div className="grid gap-4 sm:grid-cols-2">
          {routes.map((route) => (
            <div key={route.id} className="rounded-xl border border-border bg-surface p-4">
              <p className="text-sm font-semibold text-heading">{route.name}</p>
              <p className="mt-1 text-xs text-body/60">
                {routeDuration(route.estimatedDurationMinutes)} · {route.stops?.length ?? 0} stops
              </p>
            </div>
          ))}
        </div>
      </AsyncState>
    </div>
  );
}
