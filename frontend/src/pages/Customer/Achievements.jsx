import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMyXpSummary, useAllAchievements, useMyAchievements, useEvaluateAchievements } from '../../hooks/useAchievements';

export default function Achievements() {
  const xpQuery = useMyXpSummary();
  const allQuery = useAllAchievements();
  const mineQuery = useMyAchievements();
  const evaluate = useEvaluateAchievements();

  const unlockedIds = new Set((mineQuery.data || []).map((a) => a.achievementId));
  const achievements = allQuery.data || [];
  const completedCount = unlockedIds.size;
  const xp = xpQuery.data;

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Achievements' }]}
        title="Achievements"
        description={`${completedCount} of ${achievements.length} achievements unlocked.`}
        action={
          <div className="flex items-center gap-3">
            <Link to={routePaths.customerBadges} className="text-sm font-medium text-link hover:underline">
              View badge collection →
            </Link>
            <Button variant="secondary" onClick={() => evaluate.mutate()} disabled={evaluate.isPending}>
              {evaluate.isPending ? 'Checking…' : 'Check for New Unlocks'}
            </Button>
          </div>
        }
      />

      {xp && (
        <div className="mb-8 rounded-xl border border-border bg-surface p-5">
          <div className="flex items-center justify-between">
            <p className="text-sm font-semibold text-heading">Level {xp.level}</p>
            <p className="text-xs text-body/60">{xp.totalXp} XP total</p>
          </div>
          <div className="mt-3 h-2 w-full overflow-hidden rounded-full bg-background">
            <div
              className="h-full rounded-full bg-primary"
              style={{ width: `${Math.min(100, Math.round((xp.xpIntoCurrentLevel / xp.xpForNextLevel) * 100))}%` }}
            />
          </div>
          <p className="mt-2 text-xs text-body/50">{xp.xpToNextLevel} XP to next level</p>
        </div>
      )}

      <AsyncState isLoading={allQuery.isLoading} isError={allQuery.isError} error={allQuery.error}>
        <div className="space-y-4">
          {achievements.map((achievement) => {
            const complete = unlockedIds.has(achievement.id);
            const percent = xp ? Math.min(100, Math.round((xp.totalXp / achievement.requiredXp) * 100)) : 0;
            return (
              <div key={achievement.id} className="rounded-xl border border-border bg-surface p-5">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="text-sm font-semibold text-heading">{achievement.name}</p>
                  <Badge tone={complete ? 'success' : 'neutral'}>
                    {complete ? 'Completed' : `Requires ${achievement.requiredXp} XP`}
                  </Badge>
                </div>
                <p className="mt-1 text-xs text-body/60">{achievement.description}</p>
                <div className="mt-3 h-2 w-full overflow-hidden rounded-full bg-background">
                  <div className="h-full rounded-full bg-primary" style={{ width: `${complete ? 100 : percent}%` }} />
                </div>
              </div>
            );
          })}
          {achievements.length === 0 && <p className="text-sm text-body/60">No achievements defined yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
