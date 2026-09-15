import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, SectionHeader, Pagination } from '../../components/ui';
import { useBusinessPartnersList, useVerifyBusinessPartner } from '../../hooks/useBusinessPartners';
import { useRoleMutations } from '../../hooks/useRoles';
import { useAdminUsers, useAdminUserMutations } from '../../hooks/useAdminUsers';
import { useIdentityVerifications, useIdentityVerificationMutations } from '../../hooks/useIdentityVerification';
import { useAdminRoles, useRolePermissions, usePermissionsList, usePermissionMutations } from '../../hooks/usePermissions';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const verificationTone = { Pending: 'secondary', Verified: 'success', Rejected: 'neutral', Suspended: 'neutral', None: 'neutral', Approved: 'success' };
const roleOptions = ['Customer', 'Producer', 'BusinessPartner', 'Tourist', 'HeritageAcademyMember', 'HeritageInnovationHub', 'GovernmentNGO', 'LogisticsPartner', 'SuperAdmin'];

function DirectoryTab() {
  const [filters, setFilters] = useState({ search: '', role: '', isActive: '' });
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data, isLoading, isError, error } = useAdminUsers({
    search: filters.search || undefined,
    role: filters.role || undefined,
    isActive: filters.isActive === '' ? undefined : filters.isActive === 'true',
    page,
    pageSize,
  });
  const { activate, deactivate } = useAdminUserMutations();
  const { assign, remove } = useRoleMutations();
  const [roleForm, setRoleForm] = useState({ userId: '', role: 'Customer' });

  const users = data?.items || [];
  const totalPages = Math.max(1, Math.ceil((data?.totalCount || 0) / pageSize));

  const handleAssign = (event) => {
    event.preventDefault();
    assign.mutate(roleForm);
  };

  return (
    <div>
      <div className="mb-4 flex flex-wrap gap-2">
        <input
          placeholder="Search name or email…"
          value={filters.search}
          onChange={(e) => { setPage(1); setFilters((p) => ({ ...p, search: e.target.value })); }}
          className={`${inputClass} flex-1`}
        />
        <select
          value={filters.role}
          onChange={(e) => { setPage(1); setFilters((p) => ({ ...p, role: e.target.value })); }}
          className={inputClass}
        >
          <option value="">All roles</option>
          {roleOptions.map((r) => <option key={r} value={r}>{r}</option>)}
        </select>
        <select
          value={filters.isActive}
          onChange={(e) => { setPage(1); setFilters((p) => ({ ...p, isActive: e.target.value })); }}
          className={inputClass}
        >
          <option value="">All statuses</option>
          <option value="true">Active</option>
          <option value="false">Suspended</option>
        </select>
      </div>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="mb-4 divide-y divide-border rounded-xl border border-border bg-surface">
          {users.map((u) => (
            <div key={u.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{u.fullName}</p>
                <p className="text-xs text-body/60">{u.email} · {u.roles.join(', ') || 'No role'}</p>
              </div>
              <div className="flex items-center gap-2">
                <Badge tone={u.isActive ? 'success' : 'neutral'}>{u.isActive ? 'Active' : 'Suspended'}</Badge>
                <Badge tone={verificationTone[u.identityVerificationStatus] || 'neutral'}>ID: {u.identityVerificationStatus}</Badge>
                {u.isActive ? (
                  <Button variant="secondary" size="sm" disabled={deactivate.isPending} onClick={() => deactivate.mutate(u.id)}>Suspend</Button>
                ) : (
                  <Button variant="primary" size="sm" disabled={activate.isPending} onClick={() => activate.mutate(u.id)}>Activate</Button>
                )}
              </div>
            </div>
          ))}
          {users.length === 0 && <p className="p-6 text-center text-sm text-body/60">No users match these filters.</p>}
        </div>
        {totalPages > 1 && <Pagination currentPage={page} totalPages={totalPages} onPageChange={setPage} />}
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
          {roleOptions.map((r) => <option key={r} value={r}>{r}</option>)}
        </select>
        <Button type="submit" variant="primary" disabled={assign.isPending}>Assign Role</Button>
        <Button type="button" variant="secondary" onClick={() => remove.mutate(roleForm)} disabled={remove.isPending}>Remove Role</Button>
      </form>
    </div>
  );
}

function BusinessPartnersTab() {
  const { data, isLoading, isError, error } = useBusinessPartnersList({ pageSize: 50 });
  const verify = useVerifyBusinessPartner();
  const partners = data?.items || [];

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
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
                  <Button variant="primary" size="sm" onClick={() => verify.mutate({ userId: partner.userId, payload: { status: 'Verified' } })}>Approve</Button>
                  <Button variant="secondary" size="sm" onClick={() => verify.mutate({ userId: partner.userId, payload: { status: 'Rejected' } })}>Reject</Button>
                </>
              )}
            </div>
          </div>
        ))}
        {partners.length === 0 && <p className="p-6 text-center text-sm text-body/60">No business partner profiles yet.</p>}
      </div>
    </AsyncState>
  );
}

