import { useState } from 'react';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useHeritageSkills } from '../../hooks/useHeritageSkills';
import { useMentorMatch } from '../../hooks/useMentorMatching';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const skillLevels = ['Beginner', 'Intermediate', 'Advanced', 'Expert'];

export default function MentorMatching() {
  const skillsQuery = useHeritageSkills();
  const match = useMentorMatch();
  const [form, setForm] = useState({
    heritageSkillId: '', minSkillLevel: '', learningGoalKeyword: '', location: '', minYearsOfExperience: '',
  });

  const handleSearch = (event) => {
    event.preventDefault();
    match.mutate({
      ...form,
      heritageSkillId: form.heritageSkillId || null,
      minSkillLevel: form.minSkillLevel || null,
      minYearsOfExperience: form.minYearsOfExperience === '' ? null : Number(form.minYearsOfExperience),
      maxResults: 10,
    });
  };

  const results = match.data || [];

  return (
    <div>
      <PageHeader title="AI Mentor Matching" description="Find the best-matched mentors for your learning goals, skill and location." />

      <form onSubmit={handleSearch} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
        <select value={form.heritageSkillId} onChange={(e) => setForm((p) => ({ ...p, heritageSkillId: e.target.value }))} className={inputClass}>
          <option value="">Any skill</option>
          {(skillsQuery.data || []).map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
        </select>
        <select value={form.minSkillLevel} onChange={(e) => setForm((p) => ({ ...p, minSkillLevel: e.target.value }))} className={inputClass}>
          <option value="">Any level</option>
          {skillLevels.map((l) => <option key={l} value={l}>{l}</option>)}
        </select>
        <input placeholder="Learning goal keyword" value={form.learningGoalKeyword} onChange={(e) => setForm((p) => ({ ...p, learningGoalKeyword: e.target.value }))} className={inputClass} />
        <input placeholder="Location" value={form.location} onChange={(e) => setForm((p) => ({ ...p, location: e.target.value }))} className={inputClass} />
        <input type="number" min="0" placeholder="Min years of experience" value={form.minYearsOfExperience} onChange={(e) => setForm((p) => ({ ...p, minYearsOfExperience: e.target.value }))} className={inputClass} />
        <Button type="submit" variant="primary" disabled={match.isPending}>{match.isPending ? 'Matching…' : 'Find Mentors'}</Button>
      </form>

      {match.isSuccess && (
        <div className="space-y-3">
          {results.map((r) => (
            <div key={r.mentorProfileId} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{r.fullName}</p>
                  <p className="text-xs text-body/60">{r.expertise} · {r.yearsOfExperience}y experience{r.location ? ` · ${r.location}` : ''}</p>
                </div>
                <Badge tone="success">{Math.round(r.matchScore)}% match</Badge>
              </div>
              {r.bio && <p className="mt-2 text-xs text-body/70">{r.bio}</p>}
              {r.matchReasons.length > 0 && (
                <p className="mt-2 text-xs text-body/50">Why: {r.matchReasons.join(' · ')}</p>
              )}
            </div>
          ))}
          {results.length === 0 && <p className="text-sm text-body/60">No matching mentors found — try broadening your filters.</p>}
        </div>
      )}
    </div>
  );
}
