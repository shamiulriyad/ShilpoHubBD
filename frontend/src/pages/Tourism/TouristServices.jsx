import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, CategoryFilter, AsyncState } from '../../components/ui';
import { useTouristServices } from '../../hooks/useTouristServices';

const types = ['GuideBooking', 'WorkshopBooking', 'ArtisanHomeVisit', 'HomestayBooking', 'TransportationBooking'];
const typeLabels = {
  GuideBooking: 'Local Guide',
  WorkshopBooking: 'Craft Workshop',
  ArtisanHomeVisit: 'Artisan Home Visit',
  HomestayBooking: 'Homestay',
  TransportationBooking: 'Transportation',
};

export default function TouristServices() {
  const [type, setType] = useState(null);
  const { data, isLoading, isError, error } = useTouristServices({ type: type || undefined, pageSize: 24 });
  const services = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Tourist Services' },
        ]}
        title="Tourist Services"
        description="Book guides, workshops, homestays and transportation from local producers."
      />

      <CategoryFilter
        className="mb-6"
        options={[{ id: null, name: 'All' }, ...types.map((t) => ({ id: t, name: typeLabels[t] }))]}
        active={type}
        onChange={setType}
      />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {services.map((service) => (
            <Link
              key={service.id}
              to={routePaths.tourismServiceDetails.replace(':serviceId', service.id)}
              className="flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
            >
              <div className="flex aspect-[4/3] items-center justify-center bg-background text-xs text-body/40">
                {service.imageUrl ? (
                  <img src={service.imageUrl} alt={service.title} className="h-full w-full object-cover" />
                ) : (
                  'Service Photo'
                )}
              </div>
              <div className="space-y-1.5 p-4">
                <Badge tone="secondary">{typeLabels[service.type] || service.type}</Badge>
                <p className="text-sm font-semibold text-heading">{service.title}</p>
                <p className="text-xs text-body/60">By {service.producerName} · {service.districtName}</p>
                <p className="text-sm font-semibold text-primary">৳ {service.price.toLocaleString()}</p>
              </div>
            </Link>
          ))}
          {services.length === 0 && <p className="col-span-full text-sm text-body/60">No tourist services available yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
