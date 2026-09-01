import { useEffect, useState } from 'react';
import { PageHeader, Button, Badge } from '../../components/ui';
import { useMyPortfolio, usePortfolioMutations } from '../../hooks/usePortfolio';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const visibilities = ['Public', 'Private'];

const emptyProject = { title: '', description: '', imageUrl: '', projectUrl: '', completedAt: '' };

export default function Portfolio() {
  const portfolioQuery = useMyPortfolio();
  const { updateMine, updateVisibility, addProject, removeProject } = usePortfolioMutations();
  const [headlineForm, setHeadlineForm] = useState({ headline: '', summary: '' });
  const [showForm, setShowForm] = useState(false);
  const [project, setProject] = useState(emptyProject);

  const portfolio = portfolioQuery.data;

  useEffect(() => {
    if (portfolio) {
      setHeadlineForm({ headline: portfolio.headline || '', summary: portfolio.summary || '' });
    }
  }, [portfolio]);

  const handleSaveHeadline = (event) => {
    event.preventDefault();
    updateMine.mutate(headlineForm);
  };

  const handleAddProject = (event) => {
    event.preventDefault();
    addProject.mutate(
      { ...project, completedAt: project.completedAt || null, displayOrder: portfolio?.projects.length ?? 0 },
      { onSuccess: () => { setShowForm(false); setProject(emptyProject); } },
    );
  };

  if (portfolioQuery.isLoading) return <p className="py-10 text-center text-sm text-body/60">Loading…</p>;
  if (!portfolio) return null;

  return (
    <div>
      <PageHeader
        title="Portfolio"
        description="Showcase the work you've created through your courses and apprenticeships."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Add Work'}</Button>}
      />

      <form onSubmit={handleSaveHeadline} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
        <input placeholder="Headline" value={headlineForm.headline} onChange={(e) => setHeadlineForm((p) => ({ ...p, headline: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
        <textarea rows={2} placeholder="Summary" value={headlineForm.summary} onChange={(e) => setHeadlineForm((p) => ({ ...p, summary: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
        <div className="flex items-center gap-2">
          <span className="text-xs text-body/60">Visibility:</span>
          <select
            value={portfolio.visibility}
            onChange={(e) => updateVisibility.mutate({ visibility: e.target.value })}
            className={inputClass}
          >
            {visibilities.map((v) => <option key={v} value={v}>{v}</option>)}
          </select>
        </div>
        <Button type="submit" variant="secondary" disabled={updateMine.isPending}>{updateMine.isPending ? 'Saving…' : 'Save Headline'}</Button>
      </form>

      {showForm && (
        <form onSubmit={handleAddProject} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={project.title} onChange={(e) => setProject((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required rows={2} placeholder="Description" value={project.description} onChange={(e) => setProject((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Image URL" value={project.imageUrl} onChange={(e) => setProject((p) => ({ ...p, imageUrl: e.target.value }))} className={inputClass} />
          <input placeholder="Project URL" value={project.projectUrl} onChange={(e) => setProject((p) => ({ ...p, projectUrl: e.target.value }))} className={inputClass} />
          <input type="date" value={project.completedAt} onChange={(e) => setProject((p) => ({ ...p, completedAt: e.target.value }))} className={inputClass} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={addProject.isPending}>
            {addProject.isPending ? 'Adding…' : 'Add Project'}
          </Button>
        </form>
      )}

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {portfolio.projects.map((p) => (
          <div key={p.id} className="relative flex flex-col justify-end rounded-xl border border-border bg-surface p-3 text-xs">
            {p.imageUrl && <img src={p.imageUrl} alt={p.title} className="mb-2 aspect-square w-full rounded-lg object-cover" />}
            <p className="font-medium text-heading">{p.title}</p>
            <p className="line-clamp-2 text-body/60">{p.description}</p>
            <button type="button" onClick={() => removeProject.mutate(p.id)} className="mt-2 self-start text-danger hover:underline">Remove</button>
          </div>
        ))}
        {portfolio.projects.length === 0 && (
          <p className="col-span-full text-sm text-body/60">No showcase projects yet. Add your first one above.</p>
        )}
      </div>

      {portfolio.mentorFeedback.length > 0 && (
        <div className="mt-8">
          <h3 className="mb-3 text-sm font-semibold text-heading">Mentor Feedback</h3>
          <div className="space-y-2">
            {portfolio.mentorFeedback.map((f) => (
              <div key={f.id} className="rounded-lg border border-border bg-surface p-3 text-sm">
                <div className="flex items-center justify-between">
                  <span className="font-medium text-heading">{f.mentorName}</span>
                  {f.rating != null && <Badge tone="success">{f.rating}★</Badge>}
                </div>
                <p className="text-xs text-body/60">{f.message}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
