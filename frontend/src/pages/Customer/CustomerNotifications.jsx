import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import NotificationCenter from '../../components/notifications/NotificationCenter';

export default function CustomerNotifications() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Notifications' }]}
        title="Notifications"
        description="Stay up to date with orders, messages and heritage updates."
      />
      <NotificationCenter />
    </div>
  );
}
