import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useSurveys, useSurvey, useSurveyMutations, useSurveyResponses, useSurveyEvidence, useSurveyWorkItemMutations,
} from '../../hooks/useFieldResearch';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Draft: 'neutral', Open: 'success', Closed: 'neutral', Archived: 'neutral' };
const tabs = ['Questions', 'Field Researchers', 'Responses', 'Evidence'];
const questionTypes = ['ShortText', 'LongText', 'Number', 'SingleChoice', 'MultiChoice', 'Rating', 'Location'];
const evidenceTypes = ['Photo', 'Audio', 'Video', 'Document', 'Transcript'];

function QuestionsTab({ survey }) {
  const { addQuestion, removeQuestion } = useSurveyMutations();
  const [form, setForm] = useState({ text: '', questionType: 'ShortText', isRequired: true });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.text) return;
    addQuestion.mutate(
      { id: survey.id, payload: { ...form, orderIndex: survey.questions.length } },
      { onSuccess: () => setForm({ text: '', questionType: 'ShortText', isRequired: true }) },
    );
  };

  return (
    <div>
      <div className="mb-3 space-y-2">
        {survey.questions.map((q) => (
          <div key={q.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <span>{q.text} ({q.questionType}){q.isRequired ? ' *' : ''}</span>
            <button type="button" onClick={() => removeQuestion.mutate({ id: survey.id, questionId: q.id })} className="text-xs text-danger hover:underline">Remove</button>
          </div>
        ))}
        {survey.questions.length === 0 && <p className="text-sm text-body/60">No questions yet.</p>}
      </div>
      <form onSubmit={handleAdd} className="flex flex-wrap gap-2">
        <input placeholder="Question text" value={form.text} onChange={(e) => setForm((p) => ({ ...p, text: e.target.value }))} className={`${inputClass} flex-1`} />
        <select value={form.questionType} onChange={(e) => setForm((p) => ({ ...p, questionType: e.target.value }))} className={inputClass}>
          {questionTypes.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <label className="flex items-center gap-1 text-xs text-body/60">
          <input type="checkbox" checked={form.isRequired} onChange={(e) => setForm((p) => ({ ...p, isRequired: e.target.checked }))} /> Required
        </label>
        <Button type="submit" variant="secondary" size="sm" disabled={addQuestion.isPending}>Add</Button>
      </form>
    </div>
  );
}

function FieldResearchersTab({ survey }) {
  const { assignFieldResearcher, removeFieldResearcher } = useSurveyMutations();
  const [userId, setUserId] = useState('');

  return (
    <div>
      <div className="mb-3 space-y-2">
        {survey.fieldAssignments.map((a) => (
          <div key={a.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <span>{a.fieldResearcherName} ({a.role})</span>
            <button type="button" onClick={() => removeFieldResearcher.mutate({ id: survey.id, assignmentId: a.id })} className="text-xs text-danger hover:underline">Remove</button>
          </div>
        ))}
        {survey.fieldAssignments.length === 0 && <p className="text-sm text-body/60">No field researchers assigned yet.</p>}
      </div>
      <div className="flex gap-2">
        <input placeholder="User ID" value={userId} onChange={(e) => setUserId(e.target.value)} className={`${inputClass} flex-1`} />
        <Button
          variant="secondary"
          size="sm"
          disabled={!userId || assignFieldResearcher.isPending}
          onClick={() => assignFieldResearcher.mutate({ id: survey.id, payload: { fieldResearcherUserId: userId } }, { onSuccess: () => setUserId('') })}
        >
          Assign
        </Button>
      </div>
    </div>
  );
}

function ResponsesTab({ surveyId }) {
  const responsesQuery = useSurveyResponses(surveyId, { pageSize: 50 });
  const { reviewResponse } = useSurveyWorkItemMutations(surveyId);

  return (
    <div className="space-y-2">
      {(responsesQuery.data?.items || []).map((r) => (
        <div key={r.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-surface px-3 py-2 text-sm">
          <span>{r.respondentName || 'Anonymous'} · {r.villageName || r.districtName || '—'} · {r.answerCount} answer(s)</span>
          <div className="flex items-center gap-2">
            <Badge tone={r.status === 'Approved' ? 'success' : 'neutral'}>{r.status}</Badge>
            {r.status === 'Submitted' && (
              <>
                <button type="button" onClick={() => reviewResponse.mutate({ responseId: r.id, payload: { decision: 'Approve' } })} className="text-xs text-primary hover:underline">Approve</button>
                <button type="button" onClick={() => reviewResponse.mutate({ responseId: r.id, payload: { decision: 'Reject' } })} className="text-xs text-danger hover:underline">Reject</button>
              </>
            )}
          </div>
        </div>
      ))}
      {(responsesQuery.data?.items || []).length === 0 && <p className="text-sm text-body/60">No responses collected yet.</p>}
    </div>
  );
}

function EvidenceTab({ surveyId }) {
  const evidenceQuery = useSurveyEvidence(surveyId, { pageSize: 50 });
  const { createEvidence, removeEvidence } = useSurveyWorkItemMutations(surveyId);
  const [form, setForm] = useState({ evidenceType: 'Photo', title: '', fileUrl: '' });

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title) return;
    createEvidence.mutate(form, { onSuccess: () => setForm({ evidenceType: 'Photo', title: '', fileUrl: '' }) });
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 flex flex-wrap gap-2">
        <select value={form.evidenceType} onChange={(e) => setForm((p) => ({ ...p, evidenceType: e.target.value }))} className={inputClass}>
          {evidenceTypes.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
        <input placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} flex-1`} />
        <input placeholder="File URL" value={form.fileUrl} onChange={(e) => setForm((p) => ({ ...p, fileUrl: e.target.value }))} className={`${inputClass} flex-1`} />
        <Button type="submit" variant="secondary" size="sm" disabled={createEvidence.isPending}>Add</Button>
      </form>
      <div className="space-y-2">
        {(evidenceQuery.data?.items || []).map((ev) => (
          <div key={ev.id} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <span>{ev.title} ({ev.evidenceType}) · {ev.capturedByName}</span>
            <button type="button" onClick={() => removeEvidence.mutate(ev.id)} className="text-xs text-danger hover:underline">Remove</button>
          </div>
        ))}
        {(evidenceQuery.data?.items || []).length === 0 && <p className="text-sm text-body/60">No evidence captured yet.</p>}
      </div>
    </div>
  );
}

function SurveyDetail({ id }) {
  const detailQuery = useSurvey(id);
  const { updateStatus } = useSurveyMutations();
  const [tab, setTab] = useState('Questions');

  const survey = detailQuery.data;
  if (detailQuery.isLoading) return <p className="py-4 text-sm text-body/60">Loading…</p>;
  if (!survey) return null;

  return (
    <div className="mt-4 border-t border-border pt-4">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs text-body/60">{survey.responseCount} response(s) · {survey.evidenceCount} evidence item(s)</p>
        <select value={survey.status} onChange={(e) => updateStatus.mutate({ id, payload: { status: e.target.value } })} className={inputClass}>
          {['Draft', 'Open', 'Closed', 'Archived'].map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
      </div>
      <div className="mb-4 flex flex-wrap gap-2 border-b border-border">
        {tabs.map((t) => (
          <button key={t} type="button" onClick={() => setTab(t)} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === t ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>
            {t}
          </button>
        ))}
      </div>
      {tab === 'Questions' && <QuestionsTab survey={survey} />}
      {tab === 'Field Researchers' && <FieldResearchersTab survey={survey} />}
      {tab === 'Responses' && <ResponsesTab surveyId={id} />}
      {tab === 'Evidence' && <EvidenceTab surveyId={id} />}
    </div>
  );
}

export default function FieldResearch() {
  const { data, isLoading, isError, error } = useSurveys({ pageSize: 50 });
  const { create } = useSurveyMutations();
  const [showForm, setShowForm] = useState(false);
  const [selectedId, setSelectedId] = useState(null);
  const [form, setForm] = useState({ title: '', description: '', targetRegion: '', allowAnonymousResponses: true });

  const surveys = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, {
      onSuccess: (result) => { setShowForm(false); setForm({ title: '', description: '', targetRegion: '', allowAnonymousResponses: true }); setSelectedId(result.id); },
    });
  };

  return (
    <div>
      <PageHeader
        title="Field Research"
        description="Design surveys, assign field researchers, and collect responses and evidence from heritage villages."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Survey'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Target region" value={form.targetRegion} onChange={(e) => setForm((p) => ({ ...p, targetRegion: e.target.value }))} className={inputClass} />
          <label className="flex items-center gap-2 text-sm text-body/70">
            <input type="checkbox" checked={form.allowAnonymousResponses} onChange={(e) => setForm((p) => ({ ...p, allowAnonymousResponses: e.target.checked }))} /> Allow anonymous responses
          </label>
          <Button type="submit" variant="primary" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create Survey'}</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {surveys.map((s) => (
            <div key={s.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{s.title}</p>
                  <p className="text-xs text-body/60">{s.targetRegion || 'All regions'} · {s.questionCount} question(s) · {s.responseCount} response(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[s.status] || 'neutral'}>{s.status}</Badge>
                  <Button variant="secondary" onClick={() => setSelectedId(selectedId === s.id ? null : s.id)}>
                    {selectedId === s.id ? 'Hide' : 'Open'}
                  </Button>
                </div>
              </div>
              {selectedId === s.id && <SurveyDetail id={s.id} />}
            </div>
          ))}
          {surveys.length === 0 && <p className="text-sm text-body/60">No surveys yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
