import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
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
          <div
            key={badge.id}
            className={`flex flex-col items-center gap-2 rounded-xl border p-5 text-center ${
              badge.earned ? 'border-border bg-surface' : 'border-dashed border-border bg-surface opacity-50 grayscale'
            }`}
          >
            <span className="text-3xl">{badge.icon}</span>
            <p className="text-sm font-semibold text-heading">{badge.name}</p>
            <p className="text-xs text-body/60">{badge.description}</p>
            {!badge.earned && <p className="text-xs font-medium text-body/40">Locked</p>}
          </div>
        ))}
      </div>
    </div>
  );
}
