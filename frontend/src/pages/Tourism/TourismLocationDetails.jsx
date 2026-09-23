import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import LocationMedia from '../../components/tourism/LocationMedia';
import { useTourismLocation } from '../../hooks/useTourismLocations';
import { coordinatesNote, verificationBadge } from '../../utils/tourismLocation';

export default function TourismLocationDetails() {
  const { locationId } = useParams();
  const query = useTourismLocation(locationId);
  const location = query.data;

  return (
    <main className="mx-auto max-w-6xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
        {location && (
          <>
            <PageHeader
              title={location.name}
              description={`${location.type} · ${location.districtName}`}
              breadcrumbs={[
                { label: 'Tourism', path: routePaths.tourism },
                { label: 'AI Trip Planner', path: routePaths.tourismAiPlanner },
                { label: location.name },
              ]}
            />
            <div className="grid gap-8 lg:grid-cols-[1.4fr_1fr]">
              <div className="overflow-hidden rounded-2xl border border-border">
                <LocationMedia location={location} className="h-72 lg:h-full lg:min-h-[24rem]" />
              </div>
              <article className="rounded-2xl border border-border bg-surface p-6">
                <div className="flex flex-wrap gap-2">
                  <Badge tone="secondary">{location.type}</Badge>
                  <Badge tone={verificationBadge(location).tone}>{verificationBadge(location).label}</Badge>
                </div>
                <h2 className="mt-5 text-xl font-semibold">About this location</h2>
                <p className="mt-3 whitespace-pre-line leading-7 text-body/80">
                  {location.description || 'Details will be published by ShilpoHub administrators.'}
                </p>
                <dl className="mt-6 space-y-4 border-t border-border pt-5 text-sm">
                  <div>
                    <dt className="font-semibold">District</dt>
                    <dd>{location.districtName}</dd>
                  </div>
                  {location.address && (
                    <div>
                      <dt className="font-semibold">Address</dt>
                      <dd>{location.address}</dd>
                    </div>
                  )}
                  {location.price != null && (
                    <div>
                      <dt className="font-semibold">Price</dt>
                      <dd>৳ {Number(location.price).toLocaleString('en-BD')} / night</dd>
                    </div>
                  )}
                  {location.entryFee != null && (
                    <div>
                      <dt className="font-semibold">Entry fee</dt>
                      <dd>৳ {Number(location.entryFee).toLocaleString('en-BD')}</dd>
                    </div>
                  )}
                  {location.openingHours && (
                    <div>
                      <dt className="font-semibold">Opening hours</dt>
                      <dd>{location.openingHours}</dd>
                    </div>
                  )}
                  {location.facilities && (
                    <div>
                      <dt className="font-semibold">Facilities</dt>
                      <dd>{location.facilities}</dd>
                    </div>
                  )}
                  {location.contactInfo && (
                    <div>
                      <dt className="font-semibold">Contact</dt>
                      <dd>{location.contactInfo}</dd>
                    </div>
                  )}
                  <div>
                    <dt className="font-semibold">Location</dt>
                    <dd>{location.latitude.toFixed(5)}, {location.longitude.toFixed(5)}</dd>
                    {coordinatesNote(location) && <dd className="text-xs text-body/60">{coordinatesNote(location)}</dd>}
                  </div>
                  {location.unverifiedFields && (
                    <div>
                      <dt className="font-semibold">Not verified</dt>
                      <dd className="text-body/70">{location.unverifiedFields}</dd>
                    </div>
                  )}
                  {location.sourceUrl?.startsWith('http') && (
                    <div>
                      <dt className="font-semibold">Source</dt>
                      <dd>
                        <a href={location.sourceUrl} target="_blank" rel="noopener noreferrer" className="break-all text-primary underline">
                          {location.sourceUrl}
                        </a>
                        {location.dataRetrievedOn && (
                          <span className="block text-xs text-body/60">Retrieved {new Date(location.dataRetrievedOn).toLocaleDateString()}</span>
                        )}
                      </dd>
                    </div>
                  )}
                </dl>
              </article>
            </div>
          </>
        )}
      </AsyncState>
    </main>
  );
}
