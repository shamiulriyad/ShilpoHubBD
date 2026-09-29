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
    {approved && <section className="space-y-6"><div><p className="text-xs font-semibold uppercase tracking-[.16em] text-primary/70">Mentor workspace</p><h2 className="mt-1 text-xl font-semibold text-heading">Course management</h2><p className="mt-1 text-sm text-body/60">Publish a new course and monitor student capacity from one place.</p></div>
      <form className="grid gap-4 rounded-xl border border-border p-5 sm:grid-cols-2" onSubmit={e => { e.preventDefault(); publish.mutate(); }}>
        {[['title', 'Course title'], ['description', 'What students will learn'], ['category', 'Craft category'], ['venue', 'Offline venue / address']].map(([key, label]) => <label key={key}>{label}<textarea className={input} required={key !== 'venue' || course.deliveryMode !== 'Online'} maxLength={key === 'description' ? 4000 : key === 'venue' ? 500 : 200} value={course[key]} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, [key]: e.target.value }))} /></label>)}
        {[['price', 'Fee (BDT)', 0, 1000000], ['durationDays', 'Duration in days', 1, 365], ['daysPerWeek', 'Class days per week', 1, 7], ['sessionMinutes', 'Minutes per class', 15, 480], ['maxApprentices', 'Maximum students', 1, 10000]].map(([key, label, min, max]) => <label key={key}>{label}<input type="number" step={key === 'price' ? '0.01' : '1'} required min={min} max={max} className={input} value={course[key]} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, [key]: Number(e.target.value) }))} /></label>)}
        <label>Class time (Bangladesh)<input type="time" required className={input} value={course.classTime} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, classTime: e.target.value }))} /></label>
        <label>Available formats<select className={input} value={course.deliveryMode} disabled={!!draftId} onChange={e => setCourse(f => ({ ...f, deliveryMode: e.target.value }))}><option>Both</option><option>Online</option><option>Offline</option></select></label>
        <p>Students reserve a seat and pay the listed fee directly to the mentor. Online payment collection is not connected.</p>
        <MutationFeedback mutation={publish} /><Button type="submit" disabled={publish.isPending}>{draftId ? 'Retry publishing saved draft' : 'Submit and publish course'}</Button>
      </form>
      <div className="flex flex-wrap items-end justify-between gap-3 border-t border-border pt-6">
        <div><h3 className="text-lg font-semibold text-heading">Your courses</h3><p className="mt-1 text-sm text-body/60">{(courses.data || []).length} course{(courses.data || []).length === 1 ? '' : 's'} created</p></div>
        {!!(courses.data || []).length && <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">{(courses.data || []).filter(c => c.status === 'Published').length} published</span>}
      </div>
      {courses.isLoading && <p role="status" className="rounded-xl border border-border bg-surface p-5 text-sm text-body/60">Loading your courses…</p>}
      {courses.isError && <p role="alert" className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">Unable to load courses.</p>}
      <div className="grid gap-4 lg:grid-cols-2">{(courses.data || []).map(c => {
        const enrolled = c.activeEnrollmentCount || 0;
        const capacity = c.maxApprentices || 0;
        const occupancy = capacity ? Math.min(100, Math.round((enrolled / capacity) * 100)) : 0;
        return <Link to={`/academy/courses/${c.id}`} key={c.id} className="group rounded-xl border border-border bg-surface p-5 transition hover:-translate-y-0.5 hover:border-primary/30 hover:shadow-md">
          <div className="flex items-start justify-between gap-4"><div className="min-w-0"><p className="text-[11px] font-semibold uppercase tracking-[.14em] text-primary/65">{c.category || 'Heritage course'}</p><h4 className="mt-1 truncate text-base font-semibold text-heading group-hover:text-primary">{c.title}</h4></div><span className={`shrink-0 rounded-full px-2.5 py-1 text-[11px] font-semibold ${c.status === 'Published' ? 'bg-emerald-50 text-emerald-700' : 'bg-background text-body/70'}`}>{c.status}</span></div>
          <div className="mt-5 grid grid-cols-2 gap-4 border-y border-border py-3 text-sm"><div><p className="text-[11px] uppercase tracking-wide text-body/45">Course fee</p><p className="mt-1 font-semibold text-heading">BDT {Number(c.price || 0).toLocaleString('en-BD')}</p></div><div><p className="text-[11px] uppercase tracking-wide text-body/45">Enrollment</p><p className="mt-1 font-semibold text-heading">{enrolled} of {capacity} students</p></div></div>
          <div className="mt-4"><div className="mb-2 flex justify-between text-[11px] text-body/55"><span>Capacity</span><span>{occupancy}% filled</span></div><div className="h-1.5 overflow-hidden rounded-full bg-background"><div className="h-full rounded-full bg-primary transition-all" style={{ width: `${occupancy}%` }} /></div></div>
          <p className="mt-4 text-xs font-semibold text-primary">View course details <span aria-hidden="true" className="inline-block transition group-hover:translate-x-1">→</span></p>
        </Link>;
      })}</div>
      {courses.isSuccess && !(courses.data || []).length && <div className="rounded-xl border border-dashed border-border bg-surface p-8 text-center"><h3 className="font-semibold text-heading">No courses yet</h3><p className="mt-1 text-sm text-body/60">Use the form above to publish your first heritage course.</p></div>}
    </section>}
  </div>;
}
