import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { BadgeCard } from '../../components/cards';
import { useAllBadges, useMyBadges } from '../../hooks/usePassport';

export default function BadgeCollection() {
  const allQuery = useAllBadges();
  const mineQuery = useMyBadges();

  const earnedBadgeIds = new Set((mineQuery.data || []).map((b) => b.badgeId));
  const badges = allQuery.data || [];
  const earnedCount = earnedBadgeIds.size;

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Badge Collection' }]}
        title="Badge Collection"
        description={`${earnedCount} of ${badges.length} badges earned.`}
        action={
          <Link to={routePaths.customerAchievements} className="text-sm font-medium text-link hover:underline">
            View achievements →
          </Link>
        }
      />

      <AsyncState isLoading={allQuery.isLoading} isError={allQuery.isError} error={allQuery.error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {badges.map((badge) => (
            <BadgeCard
              key={badge.id}
              badge={{
                id: badge.id,
                name: badge.name,
                description: badge.description,
                icon: '🏅',
                earned: earnedBadgeIds.has(badge.id),
              }}
            />
          ))}
          {badges.length === 0 && <p className="col-span-full text-sm text-body/60">No badges defined yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
