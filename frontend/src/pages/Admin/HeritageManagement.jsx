import { useState } from 'react';
import { PageHeader, Button, Badge, AsyncState, SectionHeader } from '../../components/ui';
import { useDistricts, useDistrictMutations } from '../../hooks/useDistricts';
import { useVillages, useCreateVillage, useVillageMutations } from '../../hooks/useVillages';
import { useCategories, useCategoryMutations } from '../../hooks/useCategories';
import { useHeritageFestivals, useHeritageFestivalMutations } from '../../hooks/useHeritageFestivals';
import { useUnescoRecords, useUnescoRecordMutations } from '../../hooks/useUnescoRecords';
import { useHeritageIdentity, useVerifyHeritageIdentity } from '../../hooks/useHeritageIdentity';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const unescoTypes = ['CulturalHeritageSite', 'NaturalHeritageSite', 'IntangibleCulturalHeritage', 'MemoryOfTheWorld'];

function toDateInput(value) {
  return value ? value.slice(0, 10) : '';
}

// ---- Categories ----------------------------------------------------------

const emptyCategoryForm = { name: '', description: '', imageUrl: '', displayOrder: 0, isActive: true };

function CategoriesTab() {
  const { data, isLoading, isError, error } = useCategories();
  const { create, update, remove } = useCategoryMutations();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyCategoryForm);

  const categories = data || [];

  const startCreate = () => { setEditingId(null); setForm(emptyCategoryForm); setShowForm(true); };
  const startEdit = (c) => {
    setEditingId(c.id);
    setForm({ name: c.name, description: c.description || '', imageUrl: c.imageUrl || '', displayOrder: c.displayOrder, isActive: c.isActive });
    setShowForm(true);
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    if (editingId) {
      update.mutate({ id: editingId, payload: form }, { onSuccess: () => setShowForm(false) });
    } else {
      create.mutate(form, { onSuccess: () => setShowForm(false) });
    }
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={showForm ? () => setShowForm(false) : startCreate}>
          {showForm ? 'Cancel' : 'Add Category'}
        </Button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea placeholder="Description" rows={2} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Image URL" value={form.imageUrl} onChange={(e) => setForm((p) => ({ ...p, imageUrl: e.target.value }))} className={inputClass} />
          <input type="number" placeholder="Display order" value={form.displayOrder} onChange={(e) => setForm((p) => ({ ...p, displayOrder: Number(e.target.value) }))} className={inputClass} />
          {editingId && (
            <label className="flex items-center gap-2 text-sm sm:col-span-2">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))} />
              Active
            </label>
          )}
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending || update.isPending}>
            {editingId ? 'Save Changes' : 'Create Category'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {categories.map((c) => (
            <div key={c.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{c.name}</p>
                <p className="text-xs text-body/60">{c.productCount} products · order {c.displayOrder}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={c.isActive ? 'success' : 'neutral'}>{c.isActive ? 'Active' : 'Inactive'}</Badge>
                <Button variant="secondary" size="sm" onClick={() => startEdit(c)}>Edit</Button>
                <Button variant="secondary" size="sm" disabled={remove.isPending} onClick={() => remove.mutate(c.id)}>Delete</Button>
              </div>
            </div>
          ))}
          {categories.length === 0 && <p className="p-6 text-center text-sm text-body/60">No categories yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

// ---- Villages --------------------------------------------------------------

function VillagesTab() {
  const districtsQuery = useDistricts();
  const { data, isLoading, isError, error } = useVillages();
  const createVillage = useCreateVillage();
  const { update, remove } = useVillageMutations();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({ name: '', craft: '', description: '', imageUrl: '', districtId: '', isActive: true });

  const villages = data || [];
  const districts = districtsQuery.data || [];

  const startCreate = () => { setEditingId(null); setForm({ name: '', craft: '', description: '', imageUrl: '', districtId: '', isActive: true }); setShowForm(true); };
  const startEdit = (v) => {
    setEditingId(v.id);
    setForm({ name: v.name, craft: v.craft, description: v.description || '', imageUrl: v.imageUrl || '', districtId: v.districtId, isActive: v.isActive });
    setShowForm(true);
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    if (editingId) {
      update.mutate({ id: editingId, payload: form }, { onSuccess: () => setShowForm(false) });
    } else {
      const { isActive, ...payload } = form;
      createVillage.mutate(payload, { onSuccess: () => setShowForm(false) });
    }
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={showForm ? () => setShowForm(false) : startCreate}>
          {showForm ? 'Cancel' : 'Add Village'}
        </Button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Village name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={inputClass} />
          <input required placeholder="Craft" value={form.craft} onChange={(e) => setForm((p) => ({ ...p, craft: e.target.value }))} className={inputClass} />
          <select required value={form.districtId} onChange={(e) => setForm((p) => ({ ...p, districtId: e.target.value }))} className={`${inputClass} sm:col-span-2`}>
            <option value="">District</option>
            {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <textarea placeholder="Description" rows={2} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <input placeholder="Image URL" value={form.imageUrl} onChange={(e) => setForm((p) => ({ ...p, imageUrl: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          {editingId && (
            <label className="flex items-center gap-2 text-sm sm:col-span-2">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))} />
              Active
            </label>
          )}
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createVillage.isPending || update.isPending}>
            {editingId ? 'Save Changes' : 'Add Village'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {villages.map((v) => (
            <div key={v.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{v.name}</p>
                <p className="text-xs text-body/60">{v.craft} · {v.districtName}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={v.isActive ? 'success' : 'neutral'}>{v.isActive ? 'Active' : 'Inactive'}</Badge>
                <Button variant="secondary" size="sm" onClick={() => startEdit(v)}>Edit</Button>
                <Button variant="secondary" size="sm" disabled={remove.isPending} onClick={() => remove.mutate(v.id)}>Delete</Button>
              </div>
            </div>
          ))}
          {villages.length === 0 && <p className="p-6 text-center text-sm text-body/60">No villages yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

// ---- Districts -------------------------------------------------------------

function DistrictsTab() {
  const { data, isLoading, isError, error } = useDistricts({ includeInactive: true });
  const { update } = useDistrictMutations();
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({ division: '', displayOrder: 0, isActive: true });

  const districts = data || [];

  const startEdit = (d) => {
    setEditingId(d.id);
    setForm({ division: d.division, displayOrder: d.displayOrder, isActive: d.isActive });
  };

  const handleSubmit = (event, id) => {
    event.preventDefault();
    update.mutate({ id, payload: form }, { onSuccess: () => setEditingId(null) });
  };

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {districts.map((d) => (
          <div key={d.id} className="p-4">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-heading">{d.name}</p>
                <p className="text-xs text-body/60">{d.division} · order {d.displayOrder}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={d.isActive ? 'success' : 'neutral'}>{d.isActive ? 'Active' : 'Inactive'}</Badge>
                <Button variant="secondary" size="sm" onClick={() => (editingId === d.id ? setEditingId(null) : startEdit(d))}>
                  {editingId === d.id ? 'Cancel' : 'Edit'}
                </Button>
              </div>
            </div>
            {editingId === d.id && (
              <form onSubmit={(e) => handleSubmit(e, d.id)} className="mt-3 flex flex-wrap gap-2">
                <input required placeholder="Division" value={form.division} onChange={(e) => setForm((p) => ({ ...p, division: e.target.value }))} className={inputClass} />
                <input type="number" placeholder="Display order" value={form.displayOrder} onChange={(e) => setForm((p) => ({ ...p, displayOrder: Number(e.target.value) }))} className={inputClass} />
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))} />
                  Active
                </label>
                <Button type="submit" variant="primary" size="sm" disabled={update.isPending}>Save</Button>
              </form>
            )}
          </div>
        ))}
        {districts.length === 0 && <p className="p-6 text-center text-sm text-body/60">No districts found.</p>}
      </div>
    </AsyncState>
  );
}

// ---- Festivals --------------------------------------------------------------

const emptyFestivalForm = { name: '', description: '', districtId: '', startDate: '', endDate: '', isRecurringAnnually: false, imageUrl: '', isActive: true };

function FestivalsTab() {
  const districtsQuery = useDistricts();
  const { data, isLoading, isError, error } = useHeritageFestivals({ pageSize: 100 });
  const { create, update, remove } = useHeritageFestivalMutations();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyFestivalForm);

  const festivals = data?.items || [];
  const districts = districtsQuery.data || [];

  const startCreate = () => { setEditingId(null); setForm(emptyFestivalForm); setShowForm(true); };
  const startEdit = (f) => {
    setEditingId(f.id);
    setForm({
      name: f.name,
      description: f.description,
      districtId: f.districtId,
      startDate: toDateInput(f.startDate),
      endDate: toDateInput(f.endDate),
      isRecurringAnnually: f.isRecurringAnnually,
      imageUrl: f.imageUrl || '',
      isActive: f.isActive,
    });
    setShowForm(true);
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    if (editingId) {
      update.mutate({ id: editingId, payload: form }, { onSuccess: () => setShowForm(false) });
    } else {
      const { isActive, ...payload } = form;
      create.mutate(payload, { onSuccess: () => setShowForm(false) });
    }
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={showForm ? () => setShowForm(false) : startCreate}>
          {showForm ? 'Cancel' : 'Add Festival'}
        </Button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Name" value={form.name} onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <textarea required placeholder="Description" rows={2} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select required value={form.districtId} onChange={(e) => setForm((p) => ({ ...p, districtId: e.target.value }))} className={inputClass}>
            <option value="">District</option>
            {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <input placeholder="Image URL" value={form.imageUrl} onChange={(e) => setForm((p) => ({ ...p, imageUrl: e.target.value }))} className={inputClass} />
          <input required type="date" value={form.startDate} onChange={(e) => setForm((p) => ({ ...p, startDate: e.target.value }))} className={inputClass} />
          <input required type="date" value={form.endDate} onChange={(e) => setForm((p) => ({ ...p, endDate: e.target.value }))} className={inputClass} />
          <label className="flex items-center gap-2 text-sm">
            <input type="checkbox" checked={form.isRecurringAnnually} onChange={(e) => setForm((p) => ({ ...p, isRecurringAnnually: e.target.checked }))} />
            Recurs annually
          </label>
          {editingId && (
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))} />
              Active
            </label>
          )}
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending || update.isPending}>
            {editingId ? 'Save Changes' : 'Create Festival'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {festivals.map((f) => (
            <div key={f.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{f.name}</p>
                <p className="text-xs text-body/60">{f.districtName} · {new Date(f.startDate).toLocaleDateString()} – {new Date(f.endDate).toLocaleDateString()}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={f.isActive ? 'success' : 'neutral'}>{f.isActive ? 'Active' : 'Inactive'}</Badge>
                <Button variant="secondary" size="sm" onClick={() => startEdit(f)}>Edit</Button>
                <Button variant="secondary" size="sm" disabled={remove.isPending} onClick={() => remove.mutate(f.id)}>Delete</Button>
              </div>
            </div>
          ))}
          {festivals.length === 0 && <p className="p-6 text-center text-sm text-body/60">No festivals yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

// ---- UNESCO Records ---------------------------------------------------------

const emptyUnescoForm = { title: '', type: 'CulturalHeritageSite', description: '', inscribedYear: new Date().getFullYear(), districtId: '', imageUrl: '', officialUrl: '', displayOrder: 0, isActive: true };

function UnescoTab() {
  const districtsQuery = useDistricts();
  const { data, isLoading, isError, error } = useUnescoRecords({ includeInactive: true });
  const { create, update, remove } = useUnescoRecordMutations();
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyUnescoForm);

  const records = data || [];
  const districts = districtsQuery.data || [];

  const startCreate = () => { setEditingId(null); setForm(emptyUnescoForm); setShowForm(true); };
  const startEdit = (r) => {
    setEditingId(r.id);
    setForm({
      title: r.title,
      type: r.type,
      description: r.description,
      inscribedYear: r.inscribedYear,
      districtId: r.districtId || '',
      imageUrl: r.imageUrl || '',
      officialUrl: r.officialUrl || '',
      displayOrder: r.displayOrder,
      isActive: r.isActive,
    });
    setShowForm(true);
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    const payload = { ...form, districtId: form.districtId || null };
    if (editingId) {
      update.mutate({ id: editingId, payload }, { onSuccess: () => setShowForm(false) });
    } else {
      const { isActive, ...createPayload } = payload;
      create.mutate(createPayload, { onSuccess: () => setShowForm(false) });
    }
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={showForm ? () => setShowForm(false) : startCreate}>
          {showForm ? 'Cancel' : 'Add UNESCO Record'}
        </Button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select value={form.type} onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))} className={inputClass}>
            {unescoTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input type="number" placeholder="Inscribed year" value={form.inscribedYear} onChange={(e) => setForm((p) => ({ ...p, inscribedYear: Number(e.target.value) }))} className={inputClass} />
          <textarea required placeholder="Description" rows={2} value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select value={form.districtId} onChange={(e) => setForm((p) => ({ ...p, districtId: e.target.value }))} className={inputClass}>
            <option value="">No specific district</option>
            {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <input type="number" placeholder="Display order" value={form.displayOrder} onChange={(e) => setForm((p) => ({ ...p, displayOrder: Number(e.target.value) }))} className={inputClass} />
          <input placeholder="Image URL" value={form.imageUrl} onChange={(e) => setForm((p) => ({ ...p, imageUrl: e.target.value }))} className={inputClass} />
          <input placeholder="Official UNESCO URL" value={form.officialUrl} onChange={(e) => setForm((p) => ({ ...p, officialUrl: e.target.value }))} className={inputClass} />
          {editingId && (
            <label className="flex items-center gap-2 text-sm sm:col-span-2">
              <input type="checkbox" checked={form.isActive} onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))} />
              Active
            </label>
          )}
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={create.isPending || update.isPending}>
            {editingId ? 'Save Changes' : 'Create Record'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {records.map((r) => (
            <div key={r.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{r.title}</p>
                <p className="text-xs text-body/60">{r.type} · inscribed {r.inscribedYear}{r.districtName ? ` · ${r.districtName}` : ''}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={r.isActive ? 'success' : 'neutral'}>{r.isActive ? 'Active' : 'Inactive'}</Badge>
                <Button variant="secondary" size="sm" onClick={() => startEdit(r)}>Edit</Button>
                <Button variant="secondary" size="sm" disabled={remove.isPending} onClick={() => remove.mutate(r.id)}>Delete</Button>
              </div>
            </div>
          ))}
          {records.length === 0 && <p className="p-6 text-center text-sm text-body/60">No UNESCO records yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

// ---- Producer Heritage Identity Verification --------------------------------

function IdentityVerificationTab() {
  const [producerId, setProducerId] = useState('');
  const [lookupId, setLookupId] = useState(null);
  const identityQuery = useHeritageIdentity(lookupId);
  const verifyIdentity = useVerifyHeritageIdentity();

  return (
    <div>
      <SectionHeader eyebrow="Verification" title="Producer Heritage Identity" description="Look up a producer's heritage identity submission by their user ID and verify or reject it." />
      <div className="mb-4 flex gap-2">
        <input
          placeholder="Producer ID"
          value={producerId}
          onChange={(event) => setProducerId(event.target.value)}
          className={`${inputClass} flex-1`}
        />
        <Button variant="primary" onClick={() => setLookupId(producerId)}>Look Up</Button>
      </div>
      {lookupId && (
        <AsyncState isLoading={identityQuery.isLoading} isError={identityQuery.isError} error={identityQuery.error}>
          {identityQuery.data && (
            <div className="rounded-xl border border-border bg-surface p-5">
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold text-heading">{identityQuery.data.producerName}</p>
                <Badge tone={identityQuery.data.verificationStatus === 'Verified' ? 'success' : 'secondary'}>{identityQuery.data.verificationStatus}</Badge>
              </div>
              <p className="mt-1 text-xs text-body/60">{identityQuery.data.primaryCraft} · {identityQuery.data.workshopName}</p>
              <p className="mt-2 text-sm text-body/70">{identityQuery.data.workshopDescription}</p>
              <div className="mt-4 flex gap-2">
                <Button variant="primary" onClick={() => verifyIdentity.mutate({ producerId: lookupId, payload: { status: 'Verified' } })}>Approve</Button>
                <Button variant="secondary" onClick={() => verifyIdentity.mutate({ producerId: lookupId, payload: { status: 'Rejected' } })}>Reject</Button>
              </div>
            </div>
          )}
        </AsyncState>
      )}
    </div>
  );
}

// ---- Page --------------------------------------------------------------------

const tabs = [
  { id: 'categories', label: 'Craft Categories' },
  { id: 'villages', label: 'Heritage Villages' },
  { id: 'districts', label: 'Districts' },
  { id: 'festivals', label: 'Festivals' },
  { id: 'unesco', label: 'UNESCO Records' },
  { id: 'identity', label: 'Producer Identity' },
];

export default function HeritageManagement() {
  const [tab, setTab] = useState('categories');

  return (
    <div>
      <PageHeader title="Heritage Management" description="Manage craft categories, heritage villages, districts, festivals, UNESCO records and producer heritage-identity verification." />

      <div className="mb-6 flex flex-wrap gap-2 border-b border-border">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === t.id ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'categories' && <CategoriesTab />}
      {tab === 'villages' && <VillagesTab />}
      {tab === 'districts' && <DistrictsTab />}
      {tab === 'festivals' && <FestivalsTab />}
      {tab === 'unesco' && <UnescoTab />}
      {tab === 'identity' && <IdentityVerificationTab />}
    </div>
  );
}
