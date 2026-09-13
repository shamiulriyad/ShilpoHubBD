import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState, Pagination } from '../../components/ui';
import { useLiveEvents } from '../../hooks/useLiveEvents';
import SafeImage from '../../components/media/SafeImage';

const filters = [
  { label: 'All', status: undefined },
  { label: 'Live', status: 'Live' },
  { label: 'Upcoming', status: 'Scheduled' },
  { label: 'Past', status: 'Ended' },
];

const statusMeta = {
  Live: { label: 'Live now', tone: 'success', action: 'Join live' },
  Scheduled: { label: 'Upcoming', tone: 'secondary', action: 'View details' },
  Ended: { label: 'Ended', tone: 'neutral', action: 'View summary' },
  Cancelled: { label: 'Cancelled', tone: 'neutral', action: null },
};

export default function WorkshopGallery() {
  const [filter, setFilter] = useState(filters[0]);
  const [page, setPage] = useState(1);
  const eventsQuery = useLiveEvents({ status: filter.status, page, pageSize: 12 });
  const events = eventsQuery.data?.items || [];

  const selectFilter = (nextFilter) => {
    setFilter(nextFilter);
    setPage(1);
  };

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Live Shopping' }]}
        title="Live Shopping Events"
        description="Browse real scheduled, active and completed live-commerce events from ShilpoHub producers."
      />

      <div className="mb-8 flex flex-wrap gap-2" role="group" aria-label="Filter live shopping events">
        {filters.map((item) => (
          <button
            key={item.label}
            type="button"
            onClick={() => selectFilter(item)}
            aria-pressed={filter.label === item.label}
            className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              filter.label === item.label
                ? 'border-primary bg-primary text-surface'
                : 'border-border bg-surface text-body hover:bg-background'
            }`}
          >
            {item.label}
          </button>
        ))}
      </div>

      <AsyncState isLoading={eventsQuery.isLoading} isError={eventsQuery.isError} error={eventsQuery.error} loadingText="Loading live shopping events…">
        <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
          {events.map((event) => {
            const meta = statusMeta[event.status] || { label: event.status || 'Unknown', tone: 'neutral', action: 'View details' };
            return (
              <article key={event.id} className="overflow-hidden rounded-xl border border-border bg-surface">
                <div className="aspect-[16/9] overflow-hidden bg-background">
                  {event.productImageUrl ? (
                    <SafeImage src={event.productImageUrl} alt="" className="h-full w-full object-cover" loading="lazy" />
                  ) : (
                    <div className="flex h-full items-center justify-center px-4 text-center text-sm text-body/50">{event.productName}</div>
                  )}
                </div>
                <div className="space-y-3 p-4">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <Badge tone={meta.tone}>{meta.label}</Badge>
                    {event.reactionCount > 0 && <span className="text-xs text-body/50">{event.reactionCount} reactions</span>}
                  </div>
                  <div>
                    <h2 className="text-sm font-semibold text-heading">{event.title}</h2>
                    <p className="mt-1 text-xs text-body/60">
                      {new Date(event.scheduledStartAt).toLocaleString()}
                    </p>
                  </div>
                  <div className="flex flex-wrap gap-x-3 gap-y-1 text-xs">
                    <Link
                      to={routePaths.customerProducerProfile.replace(':producerId', event.producerId)}
                      className="text-link hover:underline"
                    >
                      {event.producerName}
                    </Link>
                    <Link
                      to={routePaths.customerProductDetails.replace(':productId', event.productId)}
                      className="text-link hover:underline"
                    >
                      {event.productName}
                    </Link>
                  </div>
                  {meta.action && (
                    <Link
                      to={routePaths.customerLiveShopping.replace(':workshopId', event.id)}
                      className="inline-flex w-full items-center justify-center rounded-full bg-primary px-4 py-2.5 text-sm font-semibold text-surface transition hover:bg-primary-dark"
                    >
                      {meta.action}
                    </Link>
                  )}
                </div>
              </article>
            );
          })}
        </div>
        {events.length === 0 && (
          <div className="rounded-xl border border-dashed border-border bg-surface p-8 text-center text-sm text-body/60">
            No {filter.label.toLowerCase()} live shopping events are available.
          </div>
        )}
      </AsyncState>

      {(eventsQuery.data?.totalPages || 0) > 1 && (
        <div className="mt-8">
          <Pagination currentPage={page} totalPages={eventsQuery.data.totalPages} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
}
