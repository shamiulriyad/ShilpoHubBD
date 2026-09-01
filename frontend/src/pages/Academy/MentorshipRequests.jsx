import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMentors } from '../../hooks/useMentors';
import { useHeritageSkills } from '../../hooks/useHeritageSkills';
import { useMyMentorshipRequestsAsLearner, useMyMentorshipRequestsAsMentor, useMentorshipRequestMutations } from '../../hooks/useMentorshipRequests';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const statusTone = { Pending: 'secondary', Accepted: 'primary', Rejected: 'neutral', Completed: 'success' };

export default function MentorshipRequests() {
  const [tab, setTab] = useState('learner');
  const asLearnerQuery = useMyMentorshipRequestsAsLearner();
  const asMentorQuery = useMyMentorshipRequestsAsMentor();
  const mentorsQuery = useMentors({ pageSize: 100 });
  const skillsQuery = useHeritageSkills();
  const { create, accept, reject, complete } = useMentorshipRequestMutations();
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ mentorProfileId: '', heritageSkillId: '', message: '' });

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { ...form, heritageSkillId: form.heritageSkillId || null },
      { onSuccess: () => { setShowForm(false); setForm({ mentorProfileId: '', heritageSkillId: '', message: '' }); } },
    );
  };

  const list = tab === 'learner' ? asLearnerQuery : asMentorQuery;
  const items = list.data || [];

  return (
    <div>
      <PageHeader
        title="Mentorship Requests"
        description="Request guidance from master artisans, or manage requests from your own mentees."
        action={tab === 'learner' && <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Request Mentorship'}</Button>}
      />

      <div className="mb-4 flex gap-2 border-b border-border">
        <button type="button" onClick={() => setTab('learner')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'learner' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>As Learner</button>
        <button type="button" onClick={() => setTab('mentor')} className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === 'mentor' ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}>As Mentor</button>
      </div>

      {showForm && tab === 'learner' && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <select required value={form.mentorProfileId} onChange={(e) => setForm((p) => ({ ...p, mentorProfileId: e.target.value }))} className={inputClass}>
            <option value="">Select mentor</option>
            {(mentorsQuery.data?.items || []).map((m) => <option key={m.id} value={m.id}>{m.name || m.fullName}</option>)}
          </select>
          <select value={form.heritageSkillId} onChange={(e) => setForm((p) => ({ ...p, heritageSkillId: e.target.value }))} className={inputClass}>
            <option value="">Skill (optional)</option>
            {(skillsQuery.data || []).map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
          <textarea required rows={3} placeholder="Message" value={form.message} onChange={(e) => setForm((p) => ({ ...p, message: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending}>
            {create.isPending ? 'Sending…' : 'Send Request'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={list.isLoading} isError={list.isError} error={list.error}>
        <div className="space-y-3">
          {items.map((r) => (
            <div key={r.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{tab === 'learner' ? r.mentorName : r.learnerName}</p>
                  <p className="text-xs text-body/60">{r.heritageSkillName || 'General mentorship'} · {new Date(r.requestedAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[r.status] || 'neutral'}>{r.status}</Badge>
                  {tab === 'mentor' && r.status === 'Pending' && (
                    <>
                      <Button size="sm" variant="primary" disabled={accept.isPending} onClick={() => accept.mutate({ id: r.id, payload: {} })}>Accept</Button>
                      <Button size="sm" variant="secondary" disabled={reject.isPending} onClick={() => reject.mutate({ id: r.id, payload: {} })}>Reject</Button>
                    </>
                  )}
                  {r.status === 'Accepted' && (
                    <Button size="sm" variant="secondary" disabled={complete.isPending} onClick={() => complete.mutate(r.id)}>Mark Complete</Button>
                  )}
                </div>
              </div>
            </div>
          ))}
          {items.length === 0 && <p className="text-sm text-body/60">No mentorship requests {tab === 'learner' ? 'sent' : 'received'} yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
