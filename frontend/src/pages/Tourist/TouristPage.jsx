import { Link } from 'react-router-dom';
import { PageHeader, QueryStatusBanner, AnalyticsChart } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { routePaths } from '../../routes/routePaths';
import { useVisitedLocations, useTouristBookingStats, useFestivalParticipation, useDistrictCoverage, useCulturalAchievements } from '../../hooks/useTouristAnalytics';

export default function TouristPage() {
  const visited = useVisitedLocations();
  const bookings = useTouristBookingStats();
  const festivals = useFestivalParticipation();
  const coverage = useDistrictCoverage();
  const achievements = useCulturalAchievements();
  const queries = [visited, bookings, festivals, coverage, achievements];
  return <div><PageHeader title="Tourist dashboard" description="Plan heritage trips and follow your real travel activity." action={<Link to={routePaths.tourismMap} className="rounded-full bg-primary px-5 py-3 font-semibold text-white">Explore places</Link>}/><QueryStatusBanner queries={queries}/>
    <div className="grid grid-cols-2 gap-4 lg:grid-cols-4"><StatCard label="Places visited" value={visited.isSuccess ? visited.data.length : '—'}/><StatCard label="Total bookings" value={bookings.data?.totalBookings ?? '—'}/><StatCard label="Districts explored" value={coverage.isSuccess ? `${coverage.data.visitedDistrictCount} / ${coverage.data.totalDistrictCount}` : '—'}/><StatCard label="Badges earned" value={achievements.data?.totalBadges ?? '—'}/></div>
    <div className="mt-8 grid gap-5 xl:grid-cols-[1.35fr_1fr]"><AnalyticsChart title="Most visited heritage places" data={(visited.data || []).map(place => ({label:place.heritagePlaceName,value:place.visitCount}))}/><section className="rounded-2xl border border-border bg-surface p-5"><h2 className="font-semibold">Trip summary</h2><dl className="mt-4 space-y-3 text-sm"><div className="flex justify-between"><dt>Pending bookings</dt><dd>{bookings.data?.pendingBookings ?? '—'}</dd></div><div className="flex justify-between"><dt>Completed bookings</dt><dd>{bookings.data?.completedBookings ?? '—'}</dd></div><div className="flex justify-between"><dt>Festival experiences</dt><dd>{festivals.data?.festivalBadgeCount ?? '—'}</dd></div><div className="flex justify-between border-t border-border pt-3"><dt>Total travel spending</dt><dd>৳ {bookings.data?.totalSpent?.toLocaleString() ?? '—'}</dd></div></dl></section></div>
    <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">{[[routePaths.tourismMap,'Heritage map','Search locations and open complete place details.'],[routePaths.tourismRoutes,'Tour routes','Find curated journeys through craft communities.'],[routePaths.tourismBookings,'My bookings','Review upcoming and previous bookings.'],[routePaths.tourismMyPlans,'My trip plans','Reopen the AI itineraries you have generated.'],[routePaths.tourismPassport,'Travel passport','See recorded visits and cultural achievements.'],[routePaths.tourismFestivals,'Festivals','Discover published cultural festivals.'],[routePaths.tourismServices,'Tourist services','Book local guides and experiences.']].map(([to,title,description]) => <Link key={to} to={to} className="rounded-2xl border border-border bg-surface p-5 transition hover:-translate-y-0.5 hover:shadow-md"><h2 className="font-semibold">{title}</h2><p className="mt-2 text-sm text-body/70">{description}</p><p className="mt-4 text-sm font-semibold text-primary">Open →</p></Link>)}</div>
  </div>;
}
