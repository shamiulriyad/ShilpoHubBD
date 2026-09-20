import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';

export default function CustomerNotifications() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Notifications' }]}
        title="Notifications"
        description="Stay up to date with orders, messages and heritage updates."
      />
      <p className="rounded-xl border border-border bg-surface p-8 text-center text-sm text-body/60">No notifications are available.</p>
    </div>
  );
}
