import { PageHeader, ChartPlaceholder } from '../../components/ui';
import { StatCard } from '../../components/cards';

export default function DashboardAnalytics() {
  return (
    <div>
      <PageHeader title="Analytics" description="Performance and engagement insights." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Profile Views" value="3,240" trend="+12% this month" />
        <StatCard label="Conversion Rate" value="4.8%" trend="+0.6%" />
        <StatCard label="Repeat Customers" value="212" trend="+18" />
        <StatCard label="Avg. Rating" value="4.7 / 5" />
      </div>
      <div className="grid gap-6 lg:grid-cols-2">
        <ChartPlaceholder title="Traffic Over Time" type="line" />
        <ChartPlaceholder title="Sales by Category" type="donut" />
      </div>
    </div>
  );
}
