import { PageHeader } from '../../components/ui';
import { notifications } from '../../data/mockData';

export default function DashboardNotifications() {
  return (
    <div>
      <PageHeader title="Notifications" description="Stay up to date with your ShilpoHub activity." />
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
