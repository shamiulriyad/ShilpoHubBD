import PageHeader from '../ui/PageHeader';
import StatCard from '../cards/StatCard';
import DashboardCard from '../cards/DashboardCard';

export default function RoleOverview({ title, description, stats = [], highlights = [] }) {
  return (
    <div>
      <PageHeader title={title} description={description} />

      {stats.length > 0 && (
        <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
          {stats.map((stat) => (
            <StatCard key={stat.label} label={stat.label} value={stat.value} />
          ))}
        </div>
      )}

      {highlights.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2">
          {highlights.map((item) => (
            <DashboardCard key={item.title} title={item.title} description={item.description} />
          ))}
        </div>
      )}
    </div>
  );
}
