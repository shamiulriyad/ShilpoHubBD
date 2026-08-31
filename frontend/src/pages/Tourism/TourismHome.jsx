import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, AsyncState } from '../../components/ui';
import { EntityCard, FestivalCard } from '../../components/cards';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useRecommendedHeritageRoutes } from '../../hooks/useHeritageRoutes';

const links = [
  { title: 'Heritage Map', description: 'Interactive heritage locations', to: routePaths.tourismMap },
  { title: 'Festival Directory', description: 'Cultural festival directory', to: routePaths.tourismFestivals },
  { title: 'Cultural Events', description: 'Upcoming events calendar', to: routePaths.tourismEvents },
  { title: 'Village Explorer', description: 'Explore heritage villages as a traveler', to: routePaths.tourismVillages },
  { title: 'Tour Routes', description: 'Guided heritage travel routes', to: routePaths.tourismRoutes },
  { title: 'Travel Passport', description: 'Track the heritage sites you have visited', to: routePaths.tourismPassport },
  { title: 'Local Cuisine', description: 'Traditional dishes and where to try them', to: routePaths.tourismCuisines },
  { title: 'Tourist Services', description: 'Book guides, workshops, homestays and transport', to: routePaths.tourismServices },
  { title: 'My Bookings', description: 'Manage your service bookings', to: routePaths.tourismBookings },
  { title: 'AI Trip Planner', description: 'Get an AI-generated day-by-day itinerary', to: routePaths.tourismAiPlanner },
];

export default function TourismHome() {
  const festivalsQuery = useHeritageFestivals({ pageSize: 3 });
  const routesQuery = useRecommendedHeritageRoutes();

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Tourism' }]}
        title="Tourism"
        description="Plan heritage journeys across Bangladesh."
      />

      <div className="mb-10 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {links.map((link) => (
          <EntityCard key={link.title} title={link.title} subtitle={link.description} to={link.to} />
        ))}
      </div>

      <SectionHeader eyebrow="Upcoming" title="Festivals" />
      <AsyncState isLoading={festivalsQuery.isLoading} isError={festivalsQuery.isError} error={festivalsQuery.error}>
        <div className="mb-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {festivalsQuery.data?.items.map((festival) => (
            <FestivalCard
              key={festival.id}
              festival={{ name: festival.name, date: festival.startDate, district: festival.districtName }}
            />
          ))}
          {festivalsQuery.data?.items.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No upcoming festivals right now.</p>
          )}
        </div>
      </AsyncState>

      <SectionHeader eyebrow="Plan a trip" title="Recommended Tour Routes" />
      <AsyncState isLoading={routesQuery.isLoading} isError={routesQuery.isError} error={routesQuery.error}>
        <div className="grid gap-4 sm:grid-cols-2">
          {routesQuery.data?.map((route) => (
            <div key={route.id} className="rounded-xl border border-border bg-surface p-4">
              <p className="text-sm font-semibold text-heading">{route.name}</p>
              <p className="mt-1 text-xs text-body/60">
                {Math.round(route.estimatedDurationMinutes / 60)}h · {route.stops.length} stops
              </p>
            </div>
          ))}
          {routesQuery.data?.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No recommended routes yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
