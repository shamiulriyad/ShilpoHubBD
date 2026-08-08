import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { BadgeCard } from '../../components/cards';
import { badges } from '../../data/mockData';

export default function BadgeCollection() {
  const earnedCount = badges.filter((b) => b.earned).length;

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

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {badges.map((badge) => (
          <BadgeCard key={badge.id} badge={badge} />
        ))}
      </div>
    </div>
  );
}
