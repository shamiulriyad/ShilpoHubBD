import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useHeritageSkills } from '../../hooks/useHeritageSkills';
import { useActiveRoadmap, useRoadmapHistory, useLearningRoadmapMutations } from '../../hooks/useLearningRoadmaps';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

export default function LearningRoadmap() {
  const activeQuery = useActiveRoadmap();
  const historyQuery = useRoadmapHistory();
  const skillsQuery = useHeritageSkills();
  const { create, refreshProgress, completeMilestone } = useLearningRoadmapMutations();
  const [form, setForm] = useState({ goal: '', targetHeritageSkillId: '' });

  const roadmap = activeQuery.data;

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate({ goal: form.goal, targetHeritageSkillId: form.targetHeritageSkillId || null }, { onSuccess: () => setForm({ goal: '', targetHeritageSkillId: '' }) });
  };

  return (
    <div>
      <PageHeader title="Learning Roadmap" description="An AI-generated path of skills, courses and lessons toward your goal." />

      <AsyncState isLoading={activeQuery.isLoading} isError={false}>
        {roadmap ? (
          <div className="rounded-xl border border-border bg-surface p-6">
            <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="text-sm font-semibold text-heading">{roadmap.goal}</p>
                {roadmap.targetHeritageSkillName && <p className="text-xs text-body/60">Target skill: {roadmap.targetHeritageSkillName}</p>}
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={roadmap.status === 'Completed' ? 'success' : 'secondary'}>{roadmap.status}</Badge>
                <Button size="sm" variant="secondary" disabled={refreshProgress.isPending} onClick={() => refreshProgress.mutate(roadmap.id)}>
                  {refreshProgress.isPending ? 'Refreshing…' : 'Refresh Progress'}
                </Button>
              </div>
            </div>

            <div className="mb-4 h-2 w-full overflow-hidden rounded-full bg-background">
              <div className="h-full rounded-full bg-primary" style={{ width: `${roadmap.progressPercent}%` }} />
            </div>
            <p className="mb-4 text-xs text-body/60">{roadmap.completedMilestoneCount}/{roadmap.totalMilestoneCount} milestones complete ({roadmap.progressPercent}%)</p>

            <div className="space-y-3">
              {roadmap.milestones.map((m) => (
                <div key={m.id} className={`rounded-lg border p-3 ${m.isCompleted ? 'border-success/30 bg-success/5' : 'border-border'}`}>
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-heading">{m.heritageSkillName} → {m.targetLevel}</span>
                    {m.isCompleted ? (
                      <Badge tone="success">Done</Badge>
                    ) : (
                      <button type="button" onClick={() => completeMilestone.mutate({ id: roadmap.id, milestoneId: m.id })} className="text-xs text-primary hover:underline">
                        Mark complete
                      </button>
                    )}
                  </div>
                  {m.recommendedCourses.length > 0 && (
                    <p className="mt-1 text-xs text-body/60">Courses: {m.recommendedCourses.map((c) => c.courseTitle).join(', ')}</p>
                  )}
                  {m.recommendedLessons.length > 0 && (
                    <p className="mt-1 text-xs text-body/60">Lessons: {m.recommendedLessons.map((l) => l.lessonTitle).join(', ')}</p>
                  )}
                </div>
              ))}
            </div>
          </div>
        ) : (
          <form onSubmit={handleCreate} className="grid gap-3 rounded-xl border border-border bg-surface p-6 sm:grid-cols-2">
            <p className="text-sm text-body/60 sm:col-span-2">No active roadmap yet. Set a goal to generate one.</p>
            <input required placeholder="Goal (e.g. Become a master weaver)" value={form.goal} onChange={(e) => setForm((p) => ({ ...p, goal: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
            <select value={form.targetHeritageSkillId} onChange={(e) => setForm((p) => ({ ...p, targetHeritageSkillId: e.target.value }))} className={inputClass}>
              <option value="">Target skill (optional)</option>
              {(skillsQuery.data || []).map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
            </select>
            <Button type="submit" variant="primary" disabled={create.isPending}>{create.isPending ? 'Generating…' : 'Generate Roadmap'}</Button>
          </form>
        )}
      </AsyncState>

      {(historyQuery.data || []).length > 0 && (
        <div className="mt-8">
          <h3 className="mb-3 text-sm font-semibold text-heading">Past Roadmaps</h3>
          <div className="space-y-2">
            {historyQuery.data.map((r) => (
              <div key={r.id} className="flex items-center justify-between rounded-lg border border-border bg-surface p-3 text-sm">
                <span>{r.goal}</span>
                <Badge tone={r.status === 'Completed' ? 'success' : 'neutral'}>{r.status} · {r.progressPercent}%</Badge>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
