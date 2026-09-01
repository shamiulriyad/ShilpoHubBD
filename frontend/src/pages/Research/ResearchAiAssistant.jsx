import { useState } from 'react';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useResearchProjects } from '../../hooks/useResearchWorkspace';
import { useResearchAnalyses, useResearchAiMutations } from '../../hooks/useResearchAiAssistant';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Completed: 'success', Failed: 'neutral', Pending: 'secondary', Running: 'primary' };
const citationStyles = ['APA', 'MLA', 'Chicago', 'Harvard'];

export default function ResearchAiAssistant() {
  const projectsQuery = useResearchProjects({ pageSize: 100 });
  const [projectId, setProjectId] = useState('');
  const [title, setTitle] = useState('');
  const [questionsText, setQuestionsText] = useState('');
  const [citationStyle, setCitationStyle] = useState('APA');
  const [expandedId, setExpandedId] = useState(null);

  const analysesQuery = useResearchAnalyses(projectId, { pageSize: 20 });
  const { runInsights, runTrends, runCorrelations, runReport, generateCitations, removeAnalysis } = useResearchAiMutations(projectId);

  const buildRequest = () => ({
    title: title || undefined,
    researchQuestions: questionsText.split('\n').map((q) => q.trim()).filter(Boolean),
  });

  const analyses = analysesQuery.data?.items || [];

  return (
    <div>
      <PageHeader title="Research AI Assistant" description="Generate insights, trend discovery, correlations, reports and citations for a research project." />

      <div className="mb-4 flex flex-wrap items-end gap-2">
        <select value={projectId} onChange={(e) => setProjectId(e.target.value)} className={inputClass}>
          <option value="">Select project…</option>
          {(projectsQuery.data?.items || []).map((p) => <option key={p.id} value={p.id}>{p.title}</option>)}
        </select>
      </div>

      {projectId && (
        <>
          <div className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
            <input placeholder="Analysis title (optional)" value={title} onChange={(e) => setTitle(e.target.value)} className={`${inputClass} sm:col-span-2`} />
            <textarea rows={2} placeholder="Research questions (one per line)" value={questionsText} onChange={(e) => setQuestionsText(e.target.value)} className={`${inputClass} sm:col-span-2`} />
            <div className="flex flex-wrap gap-2 sm:col-span-2">
              <Button variant="primary" disabled={runInsights.isPending} onClick={() => runInsights.mutate(buildRequest())}>
                {runInsights.isPending ? 'Running…' : 'Run Insights'}
              </Button>
              <Button variant="secondary" disabled={runTrends.isPending} onClick={() => runTrends.mutate(buildRequest())}>
                {runTrends.isPending ? 'Running…' : 'Discover Trends'}
              </Button>
              <Button variant="secondary" disabled={runCorrelations.isPending} onClick={() => runCorrelations.mutate(buildRequest())}>
                {runCorrelations.isPending ? 'Running…' : 'Detect Correlations'}
              </Button>
              <Button variant="secondary" disabled={runReport.isPending} onClick={() => runReport.mutate(buildRequest())}>
                {runReport.isPending ? 'Generating…' : 'Generate Report'}
              </Button>
            </div>
            <div className="flex flex-wrap items-end gap-2 sm:col-span-2">
              <select value={citationStyle} onChange={(e) => setCitationStyle(e.target.value)} className={inputClass}>
                {citationStyles.map((s) => <option key={s} value={s}>{s}</option>)}
              </select>
              <Button
                variant="secondary"
                disabled={generateCitations.isPending}
                onClick={() => generateCitations.mutate({ title: title || undefined, style: citationStyle, sources: [], publicationIds: [] })}
              >
                {generateCitations.isPending ? 'Generating…' : 'Generate Citations from Publications'}
              </Button>
            </div>
          </div>

          <div className="space-y-3">
            {analyses.map((a) => (
              <div key={a.id} className="rounded-xl border border-border bg-surface p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <p className="text-sm font-semibold text-heading">{a.title || a.analysisType}</p>
                    <p className="text-xs text-body/60">{a.analysisType} · {a.providerName} · {a.findingCount} finding(s), {a.citationCount} citation(s)</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge tone={statusTone[a.status] || 'neutral'}>{a.status}</Badge>
                    <button type="button" onClick={() => setExpandedId(expandedId === a.id ? null : a.id)} className="text-xs text-primary hover:underline">
                      {expandedId === a.id ? 'Hide' : 'Details'}
                    </button>
                    <button type="button" onClick={() => removeAnalysis.mutate(a.id)} className="text-xs text-danger hover:underline">Delete</button>
                  </div>
                </div>
                {expandedId === a.id && (
                  <div className="mt-3 border-t border-border pt-3 text-xs text-body/70">
                    <p className="mb-2">{a.resultSummary}</p>
                  </div>
                )}
              </div>
            ))}
            {analyses.length === 0 && <p className="text-sm text-body/60">No AI analyses run yet for this project.</p>}
          </div>
        </>
      )}
    </div>
  );
}
