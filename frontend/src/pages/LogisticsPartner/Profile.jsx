import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { PageHeader, Button, Badge } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { useAuth } from '../../hooks/useAuth';
import { useDistricts } from '../../hooks/useDistricts';
import {
  useMyLogisticsPartnerProfile,
  useUpsertLogisticsPartnerProfile,
  useUpsertLogisticsServiceArea,
  useRemoveLogisticsServiceArea,
} from '../../hooks/useLogisticsPartners';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

const emptyForm = {
  companyName: '',
  legalName: '',
  registrationNumber: '',
  contactPersonName: '',
  contactPhone: '',
  contactEmail: '',
  baseAddressLine: '',
  baseCity: '',
  baseDistrictId: '',
  basePostalCode: '',
  country: 'Bangladesh',
  fleetSize: 0,
  maxDailyPickups: 0,
  maxVehicleCapacityKg: '',
  operatingDayStartHour: 8,
  operatingDayEndHour: 20,
  offersCashOnDelivery: true,
  offersColdChain: false,
  offersFragileHandling: false,
  isAcceptingRequests: true,
  notes: '',
};

const verificationTone = {
  Verified: 'success',
  Pending: 'warning',
  Rejected: 'danger',
  Suspended: 'danger',
};

export default function LogisticsPartnerProfile() {
  const { user } = useAuth();
  const location = useLocation();
  const profileQuery = useMyLogisticsPartnerProfile();
  const districtsQuery = useDistricts();
  const upsert = useUpsertLogisticsPartnerProfile(user?.id);
  const upsertServiceArea = useUpsertLogisticsServiceArea(user?.id);
  const removeServiceArea = useRemoveLogisticsServiceArea(user?.id);

  const [form, setForm] = useState(emptyForm);
  const [areaForm, setAreaForm] = useState({ districtId: '', standardDeliveryDays: 3, supportsSameDay: false, surchargeAmount: '' });

  useEffect(() => {
    if (profileQuery.data) {
      setForm((prev) => ({ ...prev, ...profileQuery.data }));
    } else if (profileQuery.error?.response?.status === 404 && user) {
      setForm((prev) => ({
        ...prev,
        companyName: prev.companyName || `${user.fullName || user.name || 'My'} Logistics`,
        contactPersonName: prev.contactPersonName || user.fullName || user.name || '',
        contactEmail: prev.contactEmail || user.email || '',
      }));
    }
  }, [profileQuery.data, profileQuery.error, user]);

  const set = (field) => (event) => {
    const { type, value, checked } = event.target;
    setForm((prev) => ({ ...prev, [field]: type === 'checkbox' ? checked : value }));
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    upsert.mutate({
      ...form,
      fleetSize: Number(form.fleetSize) || 0,
      maxDailyPickups: Number(form.maxDailyPickups) || 0,
      maxVehicleCapacityKg: form.maxVehicleCapacityKg === '' ? null : Number(form.maxVehicleCapacityKg),
      operatingDayStartHour: form.operatingDayStartHour === '' ? null : Number(form.operatingDayStartHour),
      operatingDayEndHour: form.operatingDayEndHour === '' ? null : Number(form.operatingDayEndHour),
      baseDistrictId: form.baseDistrictId || null,
    });
  };

  const handleAddServiceArea = (event) => {
    event.preventDefault();
    if (!areaForm.districtId) return;
    upsertServiceArea.mutate(
      {
        districtId: areaForm.districtId,
        standardDeliveryDays: Number(areaForm.standardDeliveryDays) || 1,
        supportsSameDay: areaForm.supportsSameDay,
        surchargeAmount: areaForm.surchargeAmount === '' ? null : Number(areaForm.surchargeAmount),
        isActive: true,
      },
      { onSuccess: () => setAreaForm({ districtId: '', standardDeliveryDays: 3, supportsSameDay: false, surchargeAmount: '' }) },
    );
  };

  const profile = profileQuery.data;

  return (
    <div>
      <PageHeader
        title="Company Profile"
        description="Keep your logistics company details, capacity and service coverage up to date."
      />

      {(location.state?.setupRequired || profileQuery.error?.response?.status === 404) && (
        <div className="mb-5 rounded-2xl border border-primary/20 bg-primary-soft p-5 text-sm text-heading">
          <p className="font-semibold">Complete your company profile to activate the logistics workspace.</p>
          <p className="mt-1 text-body/70">Warehouses, shipments, pickups, returns, stock and delivery routes will become available immediately after you save these details.</p>
        </div>
      )}

      {profile && (
        <div className="mb-4 flex flex-wrap items-center gap-3 rounded-xl border border-border bg-surface p-4">
          <span className="text-sm text-body/70">Verification status:</span>
          <Badge tone={verificationTone[profile.verificationStatus] || 'neutral'}>{profile.verificationStatus}</Badge>
          {profile.verificationNotes && <span className="text-xs text-body/50">{profile.verificationNotes}</span>}
        </div>
      )}

      {profileQuery.isLoading && <p role="status" className="mb-3 text-sm text-body/60">Checking for an existing profile… You can start completing the form now.</p>}
      <form onSubmit={handleSubmit} className="grid gap-4 rounded-xl border border-border bg-surface p-6 sm:grid-cols-2">
          <input aria-label="Company name" required placeholder="Company name" value={form.companyName} onChange={set('companyName')} className={inputClass} />
          <input aria-label="Legal name" placeholder="Legal name (optional)" value={form.legalName || ''} onChange={set('legalName')} className={inputClass} />
          <input aria-label="Registration number" placeholder="Registration number" value={form.registrationNumber || ''} onChange={set('registrationNumber')} className={inputClass} />
          <input aria-label="Country" placeholder="Country" value={form.country} onChange={set('country')} className={inputClass} />

          <input aria-label="Contact person name" required placeholder="Contact person name" value={form.contactPersonName} onChange={set('contactPersonName')} className={inputClass} />
          <input aria-label="Contact phone" required placeholder="Contact phone" value={form.contactPhone} onChange={set('contactPhone')} className={inputClass} />
          <input aria-label="Contact email" required type="email" placeholder="Contact email" value={form.contactEmail} onChange={set('contactEmail')} className={`${inputClass} sm:col-span-2`} />

          <input aria-label="Base address line" required placeholder="Base address line" value={form.baseAddressLine} onChange={set('baseAddressLine')} className={`${inputClass} sm:col-span-2`} />
          <input aria-label="Base city" required placeholder="Base city" value={form.baseCity} onChange={set('baseCity')} className={inputClass} />
          <select aria-label="Base district" value={form.baseDistrictId || ''} onChange={set('baseDistrictId')} className={inputClass}>
            <option value="">Select base district</option>
            {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <input aria-label="Postal code" placeholder="Postal code" value={form.basePostalCode || ''} onChange={set('basePostalCode')} className={inputClass} />

          <label className="flex flex-col gap-1 text-xs text-body/60">
            Fleet size
            <input type="number" min="0" value={form.fleetSize} onChange={set('fleetSize')} className={inputClass} />
          </label>
          <label className="flex flex-col gap-1 text-xs text-body/60">
            Max daily pickups
            <input type="number" min="0" value={form.maxDailyPickups} onChange={set('maxDailyPickups')} className={inputClass} />
          </label>
          <label className="flex flex-col gap-1 text-xs text-body/60">
            Max vehicle capacity (kg)
            <input type="number" min="0" value={form.maxVehicleCapacityKg ?? ''} onChange={set('maxVehicleCapacityKg')} className={inputClass} />
          </label>
          <div className="grid grid-cols-2 gap-2">
            <label className="flex flex-col gap-1 text-xs text-body/60">
              Operating start hour
              <input type="number" min="0" max="23" value={form.operatingDayStartHour ?? ''} onChange={set('operatingDayStartHour')} className={inputClass} />
            </label>
            <label className="flex flex-col gap-1 text-xs text-body/60">
              Operating end hour
              <input type="number" min="0" max="23" value={form.operatingDayEndHour ?? ''} onChange={set('operatingDayEndHour')} className={inputClass} />
            </label>
          </div>

          <div className="flex flex-wrap gap-4 sm:col-span-2">
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.offersCashOnDelivery} onChange={set('offersCashOnDelivery')} />
              Cash on delivery
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.offersColdChain} onChange={set('offersColdChain')} />
              Cold chain
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.offersFragileHandling} onChange={set('offersFragileHandling')} />
              Fragile handling
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input type="checkbox" checked={form.isAcceptingRequests} onChange={set('isAcceptingRequests')} />
              Currently accepting requests
            </label>
          </div>

          <textarea aria-label="Notes" rows={3} placeholder="Notes" value={form.notes || ''} onChange={set('notes')} className={`${inputClass} sm:col-span-2`} />

          <div className="sm:col-span-2"><MutationFeedback mutation={upsert} /></div>

          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={upsert.isPending}>
            {upsert.isPending ? 'Saving…' : profile ? 'Save Profile' : 'Create Company Profile'}
          </Button>
      </form>

      {profile && (
        <div className="mt-8 rounded-xl border border-border bg-surface p-6">
          <h3 className="mb-4 text-sm font-semibold text-heading">Service Areas</h3>

          <div className="mb-4 space-y-2">
            {profile.serviceAreas.length === 0 && (
              <p className="text-sm text-body/50">No service areas added yet. Add districts you deliver to below.</p>
            )}
            {profile.serviceAreas.map((area) => (
              <div key={area.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-sm">
                <span>
                  {area.districtName} ({area.division}) — {area.standardDeliveryDays}d
                  {area.supportsSameDay && ' · same-day'}
                  {area.surchargeAmount ? ` · +৳${area.surchargeAmount}` : ''}
                </span>
                <button
                  type="button"
                  onClick={() => removeServiceArea.mutate(area.id)}
                  disabled={removeServiceArea.isPending}
                  className="text-xs text-danger hover:underline"
                >
                  Remove
                </button>
              </div>
            ))}
          </div>

          <form onSubmit={handleAddServiceArea} className="flex flex-wrap items-end gap-3">
            <select aria-label="District Id"
              value={areaForm.districtId}
              onChange={(e) => setAreaForm((prev) => ({ ...prev, districtId: e.target.value }))}
              className={inputClass}
            >
              <option value="">Add district…</option>
              {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
            <label className="flex flex-col gap-1 text-xs text-body/60">
              Delivery days
              <input
                type="number"
                min="1"
                value={areaForm.standardDeliveryDays}
                onChange={(e) => setAreaForm((prev) => ({ ...prev, standardDeliveryDays: e.target.value }))}
                className={`${inputClass} w-24`}
              />
            </label>
            <label className="flex items-center gap-2 text-sm text-body/70">
              <input
                type="checkbox"
                checked={areaForm.supportsSameDay}
                onChange={(e) => setAreaForm((prev) => ({ ...prev, supportsSameDay: e.target.checked }))}
              />
              Same-day
            </label>
            <label className="flex flex-col gap-1 text-xs text-body/60">
              Surcharge (৳)
              <input
                type="number"
                min="0"
                value={areaForm.surchargeAmount}
                onChange={(e) => setAreaForm((prev) => ({ ...prev, surchargeAmount: e.target.value }))}
                className={`${inputClass} w-28`}
              />
            </label>
            <Button type="submit" variant="secondary" disabled={upsertServiceArea.isPending || !areaForm.districtId}>
              {upsertServiceArea.isPending ? 'Adding…' : 'Add Area'}
            </Button>
          </form>
        </div>
      )}
    </div>
  );
}
