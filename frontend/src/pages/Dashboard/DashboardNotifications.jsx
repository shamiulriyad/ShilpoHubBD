import { PageHeader } from '../../components/ui';
import NotificationCenter from '../../components/notifications/NotificationCenter';

export default function DashboardNotifications() {
  return (
    <div className="mx-auto max-w-4xl">
      <PageHeader title="Notifications" description="Stay up to date with your ShilpoHub activity." />
      <NotificationCenter />
    </div>
  );
}
