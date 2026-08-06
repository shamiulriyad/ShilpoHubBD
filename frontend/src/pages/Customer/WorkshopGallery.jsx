import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button } from '../../components/ui';
import { workshops } from '../../data/mockData';

const filters = ['All', 'Live', 'Upcoming', 'Past'];
const statusLabel = { live: 'Live Now', upcoming: 'Upcoming', past: 'Replay' };
const statusTone = { live: 'success', upcoming: 'secondary', past: 'neutral' };

export default function WorkshopGallery() {
  const [filter, setFilter] = useState('All');
  const visible = workshops.filter((w) => filter === 'All' || w.status === filter.toLowerCase());

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Workshops' }]}
        title="Live Workshop Commerce"
        description="Watch artisans at work and shop directly from live and recorded workshop streams."
      />

      <div className="mb-8 flex flex-wrap gap-2">
        {filters.map((item) => (
          <button
            key={item}
            type="button"
            onClick={() => setFilter(item)}
            className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              filter === item ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-body hover:bg-background'
            }`}
          >
            {item}
          </button>
        ))}
      </div>

      <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {visible.map((workshop) => (
          <div key={workshop.id} className="overflow-hidden rounded-xl border border-border bg-surface">
            <div className="flex aspect-video items-center justify-center bg-background text-xs text-body/40">
              Workshop Stream
            </div>
            <div className="space-y-2 p-4">
              <Badge tone={statusTone[workshop.status]}>
                {statusLabel[workshop.status]}
                {workshop.status === 'live' && workshop.viewers ? ` · ${workshop.viewers} watching` : ''}
              </Badge>
              <p className="text-sm font-semibold text-heading">{workshop.title}</p>
              <p className="text-xs text-body/60">
                {workshop.craft} · {workshop.scheduledFor}
              </p>
              <Link
                to={routePaths.customerProducerProfile.replace(':producerId', workshop.producerId)}
                className="block text-xs text-link hover:underline"
              >
                by {workshop.producer}
              </Link>
              {workshop.status === 'upcoming' ? (
                <Button variant="secondary" className="mt-2 w-full">
                  Set Reminder
                </Button>
              ) : (
                <Link to={routePaths.customerLiveShopping.replace(':workshopId', workshop.id)}>
                  <Button variant={workshop.status === 'live' ? 'primary' : 'secondary'} className="mt-2 w-full">
                    {workshop.status === 'live' ? 'Join Live' : 'Watch Replay'}
                  </Button>
                </Link>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
