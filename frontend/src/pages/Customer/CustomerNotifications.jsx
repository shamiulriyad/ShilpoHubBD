import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { notifications } from '../../data/mockData';

export default function CustomerNotifications() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Notifications' }]}
        title="Notifications"
        description="Stay up to date with orders, messages and heritage updates."
        action={<Button variant="secondary">Mark all as read</Button>}
      />
      <ul className="space-y-3">
        {notifications.map((item) => (
          <li
            key={item.id}
            className={`rounded-xl border border-border p-4 ${item.read ? 'bg-surface' : 'bg-primary/5'}`}
          >
            <p className="text-sm font-medium text-heading">{item.title}</p>
            <p className="mt-1 text-xs text-body/50">{item.time}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
