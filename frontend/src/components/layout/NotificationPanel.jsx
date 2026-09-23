import NotificationCenter from '../notifications/NotificationCenter';
export default function NotificationPanel({ className = '' }) {
  return (
    <aside className={`w-full shrink-0 space-y-4 lg:w-80 lg:border-l lg:border-border lg:pl-6 ${className}`}>
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-heading">Notifications</h3>
      </div>
      <NotificationCenter compact />
    </aside>
  );
}
