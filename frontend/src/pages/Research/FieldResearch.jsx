import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useSurveys, useSurvey, useSurveyMutations, useSurveyResponses, useSurveyEvidence, useSurveyWorkItemMutations,
} from '../../hooks/useFieldResearch';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Draft: 'neutral', Open: 'success', Closed: 'neutral', Archived: 'neutral' };
const tabs = ['Questions', 'Field Researchers', 'Responses', 'Evidence'];
const questionTypes = ['ShortText', 'LongText', 'Number', 'SingleChoice', 'MultiChoice', 'Rating', 'Location'];
const evidenceTypes = [
  { value: 'Photo', label: 'Photo' },
  { value: 'AudioRecording', label: 'Voice / Audio recording' },
  { value: 'VideoRecording', label: 'Video recording' },
  { value: 'InterviewTranscript', label: 'Interview transcript' },
  { value: 'Document', label: 'Document' },
  { value: 'GpsWaypoint', label: 'GPS waypoint' },
  { value: 'Note', label: 'Note' },
];
const emptyEvidence = { evidenceType: 'Photo', title: '', fileUrl: '', transcriptText: '', language: '', durationSeconds: '', latitude: '', longitude: '' };

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
        <input aria-label="Question text" placeholder="Question text" value={form.text} onChange={(e) => setForm((p) => ({ ...p, text: e.target.value }))} className={`${inputClass} flex-1`} />
        <select aria-label="Question Type" value={form.questionType} onChange={(e) => setForm((p) => ({ ...p, questionType: e.target.value }))} className={inputClass}>
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
        <input aria-label="User ID" placeholder="User ID" value={userId} onChange={(e) => setUserId(e.target.value)} className={`${inputClass} flex-1`} />
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
  const [form, setForm] = useState(emptyEvidence);
  const [geoError, setGeoError] = useState('');
  const set = (key) => (e) => setForm((p) => ({ ...p, [key]: e.target.value }));
  const isAudio = form.evidenceType === 'AudioRecording' || form.evidenceType === 'VideoRecording';
  const isTranscript = form.evidenceType === 'InterviewTranscript' || isAudio;

  const captureLocation = () => {
    setGeoError('');
    if (!navigator.geolocation) { setGeoError('Geolocation is not supported in this browser.'); return; }
    navigator.geolocation.getCurrentPosition(
      (pos) => setForm((p) => ({ ...p, latitude: pos.coords.latitude.toFixed(6), longitude: pos.coords.longitude.toFixed(6), locationAccuracyMeters: pos.coords.accuracy })),
      () => setGeoError('Unable to read your location. Enter coordinates manually.'),
      { enableHighAccuracy: true, timeout: 10000 },
    );
  };

  const handleAdd = (e) => {
    e.preventDefault();
    if (!form.title) return;
    const num = (v) => (v === '' || v == null ? undefined : Number(v));
    const payload = {
      evidenceType: form.evidenceType,
      title: form.title,
      fileUrl: form.fileUrl || undefined,
      transcriptText: form.transcriptText || undefined,
      language: form.language || undefined,
      durationSeconds: num(form.durationSeconds),
      latitude: num(form.latitude),
      longitude: num(form.longitude),
      locationAccuracyMeters: num(form.locationAccuracyMeters),
    };
    createEvidence.mutate(payload, { onSuccess: () => setForm(emptyEvidence) });
  };

  return (
    <div>
      <form onSubmit={handleAdd} className="mb-3 grid gap-2 rounded-xl border border-border bg-surface p-3 sm:grid-cols-2">
        <select aria-label="Evidence Type" value={form.evidenceType} onChange={set('evidenceType')} className={inputClass}>
          {evidenceTypes.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
        </select>
        <input aria-label="Title" placeholder="Title" value={form.title} onChange={set('title')} className={inputClass} />
        <input aria-label="File URL" placeholder="File URL" value={form.fileUrl} onChange={set('fileUrl')} className={`${inputClass} sm:col-span-2`} />
        {isAudio && <input aria-label="Duration (seconds)" type="number" min="0" placeholder="Duration (seconds)" value={form.durationSeconds} onChange={set('durationSeconds')} className={inputClass} />}
        {isTranscript && <input aria-label="Language" placeholder="Language (e.g. bn, en)" value={form.language} onChange={set('language')} className={inputClass} />}
        {isTranscript && <textarea aria-label="Transcript" rows={3} placeholder="Transcript / voice documentation text" value={form.transcriptText} onChange={set('transcriptText')} className={`${inputClass} sm:col-span-2`} />}
        <input aria-label="Latitude" type="number" step="any" min="-90" max="90" placeholder="Latitude" value={form.latitude} onChange={set('latitude')} className={inputClass} />
        <input aria-label="Longitude" type="number" step="any" min="-180" max="180" placeholder="Longitude" value={form.longitude} onChange={set('longitude')} className={inputClass} />
        <div className="flex items-center gap-2 sm:col-span-2">
          <Button type="button" variant="secondary" size="sm" onClick={captureLocation}>Use my location (GPS)</Button>
          <Button type="submit" variant="primary" size="sm" disabled={createEvidence.isPending}>Add evidence</Button>
          {geoError && <span role="alert" className="text-xs text-danger">{geoError}</span>}
        </div>
      </form>
      <div className="space-y-2">
        {(evidenceQuery.data?.items || []).map((ev) => (
          <div key={ev.id} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm">
            <div className="flex items-center justify-between">
              <span>{ev.title} ({ev.evidenceType}) · {ev.capturedByName}</span>
              <button type="button" onClick={() => removeEvidence.mutate(ev.id)} className="text-xs text-danger hover:underline">Remove</button>
            </div>
            {(ev.latitude != null && ev.longitude != null) && <p className="text-xs text-body/60">GPS: {ev.latitude}, {ev.longitude}</p>}
            {ev.transcriptText && <p className="mt-1 line-clamp-3 text-xs text-body/70">{ev.transcriptText}</p>}
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
  if (detailQuery.isError) return <p role="alert" className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">Unable to load this record. It may have been removed or you may not have access.</p>;
  if (!survey) return <p className="py-4 text-sm text-body/60">This record is unavailable.</p>;

  return (
    <div className="mt-4 border-t border-border pt-4">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs text-body/60">{survey.responseCount} response(s) · {survey.evidenceCount} evidence item(s)</p>
        <select aria-label="Status" value={survey.status} onChange={(e) => updateStatus.mutate({ id, payload: { status: e.target.value } })} className={inputClass}>
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
          <input aria-label="Title" required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea aria-label="Description" required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input aria-label="Target region" placeholder="Target region" value={form.targetRegion} onChange={(e) => setForm((p) => ({ ...p, targetRegion: e.target.value }))} className={inputClass} />
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
