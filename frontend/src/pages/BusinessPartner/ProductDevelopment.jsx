import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useMyDevelopmentProjects, useProductDevelopmentMutations, useDevelopmentProject } from '../../hooks/useProductDevelopment';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';

const statusTone = { Requested: 'secondary', Active: 'primary', Declined: 'neutral', Approved: 'success', Converted: 'success', Cancelled: 'neutral' };

function ProjectPanel({ id }) {
  const { data: project } = useDevelopmentProject(id);
  const { addComment, decidePrototype, convertToProduct } = useProductDevelopmentMutations();
  const [comment, setComment] = useState('');
  const categoriesQuery = useCategories();
  const districtsQuery = useDistricts();
  const [convertForm, setConvertForm] = useState({ categoryId: '', districtId: '', price: '', initialStock: '' });

  if (!project) return null;

  return (
    <div className="mt-4 space-y-3 border-t border-border pt-4">
      <div className="space-y-2">
        {(project.comments || []).map((c) => (
          <p key={c.id} className="text-sm text-body/70"><span className="font-medium text-heading">{c.authorName}:</span> {c.content}</p>
        ))}
      </div>
      <div className="flex gap-2">
        <input placeholder="Add a comment…" value={comment} onChange={(e) => setComment(e.target.value)} className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <Button variant="secondary" onClick={() => { addComment.mutate({ id: project.id, content: comment }); setComment(''); }}>Comment</Button>
      </div>

      {(project.prototypeVersions || []).filter((v) => v.status === 'Pending').map((v) => (
        <div key={v.id} className="flex items-center justify-between rounded-lg border border-border bg-background p-3 text-sm">
          <span>Prototype v{v.versionNumber}: {v.description}</span>
          <div className="flex gap-2">
            <Button variant="primary" onClick={() => decidePrototype.mutate({ id: project.id, prototypeVersionId: v.id, payload: { status: 'Approved' } })}>Approve</Button>
            <Button variant="secondary" onClick={() => decidePrototype.mutate({ id: project.id, prototypeVersionId: v.id, payload: { status: 'Rejected' } })}>Reject</Button>
          </div>
        </div>
      ))}

      {project.status === 'Approved' && (
        <div className="space-y-2 rounded-lg border border-border bg-background p-3">
          <p className="text-sm font-semibold text-heading">Convert to Product</p>
          <div className="grid gap-2 sm:grid-cols-2">
            <select value={convertForm.categoryId} onChange={(e) => setConvertForm((p) => ({ ...p, categoryId: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
              <option value="">Category</option>
              {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <select value={convertForm.districtId} onChange={(e) => setConvertForm((p) => ({ ...p, districtId: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
              <option value="">District</option>
              {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
            <input type="number" placeholder="Price" value={convertForm.price} onChange={(e) => setConvertForm((p) => ({ ...p, price: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input type="number" placeholder="Initial stock" value={convertForm.initialStock} onChange={(e) => setConvertForm((p) => ({ ...p, initialStock: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>
          <Button
            variant="primary"
            onClick={() => convertToProduct.mutate({ id: project.id, payload: { categoryId: convertForm.categoryId, districtId: convertForm.districtId, price: Number(convertForm.price), initialStock: Number(convertForm.initialStock) } })}
          >
            Convert to Product
          </Button>
        </div>
      )}
    </div>
  );
}

export default function ProductDevelopment() {
  const { data, isLoading, isError, error } = useMyDevelopmentProjects({ pageSize: 50 });
  const { create } = useProductDevelopmentMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ producerId: '', title: '', businessRequirements: '', productSpecifications: '' });

  const projects = data?.items || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(form, { onSuccess: () => setShowForm(false) });
  };

  return (
    <div>
      <PageHeader
        title="Product Development"
        description="Collaborate with producers to develop a brand-new product."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Project'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input required placeholder="Producer ID" value={form.producerId} onChange={(e) => setForm((p) => ({ ...p, producerId: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={2} placeholder="Business requirements" value={form.businessRequirements} onChange={(e) => setForm((p) => ({ ...p, businessRequirements: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={2} placeholder="Product specifications" value={form.productSpecifications} onChange={(e) => setForm((p) => ({ ...p, productSpecifications: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button type="submit" variant="primary" disabled={create.isPending}>Send Request</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {projects.map((project) => (
            <div key={project.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{project.title}</p>
                  <p className="text-xs text-body/60">{project.producerName} · {project.prototypeVersionCount} prototypes</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[project.status] || 'neutral'}>{project.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === project.id ? null : project.id)}>
                    {expandedId === project.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>
              {expandedId === project.id && <ProjectPanel id={project.id} />}
            </div>
          ))}
          {projects.length === 0 && <p className="text-sm text-body/60">You haven't started any product development projects yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
