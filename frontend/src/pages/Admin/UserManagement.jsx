import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, SectionHeader } from '../../components/ui';
import { useBusinessPartnersList, useVerifyBusinessPartner } from '../../hooks/useBusinessPartners';
import { useRoleMutations } from '../../hooks/useRoles';

const verificationTone = { Pending: 'secondary', Verified: 'success', Rejected: 'neutral', Suspended: 'neutral' };

export default function UserManagement() {
  const { data, isLoading, isError, error } = useBusinessPartnersList({ pageSize: 50 });
  const verify = useVerifyBusinessPartner();
  const { assign, remove } = useRoleMutations();
  const [roleForm, setRoleForm] = useState({ userId: '', role: 'Customer' });

  const partners = data?.items || [];

  const handleAssign = (event) => {
    event.preventDefault();
    assign.mutate(roleForm);
  };

  return (
    <div>
      <PageHeader
        title="User Management"
        description="There is no generic user-directory endpoint on the backend — this page covers the two real admin capabilities that exist: Business Partner verification and role assignment."
      />

      <SectionHeader eyebrow="Verification Queue" title="Business Partners" />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="mb-10 divide-y divide-border rounded-xl border border-border bg-surface">
          {partners.map((partner) => (
            <div key={partner.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{partner.companyName}</p>
                <p className="text-xs text-body/60">{partner.userFullName} · {partner.businessType} · {partner.userEmail}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={verificationTone[partner.verificationStatus] || 'neutral'}>{partner.verificationStatus}</Badge>
                {partner.verificationStatus === 'Pending' && (
                  <>
                    <Button variant="primary" onClick={() => verify.mutate({ userId: partner.userId, payload: { status: 'Verified' } })}>Approve</Button>
                    <Button variant="secondary" onClick={() => verify.mutate({ userId: partner.userId, payload: { status: 'Rejected' } })}>Reject</Button>
                  </>
                )}
              </div>
            </div>
          ))}
          {partners.length === 0 && <p className="p-6 text-center text-sm text-body/60">No business partner profiles yet.</p>}
        </div>
      </AsyncState>

      <SectionHeader eyebrow="Access Control" title="Assign / Remove a Role" />
      <form onSubmit={handleAssign} className="flex flex-wrap gap-2 rounded-xl border border-border bg-surface p-4">
        <input
          required
          placeholder="User ID"
          value={roleForm.userId}
          onChange={(event) => setRoleForm((prev) => ({ ...prev, userId: event.target.value }))}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <select
          value={roleForm.role}
          onChange={(event) => setRoleForm((prev) => ({ ...prev, role: event.target.value }))}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          {['Customer', 'Producer', 'BusinessPartner', 'Tourist', 'HeritageAcademyMember', 'HeritageInnovationHub', 'GovernmentNGO', 'LogisticsPartner', 'SuperAdmin'].map((r) => (
            <option key={r} value={r}>{r}</option>
          ))}
        </select>
        <Button type="submit" variant="primary" disabled={assign.isPending}>Assign Role</Button>
        <Button type="button" variant="secondary" onClick={() => remove.mutate(roleForm)} disabled={remove.isPending}>Remove Role</Button>
      </form>
    </div>
  );
}
