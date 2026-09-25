export default function DashboardCard({ title, description, action, children }) {
  return (
    <div className="dashboard-card">
      <div className="mb-3 flex items-center justify-between">
        <div>
          <h3 className="text-sm font-semibold text-heading">{title}</h3>
          {description && <p className="mt-1 text-sm text-muted">{description}</p>}
        </div>
        {action}
      </div>
      {children}
    </div>
  );
}
