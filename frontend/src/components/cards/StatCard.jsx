export default function StatCard({ label, value, trend }) {
  return (
    <div className="premium-card group relative overflow-hidden p-5 transition duration-300 hover:-translate-y-1 hover:shadow-[0_18px_38px_rgba(45,44,36,0.10)]">
      <span className="absolute right-0 top-0 h-16 w-16 rounded-bl-full bg-primary-soft transition group-hover:scale-125" />
      <p className="relative text-[11px] font-bold uppercase tracking-[0.12em] text-body/55">{label}</p>
      <p className="relative mt-2 text-3xl font-bold tracking-[-0.04em] text-heading">{value}</p>
      {trend && <p className="mt-1 text-xs font-medium text-success">{trend}</p>}
    </div>
  );
}