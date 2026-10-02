import { routePaths } from '../../routes/routePaths';
import { useState } from 'react';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { PageHeader, Badge, Button, AsyncState, PageNavigation } from '../../components/ui';
import { useMyBookings, useBookingMutations } from '../../hooks/useBookings';

const statusTone = {
  Pending: 'secondary',
  Confirmed: 'primary',
  Completed: 'success',
  Rejected: 'neutral',
  Cancelled: 'neutral',
  NoShow: 'neutral',
};

export default function MyBookings() {
  const [cancelId, setCancelId] = useState(null);
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useMyBookings({ page, pageSize: 10 });
  const { cancel } = useBookingMutations();
  const bookings = data?.items || [];
  const upcomingCount = bookings.filter((booking) => ['Pending', 'Confirmed'].includes(booking.status)).length;
  const savings = bookings.reduce((total, booking) => total + Number(booking.discountAmount || 0), 0);

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'My Bookings' },
        ]}
        title="My Bookings"
        description="Guides, workshops, homestays and transportation you've booked."
      />

      {!isLoading && !isError && bookings.length > 0 && <div className="mb-6 grid gap-3 sm:grid-cols-3">
        <div className="rounded-xl border border-border bg-surface p-4"><p className="text-xs font-semibold uppercase tracking-wide text-body/55">Total bookings</p><p className="mt-2 text-2xl font-bold text-heading">{data?.totalCount ?? bookings.length}</p></div>
        <div className="rounded-xl border border-border bg-surface p-4"><p className="text-xs font-semibold uppercase tracking-wide text-body/55">Upcoming</p><p className="mt-2 text-2xl font-bold text-heading">{upcomingCount}</p></div>
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4"><p className="text-xs font-semibold uppercase tracking-wide text-emerald-800/70">Passport savings</p><p className="mt-2 text-2xl font-bold text-emerald-800">৳ {savings.toLocaleString('en-BD')}</p></div>
      </div>}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {bookings.map((booking) => (
            <article key={booking.id} className="flex flex-wrap items-center justify-between gap-5 rounded-xl border border-border bg-surface p-5 shadow-sm transition hover:border-primary/40">
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2"><p className="font-semibold text-heading">{booking.serviceTitle}</p><Badge tone={statusTone[booking.status] || 'neutral'}>{booking.status}</Badge></div>
                <p className="mt-2 text-xs text-body/60">
                  {new Date(booking.slotStartAt).toLocaleString()} · Party of {booking.partySize}
                </p>
                <p className="mt-1 text-xs text-body/55">Hosted by {booking.producerName} · {booking.serviceType.replace('Booking', '')}</p>
              </div>
              <div className="flex items-center gap-4">
                <div className="text-right">
                  {booking.discountAmount > 0 && <><p className="text-xs text-body/45 line-through">৳ {booking.basePrice.toLocaleString('en-BD')}</p><p className="text-xs font-semibold text-success">Saved ৳ {booking.discountAmount.toLocaleString('en-BD')} · {booking.discountPercent}%</p></>}
                  <p className="text-lg font-bold text-primary">৳ {booking.totalPrice.toLocaleString('en-BD')}</p>
                </div>
                {['Pending', 'Confirmed'].includes(booking.status) && (
                  <Button variant="secondary" onClick={() => setCancelId(booking.id)} disabled={cancel.isPending}>
                    Cancel booking
                  </Button>
                )}
              </div>
            </article>
          ))}
          {bookings.length === 0 && <TravelEmptyState title="No bookings yet" description="Explore heritage places and local experiences to plan your first trip. Your reservations will appear here." />}
        </div>
      </AsyncState>
      {!isLoading && !isError && <PageNavigation data={data} page={page} onPageChange={setPage} />}
      {cancelId && <section aria-label="Confirm booking cancellation" className="mt-4 rounded-xl border border-border bg-surface p-5"><h2 className="font-semibold">Cancel {bookings.find(booking => booking.id === cancelId)?.serviceTitle || 'this booking'}?</h2><p className="mt-2 text-sm text-body/70">Cancellation cannot be undone. You may need to make a new booking.</p><div className="mt-4 flex gap-3"><Button variant="secondary" disabled={cancel.isPending} onClick={() => setCancelId(null)}>Keep booking</Button><Button disabled={cancel.isPending} onClick={() => cancel.mutate({ id: cancelId }, { onSuccess: () => setCancelId(null) })}>{cancel.isPending ? 'Cancelling…' : 'Confirm cancellation'}</Button></div></section>}
      <div className="mt-4"><MutationFeedback mutation={cancel} /></div>
    </div>
  );
}
