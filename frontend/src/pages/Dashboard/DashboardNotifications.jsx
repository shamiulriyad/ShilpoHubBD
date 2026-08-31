import { PageHeader } from '../../components/ui';

// TODO(backend): no notifications API yet. Editorial placeholder items.
const notifications = [
  { id: 'notif-1', title: 'New order received', time: '1h ago', read: false },
  { id: 'notif-2', title: 'Course enrollment confirmed', time: '2h ago', read: false },
  { id: 'notif-3', title: 'Your listing was approved', time: '5h ago', read: true },
  { id: 'notif-4', title: 'Festival reminder: Jamdani Mela', time: '1d ago', read: true },
  { id: 'notif-5', title: 'New message from a producer', time: '2d ago', read: true },
];

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
