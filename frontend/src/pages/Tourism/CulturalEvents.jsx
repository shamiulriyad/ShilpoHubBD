import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { culturalEvents } from '../../data/mockData';

export default function CulturalEvents() {
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
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {culturalEvents.map((event) => (
          <div key={event.id} className="flex items-center justify-between gap-4 p-4">
            <div>
              <p className="text-sm font-semibold text-heading">{event.name}</p>
              <p className="mt-1 text-xs text-body/60">{event.venue}</p>
            </div>
            <p className="shrink-0 text-xs font-medium text-primary">{event.date}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
