import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../services/apiClient';
import SafeImage from '../../components/media/SafeImage';
import { resolveMediaUrl } from '../../components/media/CardMedia';
import { Button } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { enrichAdminData } from '../../data/adminDemoData';

export default function MentorApplications() {
  const client = useQueryClient();
  const [notes, setNotes] = useState({});
  const list = useQuery({ queryKey: ['mentor-applications'], queryFn: () => apiClient.get('/mentors/applications').then(r => enrichAdminData('/mentors/applications', r.data)) });
  const review = useMutation({ mutationFn: ({ id, approve }) => apiClient.post(`/mentors/${id}/review`, { approve, note: notes[id] || null }), onSuccess: () => client.invalidateQueries({ queryKey: ['mentor-applications'] }) });
  return <div className="space-y-5"><h1 className="text-2xl font-semibold">Mentor applications</h1><p>Review each producer's experience and photo proof before approving them to teach.</p>
    {list.isLoading && <p role="status">Loading applications…</p>}{list.isError && <p role="alert">Unable to load applications.</p>}
    <MutationFeedback mutation={review} />
    {list.data?.length === 0 && <p>No applications yet.</p>}
    {(list.data || []).map(m => <article key={m.id} className="space-y-3 rounded-xl border border-border p-5"><h2 className="text-lg font-semibold">{m.fullName} · {m.approvalStatus}</h2><p>{m.bio}</p><p>{m.expertise} · {m.yearsOfExperience} years · {m.location}</p><SafeImage src={resolveMediaUrl(m.proofImageUrl)} alt={`${m.fullName}'s expertise proof`} className="max-h-80 max-w-full object-contain" />
      {m.approvalStatus === 'Pending' && !m.isDemo ? <><label className="block">Review note<textarea className="block w-full rounded border border-border p-3" maxLength={2000} value={notes[m.id] || ''} onChange={e => setNotes(n => ({ ...n, [m.id]: e.target.value }))} /></label><div className="flex gap-3"><Button disabled={review.isPending || !m.proofImageUrl} onClick={() => review.mutate({ id: m.id, approve: true })}>Approve mentor</Button><Button variant="secondary" disabled={review.isPending} onClick={() => review.mutate({ id: m.id, approve: false })}>Reject application</Button></div></> : <p>{m.reviewNote || (m.isDemo ? 'Preview record — actions are disabled.' : '')}</p>}
    </article>)}
  </div>;
}
