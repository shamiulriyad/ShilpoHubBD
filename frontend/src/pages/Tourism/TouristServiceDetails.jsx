import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useTouristService, useServiceAvailabilitySlots } from '../../hooks/useTouristServices';
import { useBookingMutations } from '../../hooks/useBookings';
import { useAuth } from '../../hooks/useAuth';

export default function TouristServiceDetails() {
  const { serviceId } = useParams();
  const { isAuthenticated } = useAuth();
  const serviceQuery = useTouristService(serviceId);
  const slotsQuery = useServiceAvailabilitySlots(serviceId, { onlyAvailable: true, pageSize: 20 });
  const { create } = useBookingMutations();
  const [selectedSlotId, setSelectedSlotId] = useState(null);
  const [partySize, setPartySize] = useState(1);

  const service = serviceQuery.data;
  const slots = slotsQuery.data?.items || [];

  const handleBook = () => {
    create.mutate(
      { serviceId, availabilitySlotId: selectedSlotId, partySize },
      { onSuccess: () => setSelectedSlotId(null) },
    );
  };

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={serviceQuery.isLoading} isError={serviceQuery.isError} error={serviceQuery.error}>
        {service && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Tourism', path: routePaths.tourism },
                { label: 'Tourist Services', path: routePaths.tourismServices },
                { label: service.title },
              ]}
              title={service.title}
              description={`${service.type} · By ${service.producerName} · ${service.districtName}`}
            />

            <div className="grid gap-8 lg:grid-cols-2">
              <div>
                <div className="flex aspect-video items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
                  {service.imageUrl ? (
                    <img src={service.imageUrl} alt={service.title} className="h-full w-full rounded-2xl object-cover" />
                  ) : (
                    'Service Photo'
                  )}
                </div>
                <p className="mt-4 text-sm text-body/70">{service.description}</p>
                {service.averageRating > 0 && (
                  <p className="mt-2 text-sm text-secondary">★ {service.averageRating.toFixed(1)} ({service.reviewCount} reviews)</p>
                )}
              </div>

              <div className="space-y-4 rounded-xl border border-border bg-surface p-5">
                <p className="text-2xl font-semibold text-primary">৳ {service.price.toLocaleString()}</p>
                <p className="text-sm font-semibold text-heading">Choose a time slot</p>
                <div className="max-h-64 space-y-2 overflow-y-auto">
                  {slots.map((slot) => (
                    <button
                      key={slot.id}
                      type="button"
                      onClick={() => setSelectedSlotId(slot.id)}
                      className={`block w-full rounded-lg border px-3 py-2 text-left text-sm ${
                        selectedSlotId === slot.id ? 'border-primary bg-primary/5 text-primary' : 'border-border bg-background text-body'
                      }`}
                    >
                      {new Date(slot.startAt).toLocaleString()} · {slot.remainingCapacity} spot{slot.remainingCapacity !== 1 ? 's' : ''} left
                    </button>
                  ))}
                  {slots.length === 0 && <p className="text-sm text-body/60">No available time slots right now.</p>}
                </div>

                <div>
                  <label className="mb-1.5 block text-sm font-medium text-body/70">Party size</label>
                  <input
                    type="number"
                    min={1}
                    value={partySize}
                    onChange={(event) => setPartySize(Math.max(1, Number(event.target.value)))}
                    className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
                  />
                </div>

                {isAuthenticated ? (
                  <Button
                    variant="primary"
                    className="w-full"
                    disabled={!selectedSlotId || create.isPending}
                    onClick={handleBook}
                  >
                    {create.isPending ? 'Booking…' : 'Book Now'}
                  </Button>
                ) : (
                  <p className="text-xs text-body/50">
                    <Link to={routePaths.login} className="text-link hover:underline">Log in</Link> to book this service.
                  </p>
                )}
              </div>
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