function IdentityVerificationTab() {
  const [status, setStatus] = useState('Pending');
  const { data, isLoading, isError, error } = useIdentityVerifications({ status: status || undefined, pageSize: 50 });
  const { approve, reject } = useIdentityVerificationMutations();
  const [rejectingId, setRejectingId] = useState(null);
  const [reason, setReason] = useState('');

  const requests = data?.items || [];

  const handleReject = (id) => {
    reject.mutate({ id, rejectionReason: reason }, { onSuccess: () => { setRejectingId(null); setReason(''); } });
  };

  return (
    <div>
      <div className="mb-4 flex gap-2">
        <select value={status} onChange={(e) => setStatus(e.target.value)} className={inputClass}>
          <option value="">All statuses</option>
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
      </div>

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {requests.map((r) => (
            <div key={r.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{r.userFullName} <span className="font-normal text-body/50">({r.userEmail})</span></p>
                  <p className="text-xs text-body/60">{r.type} · {r.documentNumber} · submitted {new Date(r.submittedAt).toLocaleDateString()}</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={verificationTone[r.status] || 'neutral'}>{r.status}</Badge>
                  {r.status === 'Pending' && (
                    <>
                      <Button variant="primary" size="sm" disabled={approve.isPending} onClick={() => approve.mutate(r.id)}>Approve</Button>
                      <Button variant="secondary" size="sm" onClick={() => setRejectingId(rejectingId === r.id ? null : r.id)}>Reject</Button>
                    </>
                  )}
                </div>
              </div>
              {r.status === 'Rejected' && r.rejectionReason && (
                <p className="mt-2 text-xs text-body/60">Reason: {r.rejectionReason}</p>
              )}
              {rejectingId === r.id && (
                <div className="mt-3 flex gap-2">
                  <input
                    required
                    placeholder="Rejection reason…"
                    value={reason}
                    onChange={(e) => setReason(e.target.value)}
                    className={`${inputClass} flex-1`}
                  />
                  <Button variant="primary" size="sm" disabled={!reason || reject.isPending} onClick={() => handleReject(r.id)}>Confirm Reject</Button>
                </div>
              )}
            </div>
          ))}
          {requests.length === 0 && <p className="p-6 text-center text-sm text-body/60">No identity verification requests.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function PermissionsTab() {
  const rolesQuery = useAdminRoles();
  const permissionsQuery = usePermissionsList();
  const [selectedRoleId, setSelectedRoleId] = useState('');
  const rolePermissionsQuery = useRolePermissions(selectedRoleId);
  const { syncRolePermissions } = usePermissionMutations();
  const [draftCodes, setDraftCodes] = useState(null);

  const roles = rolesQuery.data || [];
  const permissions = permissionsQuery.data || [];
  const grantedCodes = draftCodes ?? rolePermissionsQuery.data?.permissionCodes ?? [];

  const modules = [...new Set(permissions.map((p) => p.module))];

  const toggle = (code) => {
    const base = draftCodes ?? rolePermissionsQuery.data?.permissionCodes ?? [];
    setDraftCodes(base.includes(code) ? base.filter((c) => c !== code) : [...base, code]);
  };

  const selectRole = (id) => {
    setSelectedRoleId(id);
    setDraftCodes(null);
  };

  const save = () => {
    syncRolePermissions.mutate(
      { roleId: selectedRoleId, permissionCodes: grantedCodes },
      { onSuccess: () => setDraftCodes(null) },
    );
  };

  return (
    <div>
      <AsyncState isLoading={rolesQuery.isLoading} isError={rolesQuery.isError} error={rolesQuery.error}>
        <div className="mb-6 grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-5">
          {roles.map((role) => (
            <button
              type="button"
              key={role.id}
              onClick={() => selectRole(role.id)}
              className={`rounded-lg border p-3 text-left text-sm transition ${selectedRoleId === role.id ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:border-primary/30'}`}
            >
              <p className="font-medium text-heading">{role.name}</p>
              <p className="text-xs text-body/60">{role.userCount} users · {role.permissionCount} permissions</p>
            </button>
          ))}
        </div>
      </AsyncState>

      {selectedRoleId && (
        <AsyncState isLoading={permissionsQuery.isLoading || rolePermissionsQuery.isLoading} isError={permissionsQuery.isError} error={permissionsQuery.error}>
          <SectionHeader
            eyebrow="Permission Matrix"
            title={rolePermissionsQuery.data?.roleName || 'Role'}
            action={<Button variant="primary" disabled={syncRolePermissions.isPending} onClick={save}>Save Changes</Button>}
          />
          <div className="space-y-6">
            {modules.map((module) => (
              <div key={module}>
                <h3 className="mb-2 text-sm font-semibold text-heading">{module}</h3>
                <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
                  {permissions.filter((p) => p.module === module).map((p) => (
                    <label key={p.code} className="flex items-start gap-2 rounded-lg border border-border bg-surface p-3 text-sm">
                      <input
                        type="checkbox"
                        checked={grantedCodes.includes(p.code)}
                        onChange={() => toggle(p.code)}
                        className="mt-0.5"
                      />
                      <span>
                        <span className="block font-medium text-heading">{p.name}</span>
                        <span className="block text-xs text-body/60">{p.description}</span>
                      </span>
                    </label>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </AsyncState>
      )}
    </div>
  );
}

const tabs = [
  { id: 'directory', label: 'User Directory' },
  { id: 'partners', label: 'Business Partners' },
  { id: 'identity', label: 'Identity Verification' },
  { id: 'permissions', label: 'Roles & Permissions' },
];

export default function UserManagement() {
  const [tab, setTab] = useState('directory');

  return (
    <div>
      <PageHeader title="User Management" description="Search and manage all platform users, review verifications, and control role permissions." />

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

      {tab === 'directory' && <DirectoryTab />}
      {tab === 'partners' && <BusinessPartnersTab />}
      {tab === 'identity' && <IdentityVerificationTab />}
      {tab === 'permissions' && <PermissionsTab />}
    </div>
  );
}
