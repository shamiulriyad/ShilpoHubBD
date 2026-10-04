import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import {
  useVisitedLocations,
  useDistrictCoverage,
  useCulturalAchievements,
  useFestivalParticipation,
} from '../../hooks/useTouristAnalytics';

const tiers = [
  { name: 'Silver', minimum: 1, discount: 5, accent: 'border-slate-300 bg-slate-50' },
  { name: 'Gold', minimum: 5, discount: 10, accent: 'border-amber-300 bg-amber-50' },
  { name: 'Diamond', minimum: 10, discount: 15, accent: 'border-cyan-300 bg-cyan-50' },
];

function getTier(siteCount) {
  return [...tiers].reverse().find((tier) => siteCount >= tier.minimum) || { name: 'Explorer', minimum: 0, discount: 0 };
}

export default function TravelPassport() {
  const visitedQuery = useVisitedLocations();
  const coverageQuery = useDistrictCoverage();
  const achievementsQuery = useCulturalAchievements();
  const festivalsQuery = useFestivalParticipation();

  const visited = visitedQuery.data || [];
  const coverage = coverageQuery.data;
  const achievements = achievementsQuery.data;
  const tier = getTier(visited.length);
  const nextTier = tiers.find((item) => item.minimum > visited.length);
  const progressStart = tier.minimum;
  const progressRange = nextTier ? nextTier.minimum - progressStart : 1;
  const progress = nextTier ? Math.min(100, ((visited.length - progressStart) / progressRange) * 100) : 100;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Travel Passport' },
        ]}
        title="Travel Passport"
        description="Every verified visit moves you toward better hotel savings."
      />

      <section className="mb-6 overflow-hidden rounded-2xl bg-gradient-to-br from-primary-dark via-primary to-emerald-700 p-6 text-white shadow-lg lg:p-8">
        <div className="grid gap-6 lg:grid-cols-[1fr_auto] lg:items-center">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-white/70">ShilpoHub travel passport</p>
            <div className="mt-3 flex flex-wrap items-end gap-3">
              <h2 className="text-3xl font-bold">{tier.name} tier</h2>
              <span className="mb-1 rounded-full bg-white/15 px-3 py-1 text-sm font-semibold backdrop-blur">
                {tier.discount}% hotel discount
              </span>
            </div>
            <p className="mt-3 max-w-2xl text-sm leading-6 text-white/80">
              Discounts are applied automatically to homestay and hotel bookings. Only unique heritage sites count toward your tier.
            </p>
            <div className="mt-5 max-w-xl">
              <div className="mb-2 flex justify-between text-xs font-medium text-white/75">
                <span>{visited.length} unique sites</span>
                <span>{nextTier ? `${nextTier.minimum - visited.length} more to ${nextTier.name}` : 'Top tier unlocked'}</span>
              </div>
              <div className="h-2 overflow-hidden rounded-full bg-black/20"><div className="h-full rounded-full bg-amber-300 transition-all" style={{ width: `${progress}%` }} /></div>
            </div>
          </div>
          <div className="flex h-28 w-28 items-center justify-center rounded-2xl border border-white/25 bg-white/10 text-center backdrop-blur">
            <div><span className="block text-4xl">◆</span><span className="mt-1 block text-xs font-bold uppercase tracking-widest">{tier.name}</span></div>
          </div>
        </div>
      </section>

      <div className="mb-10 grid gap-3 sm:grid-cols-3">
        {tiers.map((item) => {
          const unlocked = visited.length >= item.minimum;
          return <div key={item.name} className={`rounded-xl border p-4 ${unlocked ? item.accent : 'border-border bg-surface opacity-60'}`}>
            <div className="flex items-center justify-between"><p className="font-semibold text-heading">{item.name}</p><span className="text-xs font-semibold">{unlocked ? 'Unlocked' : `${item.minimum} sites`}</span></div>
            <p className="mt-1 text-sm text-body/65">{item.discount}% off eligible stays</p>
          </div>;
        })}
      </div>

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

      <div className="mb-4 flex items-end justify-between"><div><p className="text-lg font-semibold text-heading">Passport stamps</p><p className="text-sm text-body/60">Your verified heritage journey across Bangladesh</p></div></div>
      <AsyncState isLoading={visitedQuery.isLoading} isError={visitedQuery.isError} error={visitedQuery.error}>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {visited.map((place) => (
            <div
              key={place.heritagePlaceId}
              className="relative flex min-h-32 flex-col items-center justify-center overflow-hidden rounded-xl border border-primary/30 bg-surface p-4 text-center shadow-sm"
            >
              <span className="absolute right-2 top-2 text-xl text-primary/20">◆</span>
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
