import { PageHeader, ChartPlaceholder, Table } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { adminUsers } from '../../data/mockData';

export default function AdminDashboard() {
  return (
    <div>
      <PageHeader title="Admin Dashboard" description="Platform-wide overview and moderation queue." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Users" value="12,400" trend="+320 this week" />
        <StatCard label="Pending Approvals" value="18" />
        <StatCard label="Active Listings" value="3,120" />
        <StatCard label="Flagged Content" value="5" />
      </div>
      <div className="mb-6 grid gap-6 lg:grid-cols-2">
        <ChartPlaceholder title="User Growth" type="line" />
        <ChartPlaceholder title="Marketplace Volume" type="bar" />
      </div>
      <p className="mb-3 text-sm font-semibold text-heading">Recent Signups</p>
      <Table
        columns={['name', 'role', 'status', 'joined']}
        rows={adminUsers.map((u) => ({ name: u.name, role: u.role, status: u.status, joined: u.joined }))}
      />
    </div>
  );
}
