import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader, AsyncState } from '../../components/ui';
import { useHeritageSkills } from '../../hooks/useHeritageSkills';
import { useSkillAssessmentHistory, useRunSkillAssessment } from '../../hooks/useSkillAssessments';

export default function SkillAssessments() {
  const skillsQuery = useHeritageSkills();
  const historyQuery = useSkillAssessmentHistory();
  const runAssessment = useRunSkillAssessment();
  const [selectedSkillId, setSelectedSkillId] = useState('');
  const [result, setResult] = useState(null);

  const handleRun = () => {
    runAssessment.mutate(selectedSkillId, { onSuccess: (data) => setResult(data) });
  };

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Academy', path: routePaths.academy },
          { label: 'Skill Assessments' },
        ]}
        title="Skill Assessments"
        description="Assess your proficiency in a heritage craft skill and get personalized recommendations."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <div className="mb-10 flex flex-wrap gap-3 rounded-xl border border-border bg-surface p-5">
        <select
          value={selectedSkillId}
          onChange={(event) => setSelectedSkillId(event.target.value)}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Select a heritage skill…</option>
          {(skillsQuery.data || []).map((skill) => (
            <option key={skill.id} value={skill.id}>
              {skill.name}
            </option>
          ))}
        </select>
        <Button variant="primary" onClick={handleRun} disabled={!selectedSkillId || runAssessment.isPending}>
          {runAssessment.isPending ? 'Assessing…' : 'Run Assessment'}
        </Button>
      </div>

      {result && (
        <div className="mb-10 rounded-xl border border-border bg-surface p-6">
          <div className="flex items-center justify-between">
            <p className="text-sm font-semibold text-heading">{result.heritageSkillName}</p>
            <Badge tone="primary">{result.level}</Badge>
          </div>
          <p className="mt-2 text-2xl font-semibold text-primary">{result.score}</p>
          <p className="mt-2 text-sm text-body/70">{result.summary}</p>
          {result.strengths.length > 0 && (
            <div className="mt-4">
              <p className="text-xs font-semibold uppercase text-body/50">Strengths</p>
              <ul className="mt-1 list-inside list-disc text-sm text-body/70">
                {result.strengths.map((s, i) => <li key={i}>{s}</li>)}
              </ul>
            </div>
          )}
          {result.weaknesses.length > 0 && (
            <div className="mt-4">
              <p className="text-xs font-semibold uppercase text-body/50">Areas to Improve</p>
              <ul className="mt-1 list-inside list-disc text-sm text-body/70">
                {result.weaknesses.map((w, i) => <li key={i}>{w}</li>)}
              </ul>
            </div>
          )}
        </div>
      )}

      <SectionHeader eyebrow="History" title="Past Assessments" />
      <AsyncState isLoading={historyQuery.isLoading} isError={historyQuery.isError} error={historyQuery.error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {(historyQuery.data || []).map((item) => (
            <div key={item.id} className="flex items-center justify-between p-4 text-sm">
              <div>
                <p className="font-medium text-heading">{item.heritageSkillName}</p>
                <p className="text-xs text-body/50">{new Date(item.assessedAt).toLocaleDateString()}</p>
              </div>
              <div className="flex items-center gap-3">
                <span className="font-semibold text-primary">{item.score}</span>
                <Badge tone="secondary">{item.level}</Badge>
              </div>
            </div>
          ))}
          {(historyQuery.data || []).length === 0 && (
            <p className="p-6 text-center text-sm text-body/60">No assessments taken yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
