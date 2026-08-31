import { routePaths } from '../../routes/routePaths';
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
                  <Button variant="secondary" onClick={() => cancel.mutate({ id: booking.id })} disabled={cancel.isPending}>
                    Cancel
                  </Button>
                )}
              </div>
            </div>
          ))}
          {bookings.length === 0 && <p className="p-6 text-center text-sm text-body/60">You have no bookings yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
