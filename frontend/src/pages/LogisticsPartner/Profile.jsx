import { PageHeader } from '../../components/ui';
import { useAuth } from '../../hooks/useAuth';
import { useMyLogisticsPartnerProfile } from '../../hooks/useLogisticsPartners';
import { getApiErrorMessage } from '../../utils/apiError';

export default function LogisticsPartnerProfile() {
  const { user } = useAuth();
  const query = useMyLogisticsPartnerProfile();
  const profile = query.data;
  return <div>
    <PageHeader title="Company account" description="Your logistics account and service coverage are assigned by a ShilpoHub administrator." />
    <section className="rounded-xl border border-border bg-surface p-5">
      <p className="text-sm text-body/70">Login email: {user?.email}</p>
      {query.isLoading && <p role="status" className="mt-4">Loading company details…</p>}
      {query.isError && <p role="alert" className="mt-4">{query.error?.response?.status === 404 ? 'No company is assigned to this account. Contact your administrator to arrange access.' : getApiErrorMessage(query.error, 'Unable to load company details.')}</p>}
      {profile && <>
        <h2 className="mt-4 text-xl font-semibold text-heading">{profile.companyName}</h2>
        <p className="mt-2 text-sm">{profile.isActive ? 'Active' : 'Inactive'} · {profile.verificationStatus}</p>
        <p className="mt-2 text-body/70">{profile.description}</p>
        <dl className="mt-5 grid gap-4 text-sm sm:grid-cols-2">
          <div><dt className="text-body/60">Company contact</dt><dd>{profile.contactPersonName} · {profile.contactPhone}</dd></div>
          <div><dt className="text-body/60">Company email</dt><dd>{profile.contactEmail}</dd></div>
          <div><dt className="text-body/60">Base address</dt><dd>{profile.baseAddressLine}, {profile.baseCity}</dd></div>
          <div><dt className="text-body/60">Accepting deliveries</dt><dd>{profile.isAcceptingRequests ? 'Yes' : 'No'}</dd></div>
        </dl>
        <h3 className="mt-6 font-semibold">Assigned service coverage</h3>
        <div className="mt-3 space-y-2">{profile.serviceAreas?.map(area => <div key={area.id} className="rounded-lg border border-border p-3 text-sm">
          {area.division} → {area.districtName}{area.areaName ? ` → ${area.areaName}` : ''} · {area.deliveryMethod} · {area.standardDeliveryDays} days · ৳{area.deliveryCharge + (area.surchargeAmount || 0)} · {area.isActive ? 'Available' : 'Unavailable'}
        </div>)}</div>
        <p className="mt-5 text-sm text-body/60">Contact your administrator to change company details, coverage, charges or account access. Use the logistics workspace to manage your assigned deliveries.</p>
      </>}
    </section>
  </div>;
}
