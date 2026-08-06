import { PageHeader, Button } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useAuth } from '../../hooks/useAuth';

export default function DashboardProfile() {
  const { user } = useAuth();

  return (
    <div>
      <PageHeader
        title="Profile"
        description="How your profile appears across ShilpoHub."
        action={<Button variant="secondary">Edit Profile</Button>}
      />
      <div className="grid gap-6 lg:grid-cols-[1fr_2fr]">
        <DashboardCard title="Overview">
          <div className="flex flex-col items-center gap-3 text-center">
            <span className="flex h-20 w-20 items-center justify-center rounded-full bg-primary/10 text-2xl font-semibold text-primary">
              {(user?.name || 'U').slice(0, 1).toUpperCase()}
            </span>
            <div>
              <p className="text-sm font-semibold text-heading">{user?.name || 'Guest User'}</p>
              <p className="text-xs text-body/60">{user?.role || 'Member'}</p>
            </div>
          </div>
        </DashboardCard>
        <DashboardCard title="Bio" description="Tell the community about yourself">
          <p className="text-sm text-body/70">Profile bio placeholder text goes here.</p>
        </DashboardCard>
      </div>
    </div>
  );
}
