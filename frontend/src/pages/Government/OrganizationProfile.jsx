import { useEffect, useState } from 'react';
import { Badge, Button, PageHeader, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { useSupportOrganization, useArtisanSupportMutations } from '../../hooks/useArtisanSupport';

const field = 'w-full rounded-lg border border-border bg-background px-3 py-2.5 text-sm';
const empty = { organizationName: '', organizationType: 'NGO', registrationNumber: '', registrationAuthority: '', registrationDocumentUrl: '', officialEmail: '', officialPhone: '', address: '', operatingDistricts: [], organizationDetails: '', representativeName: '', representativeDesignation: '', representativeNid: '', representativePhone: '', submitForReview: false };

export default function OrganizationProfile() {
  const query = useSupportOrganization(); const { saveOrganization, uploadDocument } = useArtisanSupportMutations(); const [form, setForm] = useState(empty); const [districts, setDistricts] = useState('');
  useEffect(() => { if (query.data) { setForm({ ...empty, ...query.data, submitForReview: false }); setDistricts((query.data.operatingDistricts || []).join(', ')); } }, [query.data]);
  const set = (key) => (e) => setForm((p) => ({ ...p, [key]: e.target.value }));
  const submit = (e, send) => { e.preventDefault(); saveOrganization.mutate({ ...form, operatingDistricts: districts.split(',').map((x) => x.trim()).filter(Boolean), submitForReview: send }); };
  const upload = (e) => { const file = e.target.files?.[0]; if (file) uploadDocument.mutate(file, { onSuccess: ({ url }) => setForm((p) => ({ ...p, registrationDocumentUrl: url })) }); };
  return <div><PageHeader title="Organization verification" description="Register the organization and authorized representative. Admin verification unlocks artisan case work." />
    <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
      <div className="mb-5 flex items-center gap-3 rounded-xl border border-border bg-surface p-4"><Badge tone={form.status === 'Approved' ? 'success' : 'secondary'}>{form.status || 'Draft'}</Badge><p className="text-sm text-body">{form.status === 'Approved' ? 'Verified. Your organization can accept and manage artisan cases.' : 'Complete every field, upload the registration certificate, and submit it for Admin review.'}</p></div>
      <MutationFeedback mutation={saveOrganization} successMessage="Organization profile saved." /><MutationFeedback mutation={uploadDocument} successMessage="Registration document uploaded." />
      <form onSubmit={(e) => submit(e, false)} className="grid gap-4 rounded-xl border border-border bg-surface p-5 md:grid-cols-2">
        <label className="text-sm font-medium">Organization name<input required className={field} value={form.organizationName} onChange={set('organizationName')} /></label>
        <label className="text-sm font-medium">Organization type<select className={field} value={form.organizationType} onChange={set('organizationType')}><option>NGO</option><option>Government Agency</option><option>Government Programme</option><option>Foundation</option></select></label>
        <label className="text-sm font-medium">Registration number<input required className={field} value={form.registrationNumber} onChange={set('registrationNumber')} /></label>
        <label className="text-sm font-medium">Registration authority<input required className={field} value={form.registrationAuthority} onChange={set('registrationAuthority')} /></label>
        <label className="text-sm font-medium">Official email<input required type="email" className={field} value={form.officialEmail} onChange={set('officialEmail')} /></label>
        <label className="text-sm font-medium">Official phone<input required className={field} value={form.officialPhone} onChange={set('officialPhone')} /></label>
        <label className="text-sm font-medium md:col-span-2">Address<input required className={field} value={form.address} onChange={set('address')} /></label>
        <label className="text-sm font-medium md:col-span-2">Operating districts <span className="font-normal text-body/60">(comma separated)</span><input required className={field} value={districts} onChange={(e) => setDistricts(e.target.value)} /></label>
        <label className="text-sm font-medium md:col-span-2">Organization details<textarea required rows={3} className={field} value={form.organizationDetails} onChange={set('organizationDetails')} /></label>
        <label className="text-sm font-medium">Representative name<input required className={field} value={form.representativeName} onChange={set('representativeName')} /></label>
        <label className="text-sm font-medium">Designation<input required className={field} value={form.representativeDesignation} onChange={set('representativeDesignation')} /></label>
        <label className="text-sm font-medium">Representative NID<input required className={field} value={form.representativeNid} onChange={set('representativeNid')} /></label>
        <label className="text-sm font-medium">Representative phone<input required className={field} value={form.representativePhone} onChange={set('representativePhone')} /></label>
        <label className="text-sm font-medium md:col-span-2">Registration certificate<input type="file" accept=".pdf,image/jpeg,image/png,image/webp" className={`${field} mt-1`} onChange={upload} />{form.registrationDocumentUrl && <a className="mt-1 block text-xs text-primary underline" href={form.registrationDocumentUrl} target="_blank" rel="noreferrer">View uploaded certificate</a>}</label>
        <div className="flex flex-wrap gap-2 md:col-span-2"><Button type="submit" variant="secondary" disabled={saveOrganization.isPending}>Save draft</Button><Button type="button" disabled={saveOrganization.isPending || !form.registrationDocumentUrl} onClick={(e) => submit(e, true)}>Submit for Admin verification</Button></div>
      </form>
    </AsyncState>
  </div>;
}
