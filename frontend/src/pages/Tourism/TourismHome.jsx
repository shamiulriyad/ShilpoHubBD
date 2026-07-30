import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader } from '../../components/ui';
import { EntityCard, FestivalCard } from '../../components/cards';
import { festivals, tourRoutes } from '../../data/mockData';

const links = [
  { title: 'Heritage Map', description: 'Interactive heritage locations', to: routePaths.tourismMap },
  { title: 'Festival Directory', description: 'Cultural festival directory', to: routePaths.tourismFestivals },
  { title: 'Cultural Events', description: 'Upcoming events calendar', to: routePaths.tourismEvents },
  { title: 'Village Explorer', description: 'Explore heritage villages as a traveler', to: routePaths.tourismVillages },
  { title: 'Tour Routes', description: 'Guided heritage travel routes', to: routePaths.tourismRoutes },
  { title: 'Travel Passport', description: 'Track the heritage sites you have visited', to: routePaths.tourismPassport },
];

export default function TourismHome() {
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
      <div className="mb-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {festivals.slice(0, 3).map((festival) => (
          <FestivalCard key={festival.id} festival={festival} />
        ))}
      </div>

      <SectionHeader eyebrow="Plan a trip" title="Popular Tour Routes" />
      <div className="grid gap-4 sm:grid-cols-2">
        {tourRoutes.map((route) => (
          <div key={route.id} className="rounded-xl border border-border bg-surface p-4">
            <p className="text-sm font-semibold text-heading">{route.name}</p>
            <p className="mt-1 text-xs text-body/60">
              {route.duration} · {route.stops} stops
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
