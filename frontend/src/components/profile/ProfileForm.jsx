import { useEffect, useState } from 'react';
import { Badge, Button, AsyncState } from '../ui';
import MutationFeedback from '../ui/MutationFeedback';
import { useDistricts } from '../../hooks/useDistricts';
import { useMyProfile, useSaveProfile } from '../../hooks/useProfile';

const inputClass = 'w-full rounded-md border border-border bg-background px-3 py-2 text-sm';
const STATUS_TONE = { NotSubmitted: 'neutral', Pending: 'secondary', Approved: 'success', Rejected: 'neutral' };
const STATUS_LABEL = { NotSubmitted: 'Not submitted', Pending: 'Waiting for admin approval', Approved: 'Approved', Rejected: 'Needs changes' };

const EMPTY = { legalName: '', phone: '', nidNumber: '', expertise: '', districtId: '', addressLine: '', about: '' };

// Real-world details of the member (name, expertise, location, phone, NID). Separate from the login
// account; an admin approves it after checking the NID.
export default function ProfileForm() {
  const query = useMyProfile();
  const save = useSaveProfile();
  const districts = useDistricts().data || [];
  const [form, setForm] = useState(EMPTY);
  const profile = query.data;

  useEffect(() => {
    if (!profile) return;
    setForm({
      legalName: profile.legalName || '',
      phone: profile.phone || '',
      nidNumber: profile.nidNumber || '',
      expertise: profile.expertise || '',
      districtId: profile.districtId || '',
      addressLine: profile.addressLine || '',
      about: profile.about || '',
    });
  }, [profile]);

  const set = (key) => (e) => setForm((p) => ({ ...p, [key]: e.target.value }));
  const nidOk = /^(\d{10}|\d{13}|\d{17})$/.test(form.nidNumber.trim());
  const phoneOk = /^\+?[0-9][0-9 -]{6,18}$/.test(form.phone.trim());
  const expertiseOk = !profile?.expertiseRequired || form.expertise.trim().length > 0;
  const valid = form.legalName.trim() && phoneOk && nidOk && form.addressLine.trim() && expertiseOk;

  const submit = (event) => {
    event.preventDefault();
    if (!valid) return;
    save.mutate({
      legalName: form.legalName.trim(),
      phone: form.phone.trim(),
      nidNumber: form.nidNumber.trim(),
      expertise: form.expertise.trim() || undefined,
      districtId: form.districtId || undefined,
      addressLine: form.addressLine.trim(),
      about: form.about.trim() || undefined,
    });
  };

  return (
    <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
      {profile && (
        <form onSubmit={submit} className="space-y-4 rounded-xl border border-border bg-surface p-5">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <h2 className="text-base font-semibold text-heading">Member profile</h2>
              <p className="text-xs text-body/60">Your real details, separate from your login ({profile.loginEmail}). An admin checks the NID before approving.</p>
            </div>
            <Badge tone={STATUS_TONE[profile.status] || 'neutral'}>{STATUS_LABEL[profile.status] || profile.status}</Badge>
          </div>

          {profile.status === 'Rejected' && profile.reviewNotes && (
            <p role="alert" className="rounded-md border border-error/30 bg-error/5 px-3 py-2 text-sm text-error">Admin note: {profile.reviewNotes}</p>
          )}
          {profile.status === 'Approved' && <p className="text-xs text-body/60">Saving changes sends the profile back to the admin for approval.</p>}

          <div className="grid gap-4 sm:grid-cols-2">
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-heading">Full name (as on your NID)</span>
              <input required value={form.legalName} onChange={set('legalName')} maxLength={200} className={inputClass} />
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-heading">Phone number</span>
              <input required value={form.phone} onChange={set('phone')} placeholder="01XXXXXXXXX" className={inputClass} />
              {form.phone && !phoneOk && <span role="alert" className="mt-1 block text-xs text-error">Enter a valid phone number.</span>}
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-heading">NID number</span>
              <input required inputMode="numeric" value={form.nidNumber} onChange={set('nidNumber')} placeholder="10, 13 or 17 digits" className={inputClass} />
              {form.nidNumber && !nidOk && <span role="alert" className="mt-1 block text-xs text-error">The NID number must be 10, 13 or 17 digits.</span>}
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-heading">Expertise{profile.expertiseRequired ? '' : ' (optional)'}</span>
              <input required={profile.expertiseRequired} value={form.expertise} onChange={set('expertise')} placeholder="e.g. Jamdani weaving, pottery" maxLength={200} className={inputClass} />
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-heading">District</span>
              <select value={form.districtId} onChange={set('districtId')} className={inputClass}>
                <option value="">Select district</option>
                {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
            </label>
            <label className="block text-sm">
              <span className="mb-1 block font-medium text-heading">Address</span>
              <input required value={form.addressLine} onChange={set('addressLine')} maxLength={500} className={inputClass} />
            </label>
            <label className="block text-sm sm:col-span-2">
              <span className="mb-1 block font-medium text-heading">About you (optional)</span>
              <textarea rows={3} value={form.about} onChange={set('about')} maxLength={2000} className={inputClass} />
            </label>
          </div>

          <div className="flex items-center gap-3">
            <Button type="submit" variant="primary" disabled={!valid || save.isPending}>{save.isPending ? 'Saving…' : profile.exists ? 'Save and resubmit' : 'Submit profile'}</Button>
            {!valid && <span className="text-xs text-body/60">Fill every required field with a valid NID and phone number.</span>}
          </div>
          <MutationFeedback mutation={save} successMessage="Profile submitted. An admin will review it soon." />
        </form>
      )}
    </AsyncState>
  );
}
