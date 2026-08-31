import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button } from '../../components/ui';
import { VideoPlayer } from '../../components/media';

const filters = ['All', 'Live', 'Upcoming', 'Past'];
const statusLabel = { live: 'Live Now', upcoming: 'Upcoming', past: 'Replay' };
const statusTone = { live: 'success', upcoming: 'secondary', past: 'neutral' };

// TODO(backend): only per-producer workshop galleries exist
// (`GET /api/producers/{id}/workshop-gallery`) — there is no cross-producer
// workshop feed yet. Placeholder entries until one exists.
const workshops = [
  { id: 'workshop-1', title: 'Live Jamdani Loom Session', producerId: 'producer-1', producer: 'Rahima Begum', craft: 'Jamdani Weaving', status: 'live', scheduledFor: '2026-09-10', viewers: 128 },
  { id: 'workshop-2', title: 'Nakshi Kantha Stitch Circle', producerId: 'producer-2', producer: 'Abdul Karim', craft: 'Nakshi Kantha', status: 'upcoming', scheduledFor: '2026-09-14', viewers: null },
  { id: 'workshop-3', title: 'Terracotta Throwing Demo', producerId: 'producer-3', producer: 'Shefali Rani', craft: 'Terracotta Art', status: 'upcoming', scheduledFor: '2026-09-18', viewers: null },
  { id: 'workshop-4', title: 'Bamboo Weaving Basics', producerId: 'producer-4', producer: 'Motiur Rahman', craft: 'Bamboo Work', status: 'past', scheduledFor: '2026-08-20', viewers: null },
];

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
            <VideoPlayer
              title={workshop.title}
              live={workshop.status === 'live'}
              viewers={workshop.viewers}
              bordered={false}
            />
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
