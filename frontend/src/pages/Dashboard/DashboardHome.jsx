import { PageHeader, ChartPlaceholder, Table } from '../../components/ui';
import { DashboardCard, StatCard } from '../../components/cards';
import { adminUsers, notifications, messages } from '../../data/mockData';

const stats = [
  { label: 'Active Listings', value: '48' },
  { label: 'Orders This Month', value: '132' },
  { label: 'Course Progress', value: '3 / 6' },
  { label: 'Unread Messages', value: messages.filter((m) => m.unread).length },
];

export default function DashboardHome() {
  return (
    <div>
      <PageHeader title="Dashboard" description="An overview of your ShilpoHub activity." />

      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        {stats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} />
        ))}
      </div>

      <div className="mb-6 grid gap-6 lg:grid-cols-[2fr_1fr]">
        <ChartPlaceholder title="Activity Overview" type="bar" />
        <DashboardCard title="Recent Notifications" description="Latest updates for your account">
          <ul className="space-y-2">
            {notifications.slice(0, 4).map((item) => (
              <li key={item.id} className="text-sm text-body">
                {item.title}
                <span className="ml-2 text-xs text-body/50">{item.time}</span>
              </li>
            ))}
          </ul>
        </DashboardCard>
      </div>

      <DashboardCard title="Recent Community Members" description="Newest producers and partners on the platform">
        <Table
          columns={['name', 'role', 'status', 'joined']}
          rows={adminUsers.map((u) => ({ name: u.name, role: u.role, status: u.status, joined: u.joined }))}
        />
      </DashboardCard>
    </div>
  );
}
