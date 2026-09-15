import { PageHeader } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useAuth } from '../../hooks/useAuth';
import { roleLabel } from '../../utils/roles';

export default function DashboardProfile() {
  const { user, roles, activeRole } = useAuth();

  return (
    <div>
      <PageHeader
        title="Account Profile"
        description="Your account details and available workspaces."
      />
      <div className="grid gap-6 lg:grid-cols-[1fr_2fr]">
        <DashboardCard title="Overview">
          <div className="flex flex-col items-center gap-3 text-center">
            <span className="flex h-20 w-20 items-center justify-center rounded-full bg-primary/10 text-2xl font-semibold text-primary">
              {(user?.name || 'U').slice(0, 1).toUpperCase()}
            </span>
            <div className="min-w-0">
              <p className="break-words text-sm font-semibold text-heading">{user?.name || 'ShilpoHub member'}</p>
              <p className="mt-1 break-all text-xs text-body/60">{user?.email || 'No email returned'}</p>
            </div>
          </div>
        </DashboardCard>

        <DashboardCard title="Access" description="Your roles determine which workspaces and actions are available.">
          <dl className="space-y-4 text-sm">
            <div>
              <dt className="text-xs font-semibold uppercase tracking-wide text-body/45">Active workspace</dt>
              <dd className="mt-1 font-medium text-heading">{activeRole ? roleLabel(activeRole) : 'No active role'}</dd>
            </div>
            <div>
              <dt className="text-xs font-semibold uppercase tracking-wide text-body/45">Assigned roles</dt>
              <dd className="mt-2 flex flex-wrap gap-2">
                {roles.length > 0 ? roles.map((role) => (
                  <span key={role} className="rounded-full border border-border bg-background px-3 py-1.5 text-xs text-body">
                    {roleLabel(role)}
                  </span>
                )) : <span className="text-body/60">No roles returned for this account.</span>}
              </dd>
            </div>
          </dl>
          <p className="mt-5 text-xs leading-5 text-body/55">
            Account details are read-only here.
          </p>
        </DashboardCard>
      </div>
    </div>
  );
}
