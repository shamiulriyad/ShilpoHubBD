import { notifications } from '../../data/mockData';

export default function NotificationPanel({ className = '' }) {
  return (
    <aside className={`w-full shrink-0 space-y-4 lg:w-80 lg:border-l lg:border-border lg:pl-6 ${className}`}>
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-heading">Notifications</h3>
        <button type="button" className="text-xs font-medium text-link hover:underline">
          Mark all read
        </button>
      </div>
      <ul className="space-y-3">
        {notifications.map((item) => (
          <li
            key={item.id}
            className={`rounded-lg border border-border p-3 ${item.read ? 'bg-surface' : 'bg-primary/5'}`}
          >
            <p className="text-sm text-heading">{item.title}</p>
            <p className="mt-1 text-xs text-body/50">{item.time}</p>
          </li>
        ))}
      </ul>
    </aside>
  );
}
