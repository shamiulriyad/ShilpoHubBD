import { PageHeader } from '../../components/ui';

export default function DashboardNotifications() {
  return (
    <div>
      <PageHeader title="Notifications" description="Stay up to date with your ShilpoHub activity." />
      <p className="rounded-xl border border-border bg-surface p-8 text-center text-sm text-body/60">No notifications are available.</p>
    </div>
  );
}
