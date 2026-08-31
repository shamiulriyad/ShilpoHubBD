import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import {
  useVisitedLocations,
  useDistrictCoverage,
  useCulturalAchievements,
  useFestivalParticipation,
} from '../../hooks/useTouristAnalytics';

export default function TravelPassport() {
  const visitedQuery = useVisitedLocations();
  const coverageQuery = useDistrictCoverage();
  const achievementsQuery = useCulturalAchievements();
  const festivalsQuery = useFestivalParticipation();

  const visited = visitedQuery.data || [];
  const coverage = coverageQuery.data;
  const achievements = achievementsQuery.data;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Travel Passport' },
        ]}
        title="Travel Passport"
        description="Track the heritage sites and villages you've visited."
      />

      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <StatCard label="Sites Visited" value={visited.length} />
        <StatCard label="Districts Explored" value={coverage ? `${coverage.visitedDistrictCount}/${coverage.totalDistrictCount}` : '—'} />
        <StatCard label="Badges Earned" value={achievements?.totalBadges ?? '—'} />
      </div>

      {festivalsQuery.data?.festivalNames.length > 0 && (
        <p className="mb-6 text-sm text-body/70">
          <span className="font-medium text-heading">Festivals attended: </span>
          {festivalsQuery.data.festivalNames.join(', ')}
        </p>
      )}

      <p className="mb-3 text-sm font-semibold text-heading">Visited Places</p>
      <AsyncState isLoading={visitedQuery.isLoading} isError={visitedQuery.isError} error={visitedQuery.error}>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {visited.map((place) => (
            <div
              key={place.heritagePlaceId}
              className="flex flex-col items-center justify-center rounded-xl border border-primary bg-primary/10 p-4 text-center"
            >
              <p className="text-sm font-semibold text-primary">{place.heritagePlaceName}</p>
              <p className="text-xs text-body/60">{place.districtName}</p>
              <p className="mt-1 text-[11px] text-body/50">{place.visitCount} visit{place.visitCount > 1 ? 's' : ''}</p>
            </div>
          ))}
          {visited.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No visits recorded yet — check in at a heritage place to start your passport.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
