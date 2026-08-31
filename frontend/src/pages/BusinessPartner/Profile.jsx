import { useEffect, useState } from 'react';
import { PageHeader, Button } from '../../components/ui';
import { useAuth } from '../../hooks/useAuth';
import { useBusinessPartnerProfile, useUpsertBusinessPartnerProfile } from '../../hooks/useBusinessPartners';
import { useDistricts } from '../../hooks/useDistricts';
import { useCategories } from '../../hooks/useCategories';

const businessTypes = ['Retailer', 'Garments', 'Exporter', 'Hotel', 'InteriorDesigner', 'FurnitureCompany', 'FashionBrand', 'Restaurant', 'Other'];
const businessSizes = ['Micro', 'Small', 'Medium', 'Large', 'Enterprise'];

export default function Profile() {
  const { user } = useAuth();
  const profileQuery = useBusinessPartnerProfile(user?.id);
  const upsert = useUpsertBusinessPartnerProfile(user?.id);
  const districtsQuery = useDistricts();
  const categoriesQuery = useCategories();

  const [form, setForm] = useState({
    businessType: 'Retailer',
    companyName: '',
    registrationNumber: '',
    industry: '',
    businessSize: 'Small',
    companyDescription: '',
    addressLine: '',
    city: '',
    districtId: '',
    country: 'Bangladesh',
    contactPersonName: '',
    contactPhone: '',
    contactEmail: '',
    preferredCategoryIds: [],
  });

  useEffect(() => {
    if (profileQuery.data) {
      setForm((prev) => ({ ...prev, ...profileQuery.data }));
    }
  }, [profileQuery.data]);

  const handleSubmit = (event) => {
    event.preventDefault();
    upsert.mutate({ ...form, documents: [] });
  };

  const set = (field) => (event) => setForm((prev) => ({ ...prev, [field]: event.target.value }));

  return (
    <div>
      <PageHeader title="Company Profile" description="Keep your company details up to date for producers and admins to see." />

      {profileQuery.isLoading ? (
        <p className="py-10 text-center text-sm text-body/60">Loading…</p>
      ) : (
        <form onSubmit={handleSubmit} className="grid gap-4 rounded-xl border border-border bg-surface p-6 sm:grid-cols-2">
          <select value={form.businessType} onChange={set('businessType')} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
            {businessTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <select value={form.businessSize} onChange={set('businessSize')} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
            {businessSizes.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
          <input required placeholder="Company name" value={form.companyName} onChange={set('companyName')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Registration number" value={form.registrationNumber} onChange={set('registrationNumber')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Industry" value={form.industry} onChange={set('industry')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <select value={form.districtId} onChange={set('districtId')} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
            <option value="">Select district</option>
            {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
          </select>
          <input required placeholder="City" value={form.city} onChange={set('city')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Address line" value={form.addressLine} onChange={set('addressLine')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Contact person name" value={form.contactPersonName} onChange={set('contactPersonName')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required placeholder="Contact phone" value={form.contactPhone} onChange={set('contactPhone')} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required type="email" placeholder="Contact email" value={form.contactEmail} onChange={set('contactEmail')} className="rounded-md border border-border bg-background px-3 py-2 text-sm sm:col-span-2" />
          <textarea required rows={3} placeholder="Company description" value={form.companyDescription} onChange={set('companyDescription')} className="rounded-md border border-border bg-background px-3 py-2 text-sm sm:col-span-2" />

          <div className="sm:col-span-2">
            <p className="mb-2 text-sm font-medium text-body/70">Preferred product categories</p>
            <div className="flex flex-wrap gap-2">
              {(categoriesQuery.data || []).map((c) => (
                <label key={c.id} className="flex items-center gap-2 rounded-full border border-border bg-background px-3 py-1.5 text-sm">
                  <input
                    type="checkbox"
                    checked={form.preferredCategoryIds?.includes(c.id)}
                    onChange={(e) => setForm((prev) => ({
                      ...prev,
                      preferredCategoryIds: e.target.checked
                        ? [...(prev.preferredCategoryIds || []), c.id]
                        : (prev.preferredCategoryIds || []).filter((id) => id !== c.id),
                    }))}
                  />
                  {c.name}
                </label>
              ))}
            </div>
          </div>

          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={upsert.isPending}>
            {upsert.isPending ? 'Saving…' : 'Save Profile'}
          </Button>
        </form>
      )}
    </div>
  );
}
