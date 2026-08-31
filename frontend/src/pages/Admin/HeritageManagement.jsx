import { useState } from 'react';
import { PageHeader, Table, Button, Badge, AsyncState, SectionHeader } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages, useCreateVillage } from '../../hooks/useVillages';
import { useCategories } from '../../hooks/useCategories';
import { useHeritageIdentity, useVerifyHeritageIdentity } from '../../hooks/useHeritageIdentity';

export default function HeritageManagement() {
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const categoriesQuery = useCategories();
  const createVillage = useCreateVillage();

  const [villageForm, setVillageForm] = useState({ name: '', craft: '', districtId: '' });
  const [producerId, setProducerId] = useState('');
  const [lookupId, setLookupId] = useState(null);
  const identityQuery = useHeritageIdentity(lookupId);
  const verifyIdentity = useVerifyHeritageIdentity();

  const handleAddVillage = (event) => {
    event.preventDefault();
    createVillage.mutate(villageForm, { onSuccess: () => setVillageForm({ name: '', craft: '', districtId: '' }) });
  };

  return (
    <div>
      <PageHeader title="Heritage Management" description="Manage districts, villages, categories and producer heritage-identity verification." />

      <div className="space-y-10">
        <div>
          <p className="mb-3 text-sm font-semibold text-heading">Districts</p>
          <AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}>
            <Table columns={['name', 'division']} rows={(districtsQuery.data || []).map((d) => ({ name: d.name, division: d.division }))} />
          </AsyncState>
        </div>

        <div>
          <SectionHeader eyebrow="Heritage Villages" title="Villages" />
          <form onSubmit={handleAddVillage} className="mb-4 flex flex-wrap gap-2 rounded-xl border border-border bg-surface p-4">
            <input required placeholder="Village name" value={villageForm.name} onChange={(e) => setVillageForm((p) => ({ ...p, name: e.target.value }))} className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input required placeholder="Craft" value={villageForm.craft} onChange={(e) => setVillageForm((p) => ({ ...p, craft: e.target.value }))} className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <select required value={villageForm.districtId} onChange={(e) => setVillageForm((p) => ({ ...p, districtId: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
              <option value="">District</option>
              {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
            <Button type="submit" variant="primary" disabled={createVillage.isPending}>Add Village</Button>
          </form>
          <AsyncState isLoading={villagesQuery.isLoading} isError={villagesQuery.isError} error={villagesQuery.error}>
            <Table columns={['name', 'craft', 'districtName']} rows={villagesQuery.data || []} />
          </AsyncState>
        </div>

        <div>
          <p className="mb-3 text-sm font-semibold text-heading">Categories</p>
          <AsyncState isLoading={categoriesQuery.isLoading} isError={categoriesQuery.isError} error={categoriesQuery.error}>
            <Table columns={['name', 'productCount']} rows={categoriesQuery.data || []} />
          </AsyncState>
        </div>

        <div>
          <SectionHeader eyebrow="Verification" title="Producer Heritage Identity" description="Look up a producer's heritage identity submission by their user ID and verify or reject it." />
          <div className="mb-4 flex gap-2">
            <input
              placeholder="Producer ID"
              value={producerId}
              onChange={(event) => setProducerId(event.target.value)}
              className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
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
      </div>
    </div>
  );
}
