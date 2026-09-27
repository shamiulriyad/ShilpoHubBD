import { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader, QueryStatusBanner } from '../../components/ui';
import HeritageLeafletMap from '../../components/tourism/HeritageLeafletMap';
import SafeImage from '../../components/media/SafeImage';
import LocationMedia from '../../components/tourism/LocationMedia';
import { useDistricts } from '../../hooks/useDistricts';
import { useSavedTourPlan, useTourPlan } from '../../hooks/useAITourism';
import { useTouristServices } from '../../hooks/useTouristServices';
import { ACCOMMODATION_TYPES, POI_GROUPS, useNearbyAccommodations, useTourismLocations, useTourismPois } from '../../hooks/useTourismLocations';
import { coordinatesNote, verificationBadge } from '../../utils/tourismLocation';
import { useMessagingMutations } from '../../hooks/useMessaging';

const TRANSPORT_MODES = [
  { value: 'Bus', label: 'Bus / Road' },
  { value: 'Train', label: 'Train' },
  { value: 'Plane', label: 'Plane' },
];
const PREFERENCE_OPTIONS = ['Beach', 'Nature', 'Historical Places', 'Food', 'Culture', 'Adventure'];
const list = (data) => data?.items || (Array.isArray(data) ? data : []);
const escapeHtml = (value) => String(value ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

function buildLocationPopupHtml(location) {
  const priceLine = location.price != null
    ? `৳ ${Number(location.price).toLocaleString('en-BD')}/night`
    : location.entryFee != null ? `Entry ৳ ${Number(location.entryFee).toLocaleString('en-BD')}` : '';
  const lines = [
    `<strong>${escapeHtml(location.name)}</strong>`,
    `<div style="opacity:.7">${escapeHtml(location.type)}${location.districtName ? ' · ' + escapeHtml(location.districtName) : ''}</div>`,
    priceLine && `<div>${priceLine}</div>`,
    location.facilities && `<div>${escapeHtml(location.facilities)}</div>`,
    location.address && `<div>${escapeHtml(location.address)}</div>`,
    location.openingHours && `<div>${escapeHtml(location.openingHours)}</div>`,
    location.contactInfo && `<div>${escapeHtml(location.contactInfo)}</div>`,
    `<div>${escapeHtml(verificationBadge(location).label)}</div>`,
    coordinatesNote(location) && `<div style="opacity:.7">${escapeHtml(coordinatesNote(location))}</div>`,
    location.price == null && location.entryFee == null && `<div style="opacity:.7">Price / fee: not verified</div>`,
    `<a href="/tourism/locations/${location.id}" style="font-weight:600">View Details →</a>`,
  ].filter(Boolean);
  return `<div style="font-size:12px;line-height:1.5">${lines.join('')}</div>`;
}

// One real operator / service on the route. Only what a source states is shown; everything else
// says "not verified" -- these are not live timetables.
function TransportOptionCard({ option }) {
  const status = verificationBadge(option);
  return (
    <article className="flex flex-col rounded-xl border border-border bg-surface p-4">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-heading">{option.serviceName}</p>
          <p className="text-xs text-body/60">{option.operator}</p>
        </div>
        <Badge tone={status.tone}>{status.label}</Badge>
      </div>
      <dl className="mt-3 space-y-1 text-xs text-body/70">
        {option.serviceClasses && <div><dt className="inline font-medium text-heading">Classes: </dt><dd className="inline">{option.serviceClasses}</dd></div>}
        <div><dt className="inline font-medium text-heading">Time on road/rail: </dt><dd className="inline">{option.durationText || 'not verified'}</dd></div>
        <div><dt className="inline font-medium text-heading">Departures: </dt><dd className="inline">{option.schedule || 'not verified — check with the operator'}</dd></div>
        <div>
          <dt className="inline font-medium text-heading">Fare: </dt>
          <dd className="inline">
            {option.fareBdt != null ? `from ৳ ${Number(option.fareBdt).toLocaleString('en-BD')} (reported)` : 'not verified'}
          </dd>
        </div>
      </dl>
      {option.fareNote && <p className="mt-2 text-xs text-body/60">{option.fareNote}</p>}
      {option.notes && <p className="mt-2 text-xs text-body/60">{option.notes}</p>}
      {option.unverifiedFields && <p className="mt-2 text-[11px] text-body/50">Not verified: {option.unverifiedFields}</p>}
      <div className="mt-auto flex flex-wrap items-center gap-3 pt-3 text-xs">
        {option.bookingUrl?.startsWith('http') && (
          <a href={option.bookingUrl} target="_blank" rel="noopener noreferrer" className="font-semibold text-primary hover:underline">
            Official booking site →
          </a>
        )}
        {option.sourceUrl?.startsWith('http') && (
          <a href={option.sourceUrl} target="_blank" rel="noopener noreferrer" className="text-body/60 underline">
            Source
          </a>
        )}
        {option.dataRetrievedOn && (
          <span className="text-body/50">Checked {new Date(option.dataRetrievedOn).toLocaleDateString(undefined, { timeZone: 'UTC' })}</span>
        )}
      </div>
    </article>
  );
}

const SHOW_MORE_PREVIEW = 2;

// A card grid that shows the first two items and reveals the rest on demand. Give it a `key` that
// changes with the data set (mode, district) so it collapses again when that changes.
function ShowMoreGrid({ items, renderItem, noun = 'option' }) {
  const [expanded, setExpanded] = useState(false);
  const hiddenCount = Math.max(0, items.length - SHOW_MORE_PREVIEW);
  const visible = expanded ? items : items.slice(0, SHOW_MORE_PREVIEW);

  return (
    <>
      <div className="grid gap-4 sm:grid-cols-2">{visible.map(renderItem)}</div>
      {hiddenCount > 0 && (
        <button
          type="button"
          aria-expanded={expanded}
          onClick={() => setExpanded((prev) => !prev)}
          className="mt-4 rounded-full border border-border px-4 py-2 text-sm font-semibold text-primary hover:border-primary"
        >
          {expanded ? 'Show fewer' : `Show ${hiddenCount} more ${noun}${hiddenCount === 1 ? '' : 's'}`}
        </button>
      )}
    </>
  );
}

function TransportOptionsList({ mode, options }) {
  return (
    <div className="mt-4">
      <p className="mb-3 text-sm font-medium text-heading">
        {mode} options on this route ({options.length})
      </p>
      <ShowMoreGrid
        items={options}
        noun="option"
        renderItem={(option) => <TransportOptionCard key={`${option.mode}-${option.serviceName}`} option={option} />}
      />
    </div>
  );
}

function TogglePreference({ label, active, onToggle }) {
  return (
    <button
      type="button"
      aria-pressed={active}
      onClick={onToggle}
      className={`rounded-full border px-3.5 py-1.5 text-sm font-medium transition ${
        active ? 'border-primary bg-primary/10 text-primary' : 'border-border bg-background text-body/70 hover:border-primary/40'
      }`}
    >
      {label}
    </button>
  );
}

function ServiceCard({ service, onContact, contacted }) {
  return (
    <article className="flex flex-col overflow-hidden rounded-xl border border-border bg-surface">
      <div className="flex h-36 items-center justify-center bg-background">
        {service.imageUrl ? (
          <SafeImage src={service.imageUrl} alt={service.title} className="h-full w-full object-cover" />
        ) : (
          <span className="text-xs text-body/40">No photo yet</span>
        )}
      </div>
      <div className="flex flex-1 flex-col p-4">
        <p className="text-[10px] font-semibold uppercase tracking-wide text-primary">{service.type}</p>
        <h4 className="mt-1 text-sm font-semibold text-heading">{service.title}</h4>
        <p className="mt-1 text-xs text-body/60">{service.districtName}{service.facilities ? ` · ${service.facilities}` : ''}</p>
        {service.averageRating > 0 && (
          <p className="mt-1 text-xs text-body/60">★ {service.averageRating.toFixed(1)} ({service.reviewCount})</p>
        )}
        <p className="mt-2 text-base font-semibold text-heading">৳ {Number(service.price).toLocaleString('en-BD')}</p>
        <div className="mt-auto flex flex-wrap gap-2 pt-3">
          <Link
            to={routePaths.tourismServiceDetails.replace(':serviceId', service.id)}
            className="rounded-full border border-border px-3 py-1.5 text-xs font-semibold text-heading hover:border-primary"
          >
            View & book →
          </Link>
          <button
            type="button"
            disabled={contacted}
            onClick={() => onContact(service)}
            className="rounded-full border border-border px-3 py-1.5 text-xs font-semibold text-primary disabled:opacity-50"
          >
            {contacted ? 'Message sent' : 'Contact host'}
          </button>
        </div>
      </div>
    </article>
  );
}

const RADIUS_OPTIONS = [5, 10, 15, 25, 40];
const TYPE_LABELS = { GuestHouse: 'Guest House', HeritageSite: 'Heritage Site', TouristPlace: 'Tourist Place' };

// Popup for an accommodation marker: name, type, distance and whatever the record actually holds.
function buildNearbyPopupHtml(item) {
  const lines = [
    `<strong>${escapeHtml(item.name)}</strong>`,
    `<div style="opacity:.7">${escapeHtml(TYPE_LABELS[item.type] || item.type)}</div>`,
    `<div>${Number(item.distanceKm).toFixed(1)} km from destination centre</div>`,
    item.address && `<div>${escapeHtml(item.address)}</div>`,
    item.contactInfo && `<div>${escapeHtml(item.contactInfo)}</div>`,
    item.openingHours && `<div>Hours: ${escapeHtml(item.openingHours)}</div>`,
    item.price != null && `<div>৳ ${Number(item.price).toLocaleString('en-BD')}/night</div>`,
    item.price == null && `<div style="opacity:.7">Price: not available</div>`,
    `<div style="opacity:.7">${escapeHtml(verificationBadge(item).label)}</div>`,
    `<a href="/tourism/locations/${item.id}" style="font-weight:600">View Details →</a>`,
  ].filter(Boolean);
  return `<div style="font-size:12px;line-height:1.5">${lines.join('')}</div>`;
}

const STOP_TYPE_LABELS = { DatasetPlace: 'Tourism dataset', TourismLocation: 'ShilpoHub listing', HeritagePlace: 'Heritage place', TouristService: 'Experience' };

function LocationCard({ location }) {
  return (
    <article className="flex flex-col overflow-hidden rounded-xl border border-border bg-surface">
      <LocationMedia location={location} className="h-40" />
      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-center justify-between gap-2">
          <p className="text-[10px] font-semibold uppercase tracking-wide text-primary">{location.type}</p>
          <Badge tone={verificationBadge(location).tone}>{verificationBadge(location).label}</Badge>
        </div>
        <h4 className="mt-1 text-sm font-semibold text-heading">{location.name}</h4>
        {location.source && <p className="mt-1 text-xs text-body/50">Source: {location.source === 'Admin' ? 'ShilpoHub admin' : location.source}</p>}
        {(location.address || location.area) && <p className="mt-1 text-xs text-body/60">{location.address || location.area}</p>}
        {location.facilities && <p className="mt-1 text-xs text-body/60">{location.facilities}</p>}
        {location.openingHours && <p className="mt-1 text-xs text-body/60">Hours: {location.openingHours}</p>}
        {(location.price != null || location.entryFee != null) && (
          <p className="mt-2 text-base font-semibold text-heading">
            ৳ {Number(location.price ?? location.entryFee).toLocaleString('en-BD')}
            {location.price != null && <span className="text-xs font-normal text-body/60"> /night</span>}
            {location.price == null && location.entryFee != null && <span className="text-xs font-normal text-body/60"> entry</span>}
          </p>
        )}
        {location.price == null && ACCOMMODATION_TYPES.includes(location.type) && (
          <p className="mt-2 text-xs text-body/60">Price per night: not verified</p>
        )}
        {location.contactInfo && <p className="mt-1 text-xs text-body/70">{location.contactInfo}</p>}
        <div className="mt-auto pt-3">
          <Link
            to={routePaths.tourismLocationDetails.replace(':locationId', location.id)}
            className="rounded-full border border-border px-3 py-1.5 text-xs font-semibold text-heading hover:border-primary"
          >
            View Details →
          </Link>
        </div>
      </div>
    </article>
  );
}

export default function AiTourismPlanner() {
  const districtsQuery = useDistricts();
  const tourPlan = useTourPlan();
  const [searchParams, setSearchParams] = useSearchParams();
  const savedPlanId = searchParams.get('plan');
  const savedPlanQuery = useSavedTourPlan(savedPlanId);
  const { startConversation } = useMessagingMutations();
  const [form, setForm] = useState({
    originText: '',
    districtId: '',
    transportMode: 'Bus',
    startDate: '',
    durationDays: 2,
    partySize: 1,
    budget: '',
    preferences: [],
  });
  const [contactedIds, setContactedIds] = useState(() => new Set());

  const localTransportQuery = useTouristServices({ type: 'TransportationBooking', districtId: form.districtId || undefined, pageSize: 6 });
  const tourismLocationsQuery = useTourismLocations({ districtId: form.districtId || undefined, isActive: true, pageSize: 50 });

  // Opening a saved trip (?plan=<id>) refills the form with the request it was generated from.
  const savedRequest = savedPlanQuery.data?.request;
  useEffect(() => {
    if (!savedRequest) return;
    setForm({
      originText: savedRequest.originText || '',
      districtId: savedRequest.districtId || '',
      transportMode: savedRequest.transportMode || 'Bus',
      startDate: savedRequest.startDate ? String(savedRequest.startDate).slice(0, 10) : '',
      durationDays: savedRequest.durationDays || 2,
      partySize: savedRequest.partySize || 1,
      budget: savedRequest.budget != null ? String(savedRequest.budget) : '',
      preferences: savedRequest.preferences || [],
    });
  }, [savedRequest]);

  const togglePreference = (value) => {
    setForm((prev) => ({
      ...prev,
      preferences: prev.preferences.includes(value)
        ? prev.preferences.filter((p) => p !== value)
        : [...prev.preferences, value],
    }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    tourPlan.mutate({
      districtId: form.districtId || undefined,
      durationDays: Math.min(30, Math.max(1, Math.round(Number(form.durationDays)) || 1)),
      partySize: Math.min(100, Math.max(1, Math.round(Number(form.partySize)) || 1)),
      startDate: form.startDate || undefined,
      originText: form.originText || undefined,
      transportMode: form.transportMode,
      budget: form.budget ? Number(form.budget) : undefined,
      preferences: form.preferences,
    }, {
      // Keep the saved plan's id in the URL so a refresh (or the back button) reopens it
      // instead of losing the result.
      onSuccess: (result) => {
        if (result?.savedPlanId) setSearchParams({ plan: result.savedPlanId }, { replace: true });
      },
    });
  };

  const handleContact = (service) => {
    startConversation.mutate(
      { recipientId: service.producerId, body: `Hi! I'm planning a trip to ${service.districtName} and interested in "${service.title}". Is it available?` },
      { onSuccess: () => setContactedIds((prev) => new Set(prev).add(service.id)) },
    );
  };

  // A freshly generated plan is shown until the URL points at a different saved plan.
  const freshPlan = tourPlan.data && (!savedPlanId || tourPlan.data.savedPlanId === savedPlanId) ? tourPlan.data : null;
  const plan = freshPlan || savedPlanQuery.data?.plan;
  // Accommodation has its own section above; keep the rest of the admin-curated list to non-lodging places.
  const poiState = useTourismPois(form.districtId || undefined);
  const explorableLocations = poiState.items;
  const poiGroups = useMemo(() => {
    const known = new Set(POI_GROUPS.flatMap(([, types]) => types));
    const groups = POI_GROUPS.map(([label, types]) => [label, explorableLocations.filter((l) => types.includes(l.type))]);
    groups.push(['Other places', explorableLocations.filter((l) => !known.has(l.type))]);
    return groups.filter(([, items]) => items.length > 0);
  }, [explorableLocations]);
  const locationsById = useMemo(() => {
    const map = new Map();
    list(tourismLocationsQuery.data).forEach((l) => map.set(l.id, l));
    return map;
  }, [tourismLocationsQuery.data]);
  const stopsWithCoords = useMemo(
    () => (plan?.days || [])
      .flatMap((day) => day.stops)
      .filter((s) => s.latitude != null && s.longitude != null)
      .map((s, i) => {
        const location = s.referenceId ? locationsById.get(s.referenceId) : null;
        return {
          id: s.referenceId || `stop-${i}`,
          name: s.name,
          latitude: s.latitude,
          longitude: s.longitude,
          kind: location ? location.type.toLowerCase() : 'stop',
          popupHtml: location ? buildLocationPopupHtml(location) : undefined,
        };
      }),
    [plan, locationsById],
  );
  const locationMarkers = useMemo(
    () => list(tourismLocationsQuery.data).map((l) => ({
      id: l.id, name: l.name, latitude: l.latitude, longitude: l.longitude,
      kind: l.type.toLowerCase(), popupHtml: buildLocationPopupHtml(l),
    })),
    [tourismLocationsQuery.data],
  );
  const [radiusKm, setRadiusKm] = useState(15);
  const nearbyQuery = useNearbyAccommodations(form.districtId || undefined, radiusKm);
  const nearby = nearbyQuery.data;
  // The "Where to stay" cards and the map above share one source: accommodation within the search radius.
  const accommodation = { queries: [nearbyQuery], items: nearby?.items ?? [], isLoading: nearbyQuery.isLoading || Boolean(nearby?.isImporting) };
  const nearbyMarkers = useMemo(
    () => (nearby?.items ?? []).map((a) => ({
      id: a.id, name: a.name, latitude: a.latitude, longitude: a.longitude,
      kind: a.type.toLowerCase(), popupHtml: buildNearbyPopupHtml(a),
    })),
    [nearby],
  );
  const nearbyMapPlaces = useMemo(
    () => [
      nearby?.centerLatitude != null && {
        id: 'nearby-destination', name: `Destination: ${nearby.destinationName}`,
        latitude: nearby.centerLatitude, longitude: nearby.centerLongitude, kind: 'destination',
      },
      ...nearbyMarkers,
    ].filter(Boolean),
    [nearby, nearbyMarkers],
  );
  const mapPlaces = useMemo(() => {
    const origin = plan?.transportEstimate?.originPoint;
    const destination = plan?.transportEstimate?.destinationPoint;
    const seenIds = new Set();
    const combined = [
      origin && { id: 'origin', name: `Start: ${origin.displayName}`, latitude: origin.latitude, longitude: origin.longitude, kind: 'origin' },
      destination && { id: 'destination', name: `Destination: ${destination.displayName}`, latitude: destination.latitude, longitude: destination.longitude, kind: 'destination' },
      ...stopsWithCoords,
      ...locationMarkers,
      ...nearbyMarkers,
    ].filter(Boolean);
    return combined.filter((p) => (seenIds.has(p.id) ? false : (seenIds.add(p.id), true)));
  }, [plan, stopsWithCoords, locationMarkers, nearbyMarkers]);
  const routeStopIds = useMemo(() => stopsWithCoords.map((s) => s.id), [stopsWithCoords]);

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'AI Trip Planner' },
        ]}
        title="AI Trip Planner"
        description="Tell us your trip and get a day-by-day heritage itinerary, a real road-routed transport estimate, matching accommodation and a budget summary."
        action={
          <div className="flex flex-wrap items-center gap-3">
            <Link to={routePaths.tourismMyPlans} className="text-sm font-semibold text-primary hover:underline">
              My Trip Plans →
            </Link>
            {plan ? (
              <Badge tone={plan.isAiGenerated ? 'primary' : 'secondary'}>
                {plan.isAiGenerated ? 'Written by Gemini AI' : 'Rule-based plan (no AI key configured)'}
              </Badge>
            ) : (
              <Badge tone="primary">AI Powered</Badge>
            )}
          </div>
        }
      />
      {savedPlanId && !freshPlan && <QueryStatusBanner queries={[savedPlanQuery]} loadingLabel="Loading your saved trip…" />}

      <form onSubmit={handleSubmit} className="mb-10 grid gap-4 rounded-xl border border-border bg-surface p-6 sm:grid-cols-2 lg:grid-cols-3">
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-heading">Starting point</span>
          <input
            type="text"
            placeholder="e.g. Dhaka"
            value={form.originText}
            onChange={(event) => setForm((prev) => ({ ...prev, originText: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-heading">Destination district</span>
          <select
            value={form.districtId}
            onChange={(event) => setForm((prev) => ({ ...prev, districtId: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          >
            <option value="">Any district</option>
            {list(districtsQuery.data).map((district) => (
              <option key={district.id} value={district.id}>{district.name}</option>
            ))}
          </select>
        </label>
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-heading">Start date</span>
          <input
            type="date"
            value={form.startDate}
            onChange={(event) => setForm((prev) => ({ ...prev, startDate: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-heading">Duration (days)</span>
          <input
            type="number"
            required
            min={1}
            max={30}
            value={form.durationDays}
            onChange={(event) => setForm((prev) => ({ ...prev, durationDays: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-heading">Number of travelers</span>
          <input
            type="number"
            required
            min={1}
            max={100}
            value={form.partySize}
            onChange={(event) => setForm((prev) => ({ ...prev, partySize: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </label>
        <label className="block text-sm">
          <span className="mb-1.5 block font-medium text-heading">Budget (৳, optional)</span>
          <input
            type="number"
            min={0}
            placeholder="e.g. 15000"
            value={form.budget}
            onChange={(event) => setForm((prev) => ({ ...prev, budget: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </label>

        <div className="sm:col-span-2 lg:col-span-3">
          <span className="mb-1.5 block text-sm font-medium text-heading">Transport mode</span>
          <div className="flex flex-wrap gap-2">
            {TRANSPORT_MODES.map((mode) => (
              <button
                key={mode.value}
                type="button"
                aria-pressed={form.transportMode === mode.value}
                onClick={() => setForm((prev) => ({ ...prev, transportMode: mode.value }))}
                className={`rounded-full border px-4 py-2 text-sm font-medium transition ${
                  form.transportMode === mode.value ? 'border-primary bg-primary/10 text-primary' : 'border-border bg-background text-body/70'
                }`}
              >
                {mode.label}
              </button>
            ))}
          </div>
        </div>

        <div className="sm:col-span-2 lg:col-span-3">
          <span className="mb-1.5 block text-sm font-medium text-heading">Interests</span>
          <div className="flex flex-wrap gap-2">
            {PREFERENCE_OPTIONS.map((option) => (
              <TogglePreference
                key={option}
                label={option}
                active={form.preferences.includes(option)}
                onToggle={() => togglePreference(option)}
              />
            ))}
          </div>
        </div>

        <Button type="submit" variant="primary" className="sm:col-span-2 lg:col-span-3" disabled={tourPlan.isPending}>
          {tourPlan.isPending ? 'Planning…' : 'Generate Itinerary'}
        </Button>
      </form>

      {form.districtId && (
        <div className="mt-8">
          <SectionHeader
            eyebrow="Near your destination"
            title="Places to stay on the map"
            description="Hotels, hostels, resorts and guest houses around the destination, found from OpenStreetMap. Click a marker for its name, type and distance."
          />
          <div className="mb-3 flex flex-wrap items-center gap-3 text-sm">
            <label className="flex items-center gap-2 text-body/70">
              Search radius
              <select
                value={radiusKm}
                onChange={(event) => setRadiusKm(Number(event.target.value))}
                className="rounded-md border border-border bg-background px-2 py-1 text-sm"
              >
                {RADIUS_OPTIONS.map((r) => (
                  <option key={r} value={r}>{r} km</option>
                ))}
              </select>
            </label>
            {nearbyQuery.isLoading && <span className="text-body/60">Searching for accommodation around the destination…</span>}
            {nearby?.isImporting && <span className="text-body/60">Still fetching from OpenStreetMap — results will appear here shortly.</span>}
            {nearby && !nearby.isImporting && (
              <span className="text-body/60">
                {nearby.items.length > 0
                  ? `${nearby.items.length} place${nearby.items.length === 1 ? '' : 's'} to stay within ${nearby.radiusKm} km`
                  : 'No accommodations found within this radius.'}
              </span>
            )}
            {nearbyQuery.isError && <span className="text-red-700">Could not load nearby accommodation. Please try again.</span>}
          </div>
          {nearby?.message && <p className="mb-3 text-xs text-body/60">{nearby.message}</p>}
          {nearbyMapPlaces.length > 0 && !plan && <HeritageLeafletMap places={nearbyMapPlaces} />}
          {nearby?.items?.length > 0 && (
            <ul className="mt-3 grid gap-2 text-sm sm:grid-cols-2">
              {nearby.items.slice(0, 12).map((a) => (
                <li key={a.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2">
                  <span><span className="font-medium text-heading">{a.name}</span> <span className="text-body/60">· {TYPE_LABELS[a.type] || a.type}</span></span>
                  <span className="text-xs text-body/60">{a.distanceKm.toFixed(1)} km</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <QueryStatusBanner queries={[tourPlan]} loadingLabel="Building your itinerary…" />

      {plan && (
        <div className="space-y-10">
          <div>
            <SectionHeader eyebrow="Your Trip" title="Suggested Itinerary" description={plan.summary} />
            {plan.savedPlanId && (
              <p className="-mt-2 mb-4 text-xs text-body/60">
                Saved to your trip history ·{' '}
                <Link to={routePaths.tourismMyPlans} className="font-semibold text-primary hover:underline">
                  View My Trip Plans
                </Link>
              </p>
            )}
            <div className="space-y-4">
              {plan.days.map((day) => (
                <div key={day.dayNumber} className="rounded-xl border border-border bg-surface p-5">
                  <p className="text-sm font-semibold text-heading">
                    Day {day.dayNumber}
                    {day.date && ` · ${new Date(day.date).toLocaleDateString()}`}
                  </p>
                  <ul className="mt-3 space-y-2">
                    {day.stops.map((stop, i) => (
                      <li key={i} className="text-sm text-body/70">
                        <span className="font-medium text-heading">{stop.name}</span> ({STOP_TYPE_LABELS[stop.type] || stop.type})
                        {stop.type !== 'FreeTime' && stop.type !== 'Meal' && stop.type !== 'Rest' && stop.latitude == null && (
                          <span className="ml-2 rounded-full bg-border px-2 py-0.5 text-[10px] font-semibold uppercase text-body/60">
                            Coordinates unavailable
                          </span>
                        )}
                        {!stop.referenceId && !['FreeTime', 'Meal', 'Rest'].includes(stop.type) && (
                          <span className="ml-2 rounded-full bg-amber-100 px-2 py-0.5 text-[10px] font-semibold uppercase text-amber-800">
                            AI-suggested · unverified location
                          </span>
                        )}
                        {stop.estimatedDurationHours && (
                          <span className="ml-2 text-xs text-body/60">
                            ~{stop.estimatedDurationHours}h{stop.durationIsEstimated ? ' (estimated)' : ''}
                          </span>
                        )}
                        {stop.notes && <span className="text-body/50"> — {stop.notes}</span>}
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
            {plan.accommodationRecommendation && (
              <p className="mt-4 text-sm text-body/70">
                <span className="font-medium text-heading">Where to stay: </span>
                {plan.accommodationRecommendation}
              </p>
            )}
            {plan.unverifiedNotes?.length > 0 && (
              <ul className="mt-3 list-disc pl-5 text-xs text-body/60">
                {plan.unverifiedNotes.map((note, i) => (
                  <li key={i}>{note}</li>
                ))}
              </ul>
            )}
            {plan.highlightedFestivals.length > 0 && (
              <p className="mt-4 text-sm text-body/70">
                <span className="font-medium text-heading">Festivals to watch for: </span>
                {plan.highlightedFestivals.join(', ')}
              </p>
            )}
          </div>

          {plan.transportEstimate && (
            <div>
              <SectionHeader eyebrow="Getting there" title={`Transport — ${plan.transportEstimate.mode}`} />
              <div className="rounded-xl border border-border bg-surface p-5">
                {plan.transportEstimate.isVerifiedSchedule ? (
                  <Badge tone="success">Verified schedule</Badge>
                ) : (
                  <Badge tone="secondary">Estimate — not a live schedule</Badge>
                )}
                {(plan.transportEstimate.originPoint || plan.transportEstimate.destinationPoint) && (
                  <p className="mt-3 text-xs text-body/60">
                    {plan.transportEstimate.originPoint && <>From <span className="font-medium text-heading">{plan.transportEstimate.originPoint.displayName}</span></>}
                    {plan.transportEstimate.originPoint && plan.transportEstimate.destinationPoint && ' → '}
                    {plan.transportEstimate.destinationPoint && <span className="font-medium text-heading">{plan.transportEstimate.destinationPoint.displayName}</span>}
                  </p>
                )}
                {plan.transportEstimate.estimatedDistanceKm != null && (
                  <p className="mt-3 text-sm text-heading">
                    ≈ {plan.transportEstimate.estimatedDistanceKm} km (real road route) · ≈ {Math.floor(plan.transportEstimate.estimatedDurationMinutes / 60)}h{' '}
                    {plan.transportEstimate.estimatedDurationMinutes % 60}m by car
                  </p>
                )}
                <p className="mt-2 text-sm text-body/70">{plan.transportEstimate.notes}</p>
              </div>
              {plan.transportEstimate.options?.length > 0 && (
                <TransportOptionsList
                  key={`${plan.transportEstimate.mode}-${plan.savedPlanId ?? 'new'}`}
                  mode={plan.transportEstimate.mode}
                  options={plan.transportEstimate.options}
                />
              )}
              {list(localTransportQuery.data).length > 0 && (
                <div className="mt-4">
                  <p className="mb-3 text-sm font-medium text-heading">Local transport you can book in this district</p>
                  <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {list(localTransportQuery.data).map((service) => (
                      <ServiceCard key={service.id} service={service} onContact={handleContact} contacted={contactedIds.has(service.id)} />
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          <div>
            <SectionHeader
              eyebrow="Where to stay"
              title="Accommodation"
              description={`Hotels, resorts, hostels and guest houses within ${radiusKm} km of the destination, nearest first — from ShilpoHub admins and OpenStreetMap.`}
            />
            {form.districtId && <QueryStatusBanner queries={accommodation.queries} loadingLabel="Finding accommodation…" />}
            {!form.districtId ? (
              <p className="rounded-xl border border-border bg-surface p-5 text-sm text-body/70">
                Choose a destination district to see its accommodations.
              </p>
            ) : accommodation.items.length > 0 ? (
              <ShowMoreGrid
                key={form.districtId}
                items={accommodation.items}
                noun="accommodation"
                renderItem={(location) => <LocationCard key={location.id} location={location} />}
              />
            ) : (
              !accommodation.isLoading &&
              !accommodation.queries.some((q) => q.isError) && (
                <p className="rounded-xl border border-border bg-surface p-5 text-sm text-body/70">
                  No accommodations found within {radiusKm} km of this destination. Try a larger search radius above.
                </p>
              )
            )}
          </div>

          <div>
            <SectionHeader
              eyebrow="Explore & eat"
              title="More places to explore & eat"
              description="Restaurants, attractions and heritage sites for this district, from ShilpoHub admins and OpenStreetMap."
            />
            <QueryStatusBanner queries={[poiState.query]} loadingLabel="Loading places…" />
            {explorableLocations.length > 0 ? (
              <div className="space-y-6">
                {poiGroups.map(([label, items]) => (
                  <div key={label}>
                    <h4 className="mb-3 text-sm font-semibold text-heading">{label}</h4>
                    <ShowMoreGrid
                      key={`${form.districtId}-${label}`}
                      items={items}
                      noun="place"
                      renderItem={(location) => <LocationCard key={location.id} location={location} />}
                    />
                  </div>
                ))}
              </div>
            ) : (
              !poiState.query.isLoading && (
                <p className="rounded-xl border border-border bg-surface p-5 text-sm text-body/70">
                  No other places found for this district.
                </p>
              )
            )}
          </div>

          {plan.estimatedBudget && (
            <div>
              <SectionHeader eyebrow="Budget" title="Estimated trip cost" />
              <div className="rounded-xl border border-border bg-surface p-5">
                <ul className="space-y-2">
                  {plan.estimatedBudget.lineItems.map((item, i) => (
                    <li key={i} className="flex justify-between text-sm text-body/70">
                      <span>{item.label} <span className="text-body/40">({item.category})</span></span>
                      <span className="font-medium text-heading">৳ {Number(item.amount).toLocaleString('en-BD')}</span>
                    </li>
                  ))}
                </ul>
                <div className="mt-4 flex items-center justify-between border-t border-border pt-4">
                  <span className="text-sm font-semibold text-heading">Known / verified total</span>
                  <span className="text-lg font-semibold text-primary">৳ {Number(plan.estimatedBudget.totalEstimatedCost).toLocaleString('en-BD')}</span>
                </div>
                <p className="mt-1 text-xs text-body/50">৳ {Number(plan.estimatedBudget.perPersonCost).toLocaleString('en-BD')} per person</p>
                {form.budget && Number(form.budget) < (plan.estimatedBudget.estimatedTotal || plan.estimatedBudget.totalEstimatedCost) && (
                  <p className="mt-3 text-sm text-amber-700">This is above your stated budget of ৳ {Number(form.budget).toLocaleString('en-BD')}.</p>
                )}
                {plan.estimatedBudget.estimatedItems?.length > 0 && (
                  <div className="mt-4 rounded-lg border border-border p-3">
                    <p className="text-xs font-semibold text-heading">Rough estimate for the unverified parts</p>
                    <ul className="mt-2 space-y-1">
                      {plan.estimatedBudget.estimatedItems.map((item, i) => (
                        <li key={i} className="flex justify-between text-xs text-body/70">
                          <span>{item.label}</span>
                          <span className="font-medium text-heading">৳ {Number(item.amount).toLocaleString('en-BD')}</span>
                        </li>
                      ))}
                    </ul>
                    <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
                      <span className="text-sm font-semibold text-heading">Estimated total (with assumptions)</span>
                      <span className="text-lg font-semibold text-primary">৳ {Number(plan.estimatedBudget.estimatedTotal).toLocaleString('en-BD')}</span>
                    </div>
                    <p className="mt-1 text-xs text-body/50">৳ {Number(plan.estimatedBudget.estimatedPerPerson).toLocaleString('en-BD')} per person · {plan.estimatedBudget.estimateNote}</p>
                  </div>
                )}
                {plan.estimatedBudget.unverifiedCosts?.length > 0 && (
                  <div className="mt-4 rounded-lg bg-amber-50 p-3">
                    <p className="text-xs font-semibold text-amber-800">Unverified / excluded from the total</p>
                    <ul className="mt-1 list-disc pl-5 text-xs text-amber-800/80">
                      {plan.estimatedBudget.unverifiedCosts.map((note, i) => (
                        <li key={i}>{note}</li>
                      ))}
                    </ul>
                  </div>
                )}
                {plan.estimatedBudget.notes && <p className="mt-3 text-xs text-body/50">{plan.estimatedBudget.notes}</p>}
              </div>
            </div>
          )}

          {mapPlaces.length > 0 && (
            <div>
              <SectionHeader eyebrow="Map" title="Your itinerary on the map" description="Green = starting point, dark = destination, other colors = hotels, resorts, restaurants and attractions. Click a marker for details." />
              <HeritageLeafletMap places={mapPlaces} route={routeStopIds} routeGeometry={plan.transportEstimate?.routeGeometry} />
            </div>
          )}
        </div>
      )}
    </div>
  );
}
