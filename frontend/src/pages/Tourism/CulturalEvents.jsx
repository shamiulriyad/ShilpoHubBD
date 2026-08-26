import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { useCulturalEvents } from '../../hooks/useCulturalEvents';

export default function CulturalEvents() {
  const { data, isLoading, isError, error } = useCulturalEvents({ pageSize: 50 });
  const events = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Cultural Events' },
        ]}
        title="Cultural Events"
        description="Upcoming events, workshops and performances."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {events.map((event) => (
            <div key={event.id} className="flex items-center justify-between gap-4 p-4">
              <div>
                <p className="text-sm font-semibold text-heading">{event.name}</p>
                <p className="mt-1 text-xs text-body/60">
                  {event.heritagePlaceName || event.districtName}
                </p>
              </div>
              <p className="shrink-0 text-xs font-medium text-primary">{new Date(event.eventDate).toLocaleDateString()}</p>
            </div>
          ))}
          {events.length === 0 && <p className="p-6 text-center text-sm text-body/60">No cultural events scheduled right now.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
