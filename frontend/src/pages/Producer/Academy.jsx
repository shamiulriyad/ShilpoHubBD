import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../services/apiClient';
import { productsService } from '../../services/productsService';
import SafeImage from '../../components/media/SafeImage';
import { resolveMediaUrl } from '../../components/media/CardMedia';
import { Button } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';

const initialCourse = { title: '', description: '', category: '', price: 0, durationDays: 7, daysPerWeek: 2, sessionMinutes: 60, classTime: '10:00', deliveryMode: 'Both', venue: '', maxApprentices: 15 };
export default function ProducerAcademy() {
  const client = useQueryClient();
  const profile = useQuery({ queryKey: ['mentor', 'me'], queryFn: () => apiClient.get('/mentors/me').then(r => r.data), retry: false, refetchInterval: 15000 });
  const approved = profile.data?.approvalStatus === 'Approved';
  const courses = useQuery({ queryKey: ['mentor-courses'], queryFn: () => apiClient.get('/courses/mine').then(r => r.data), enabled: approved });
  const [form, setForm] = useState({ bio: '', expertise: '', yearsOfExperience: 0, location: '', proofImageUrl: '' });
  const [course, setCourse] = useState(initialCourse);
  const [draftId, setDraftId] = useState(null);
  const [lessonAdded, setLessonAdded] = useState(false);
  const upload = useMutation({ mutationFn: productsService.uploadImage, onSuccess: r => setForm(f => ({ ...f, proofImageUrl: r.url })) });
  const apply = useMutation({ mutationFn: () => apiClient.post('/mentors', { ...form, yearsOfExperience: Number(form.yearsOfExperience) }), onSuccess: () => client.invalidateQueries({ queryKey: ['mentor'] }) });
  const publish = useMutation({ mutationFn: async () => {
    let id = draftId;
    if (!id) { const result = await apiClient.post('/courses', course); id = result.data.id; setDraftId(id); }
    if (!lessonAdded) { await apiClient.post(`/courses/${id}/lessons`, { title: 'Course introduction and learning plan', content: course.description, displayOrder: 1 }); setLessonAdded(true); }
    await apiClient.post(`/courses/${id}/publish`);
  }, onSuccess: () => { setDraftId(null); setLessonAdded(false); setCourse(initialCourse); client.invalidateQueries({ queryKey: ['mentor-courses'] }); client.invalidateQueries({ queryKey: ['courses'] }); } });
  const input = 'mt-1 block w-full rounded-lg border border-border bg-background p-3';
  if (profile.isLoading) return <p role="status">Loading your Academy profile…</p>;
  if (profile.isError && profile.error?.response?.status !== 404) return <p role="alert">Unable to load your application. <button onClick={() => profile.refetch()}>Retry</button></p>;
  return <div className="space-y-6">
    <h1 className="text-2xl font-semibold">Academy</h1>
    <p className="text-lg text-primary">Pass your heritage to the next generation—let the craft in your hands become someone else's beginning.</p>
    {profile.data?.approvalStatus === 'Rejected' && <p role="status">Your application needs changes: {profile.data.reviewNote || 'Please review your details and proof.'} You can resubmit below.</p>}
    {profile.data && profile.data.approvalStatus !== 'Rejected' ? <section className="rounded-xl border border-border p-5"><h2 className="font-semibold">Mentor application: {profile.data.approvalStatus}</h2><p>{profile.data.reviewNote || 'Your expertise and photo proof are submitted for admin review.'}</p></section> : <form className="space-y-4 rounded-xl border border-border p-5" onSubmit={e => { e.preventDefault(); apply.mutate(); }}>
      <h2 className="text-xl font-semibold">Want to be a mentor?</h2>
      <p>Your account name is included with the application. Tell us about your experience and upload a photo that demonstrates your craft.</p>
      {[['bio', 'About you'], ['expertise', 'Your expertise'], ['location', 'Location']].map(([key, label]) => <label key={key} className="block">{label}<textarea required maxLength={key === 'bio' ? 2000 : key === 'expertise' ? 500 : 200} className={input} value={form[key]} onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))} /></label>)}
      <label className="block">Years of experience<input type="number" min="0" max="100" required className={input} value={form.yearsOfExperience} onChange={e => setForm(f => ({ ...f, yearsOfExperience: e.target.value }))} /></label>
      <label className="block">Photo proof<input type="file" accept="image/jpeg,image/png,image/webp" required={!form.proofImageUrl} onChange={e => { if (e.target.files?.[0]) upload.mutate(e.target.files[0]); }} /></label>
      {form.proofImageUrl && <SafeImage src={resolveMediaUrl(form.proofImageUrl)} alt="Craft expertise proof" className="h-40 w-40 object-cover rounded-lg" />}
      <MutationFeedback mutation={upload} /><MutationFeedback mutation={apply} />
      <Button type="submit" disabled={apply.isPending || upload.isPending || !form.proofImageUrl}>Submit for admin approval</Button>
    </form>}
    {approved && <section className="space-y-5"><h2 className="text-xl font-semibold">Upload courses</h2>
      <form className="grid gap-4 rounded-xl border border-border p-5 sm:grid-cols-2" onSubmit={e => { e.preventDefault(); publish.mutate(); }}>
        {[['title', 'Course title'], ['description', 'What students will learn'], ['category', 'Craft category'], ['venue', 'Offline venue / address']].map(([key, label]) => <label key={key}>{label}<textarea className={input} required={key !== 'venue' || course.deliveryMode !== 'Online'} maxLength={key === 'description' ? 4000 : key === 'venue' ? 500 : 200} value={course[key]} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, [key]: e.target.value }))} /></label>)}
        {[['price', 'Fee (BDT)', 0, 1000000], ['durationDays', 'Duration in days', 1, 365], ['daysPerWeek', 'Class days per week', 1, 7], ['sessionMinutes', 'Minutes per class', 15, 480], ['maxApprentices', 'Maximum students', 1, 10000]].map(([key, label, min, max]) => <label key={key}>{label}<input type="number" step={key === 'price' ? '0.01' : '1'} required min={min} max={max} className={input} value={course[key]} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, [key]: Number(e.target.value) }))} /></label>)}
        <label>Class time (Bangladesh)<input type="time" required className={input} value={course.classTime} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, classTime: e.target.value }))} /></label>
        <label>Available formats<select className={input} value={course.deliveryMode} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, deliveryMode: e.target.value }))}><option>Both</option><option>Online</option><option>Offline</option></select></label>
        <p>Students reserve a seat and pay the listed fee directly to the mentor. Online payment collection is not connected.</p>
        <MutationFeedback mutation={publish} /><Button type="submit" disabled={publish.isPending}>{draftId ? 'Retry publishing saved draft' : 'Submit and publish course'}</Button>
      </form>
      {courses.isError && <p role="alert">Unable to load courses.</p>}
      {(courses.data || []).map(c => <article key={c.id} className="rounded-lg border border-border p-4"><Link to={`/academy/courses/${c.id}`}>{c.title}</Link><p>{c.status} · BDT {c.price} · {c.activeEnrollmentCount}/{c.maxApprentices} students</p></article>)}
    </section>}
  </div>;
}
