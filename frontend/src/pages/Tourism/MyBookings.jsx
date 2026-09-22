import { routePaths } from '../../routes/routePaths';
import { useState } from 'react';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
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
  const { data, isLoading, isError, error } = useMyBookings({ pageSize: 50 });
  const { cancel } = useBookingMutations();
  const bookings = data?.items || [];

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

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {bookings.map((booking) => (
            <div key={booking.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{booking.serviceTitle}</p>
                <p className="text-xs text-body/60">
                  {new Date(booking.slotStartAt).toLocaleString()} · Party of {booking.partySize}
                </p>
              </div>
              <div className="flex items-center gap-3">
                <p className="text-sm font-semibold text-primary">৳ {booking.totalPrice.toLocaleString()}</p>
                <Badge tone={statusTone[booking.status] || 'neutral'}>{booking.status}</Badge>
                {['Pending', 'Confirmed'].includes(booking.status) && (
                  <Button variant="secondary" onClick={() => setCancelId(booking.id)} disabled={cancel.isPending}>
                    Cancel booking
                  </Button>
                )}
              </div>
            </div>
          ))}
          {bookings.length === 0 && <TravelEmptyState title="No bookings yet" description="Explore heritage places and local experiences to plan your first trip. Your reservations will appear here." />}
        </div>
      </AsyncState>
      {cancelId && <section aria-label="Confirm booking cancellation" className="mt-4 rounded-xl border border-border bg-surface p-5"><h2 className="font-semibold">Cancel {bookings.find(booking => booking.id === cancelId)?.serviceTitle || 'this booking'}?</h2><p className="mt-2 text-sm text-body/70">Cancellation cannot be undone. You may need to make a new booking.</p><div className="mt-4 flex gap-3"><Button variant="secondary" disabled={cancel.isPending} onClick={() => setCancelId(null)}>Keep booking</Button><Button disabled={cancel.isPending} onClick={() => cancel.mutate({ id: cancelId }, { onSuccess: () => setCancelId(null) })}>{cancel.isPending ? 'Cancelling…' : 'Confirm cancellation'}</Button></div></section>}
      <div className="mt-4"><MutationFeedback mutation={cancel} /></div>
    </div>
  );
}
