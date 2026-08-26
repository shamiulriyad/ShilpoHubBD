import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, SectionHeader } from '../../components/ui';
import { useMySustainabilityProfile, useSustainabilityMutations } from '../../hooks/useSustainability';

export default function Sustainability() {
  const { data, isLoading, isError, error } = useMySustainabilityProfile();
  const { addMaterial, addCertification } = useSustainabilityMutations();
  const [material, setMaterial] = useState({ materialName: '', quantityUsed: '', unit: '', isRecycled: false, isRenewable: false, isLocallySourced: false, isBiodegradable: false, carbonSavingsPerUnitKg: '' });
  const [cert, setCert] = useState({ materialName: '', certifyingBody: '', certificateReference: '', issuedAt: '' });

  const handleAddMaterial = (event) => {
    event.preventDefault();
    addMaterial.mutate({
      ...material,
      quantityUsed: Number(material.quantityUsed),
      carbonSavingsPerUnitKg: Number(material.carbonSavingsPerUnitKg) || 0,
    });
  };

  const handleAddCert = (event) => {
    event.preventDefault();
    addCertification.mutate(cert);
  };

  const profile = data;

  return (
    <div>
      <PageHeader title="Sustainability Profile" description="Track your eco-friendly materials and certifications." />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        {profile && (
          <div className="mb-8 grid grid-cols-2 gap-4 lg:grid-cols-3">
            <div className="rounded-xl border border-border bg-surface p-5">
              <p className="text-xs font-medium uppercase text-body/60">Eco Score</p>
              <p className="mt-2 text-2xl font-semibold text-primary">{profile.ecoScore}</p>
              <Badge tone="success">{profile.badgeLevel}</Badge>
            </div>
            <div className="rounded-xl border border-border bg-surface p-5">
              <p className="text-xs font-medium uppercase text-body/60">CO₂ Savings</p>
              <p className="mt-2 text-2xl font-semibold text-primary">{profile.totalCarbonSavingsKg.toLocaleString()} kg</p>
            </div>
          </div>
        )}
      </AsyncState>

      <SectionHeader eyebrow="Materials" title="Sustainable Material Records" />
      <form onSubmit={handleAddMaterial} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
        <div className="grid gap-3 sm:grid-cols-3">
          <input required placeholder="Material name" value={material.materialName} onChange={(e) => setMaterial((p) => ({ ...p, materialName: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required type="number" placeholder="Quantity used" value={material.quantityUsed} onChange={(e) => setMaterial((p) => ({ ...p, quantityUsed: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Unit (kg, m, etc.)" value={material.unit} onChange={(e) => setMaterial((p) => ({ ...p, unit: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        </div>
        <div className="flex flex-wrap gap-4 text-sm">
          {['isRecycled', 'isRenewable', 'isLocallySourced', 'isBiodegradable'].map((key) => (
            <label key={key} className="flex items-center gap-2">
              <input type="checkbox" checked={material[key]} onChange={(e) => setMaterial((p) => ({ ...p, [key]: e.target.checked }))} />
              {key.replace('is', '')}
            </label>
          ))}
        </div>
        <Button type="submit" variant="primary" disabled={addMaterial.isPending}>Add Material Record</Button>
      </form>
      <div className="mb-10 divide-y divide-border rounded-xl border border-border bg-surface">
        {(profile?.materialRecords || []).map((m) => (
          <div key={m.id} className="flex items-center justify-between p-3 text-sm">
            <span>{m.materialName} — {m.quantityUsed} {m.unit}</span>
            <span className="text-body/60">{m.totalCarbonSavingsKg.toLocaleString()} kg CO₂ saved</span>
          </div>
        ))}
        {(profile?.materialRecords || []).length === 0 && <p className="p-3 text-sm text-body/60">No material records yet.</p>}
      </div>

      <SectionHeader eyebrow="Certifications" title="Sustainability Certifications" />
      <form onSubmit={handleAddCert} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
        <input required placeholder="Material name" value={cert.materialName} onChange={(e) => setCert((p) => ({ ...p, materialName: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <input required placeholder="Certifying body" value={cert.certifyingBody} onChange={(e) => setCert((p) => ({ ...p, certifyingBody: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <input required placeholder="Certificate reference" value={cert.certificateReference} onChange={(e) => setCert((p) => ({ ...p, certificateReference: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <input required type="date" value={cert.issuedAt} onChange={(e) => setCert((p) => ({ ...p, issuedAt: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <Button type="submit" variant="primary" className="sm:col-span-2" disabled={addCertification.isPending}>Add Certification</Button>
      </form>
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {(profile?.certifications || []).map((c) => (
          <div key={c.id} className="flex items-center justify-between p-3 text-sm">
            <span>{c.materialName} — {c.certifyingBody}</span>
            <Badge tone={c.isVerified ? 'success' : 'secondary'}>{c.isVerified ? 'Verified' : 'Pending Verification'}</Badge>
          </div>
        ))}
        {(profile?.certifications || []).length === 0 && <p className="p-3 text-sm text-body/60">No certifications yet.</p>}
      </div>
    </div>
  );
}
