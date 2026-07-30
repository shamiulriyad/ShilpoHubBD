export default function StatCard({ label, value, trend }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <p className="text-xs font-medium uppercase tracking-wide text-body/60">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-heading">{value}</p>
      {trend && <p className="mt-1 text-xs font-medium text-success">{trend}</p>}
    </div>
  );
}
