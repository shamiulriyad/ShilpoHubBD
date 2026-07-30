export default function DashboardCard({ title, description, action, children }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <div className="mb-3 flex items-center justify-between">
        <div>
          <h3 className="text-sm font-semibold text-heading">{title}</h3>
          {description && <p className="text-xs text-body/60">{description}</p>}
        </div>
        {action}
      </div>
      {children}
    </div>
  );
}
