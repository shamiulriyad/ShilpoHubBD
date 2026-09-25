export default function StatCard({ label, value, trend }) {
  return <div className="stat-card"><p className="stat-label">{label}</p><p className="stat-value">{value}</p>{trend&&<p className="stat-trend">{trend}</p>}</div>;
}
