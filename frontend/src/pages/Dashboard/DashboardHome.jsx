import { PageHeader, ChartPlaceholder, Table } from '../../components/ui';
import { DashboardCard, StatCard } from '../../components/cards';
import { useConversations } from '../../hooks/useMessaging';

const listOf = (data) => data?.items || data || [];

// TODO(backend): no notifications feed or "recent members" directory endpoint yet.
const recentNotifications = [
  { id: 'n1', title: 'New order received', time: '1h ago' },
  { id: 'n2', title: 'Course enrollment confirmed', time: '2h ago' },
  { id: 'n3', title: 'Your listing was approved', time: '5h ago' },
  { id: 'n4', title: 'Festival reminder: Jamdani Mela', time: '1d ago' },
];

const recentMembers = [
  { name: 'Rahima Begum', role: 'Artisan', status: 'Active', joined: '2026-01-04' },
  { name: 'Abdul Karim', role: 'Farmer', status: 'Pending', joined: '2026-02-08' },
  { name: 'Shefali Rani', role: 'Customer', status: 'Active', joined: '2026-03-11' },
];

export default function DashboardHome() {
  const conversationsQuery = useConversations();
  const unreadMessages = listOf(conversationsQuery.data).reduce(
    (sum, c) => sum + (c.unreadCount || 0),
    0,
  );

  const stats = [
    { label: 'Active Listings', value: '—' },
    { label: 'Orders This Month', value: '—' },
    { label: 'Course Progress', value: '—' },
    { label: 'Unread Messages', value: unreadMessages },
  ];

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
            {recentNotifications.map((item) => (
              <li key={item.id} className="text-sm text-body">
                {item.title}
                <span className="ml-2 text-xs text-body/50">{item.time}</span>
              </li>
            ))}
          </ul>
        </DashboardCard>
      </div>

      <DashboardCard title="Recent Community Members" description="Newest producers and partners on the platform">
        <Table columns={['name', 'role', 'status', 'joined']} rows={recentMembers} />
      </DashboardCard>
    </div>
  );
}
