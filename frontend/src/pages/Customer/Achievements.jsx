import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { achievements } from '../../data/mockData';

export default function Achievements() {
  const completedCount = achievements.filter((a) => a.current >= a.target).length;

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Achievements' }]}
        title="Achievements"
        description={`${completedCount} of ${achievements.length} achievements unlocked.`}
        action={
          <Link to={routePaths.customerBadges} className="text-sm font-medium text-link hover:underline">
            View badge collection →
          </Link>
        }
      />

      <div className="space-y-4">
        {achievements.map((achievement) => {
          const complete = achievement.current >= achievement.target;
          const percent = Math.min(100, Math.round((achievement.current / achievement.target) * 100));
          return (
            <div key={achievement.id} className="rounded-xl border border-border bg-surface p-5">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="text-sm font-semibold text-heading">{achievement.title}</p>
                <Badge tone={complete ? 'success' : 'neutral'}>
                  {complete ? 'Completed' : `${achievement.current}/${achievement.target}`}
                </Badge>
              </div>
              <p className="mt-1 text-xs text-body/60">{achievement.description}</p>
              <div className="mt-3 h-2 w-full overflow-hidden rounded-full bg-background">
                <div className="h-full rounded-full bg-primary" style={{ width: `${percent}%` }} />
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
