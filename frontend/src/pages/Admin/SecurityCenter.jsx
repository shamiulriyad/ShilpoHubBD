import { PageHeader, Table, Badge } from '../../components/ui';
import { StatCard } from '../../components/cards';

const logs = [
  { event: 'Failed login attempt', user: 'unknown', time: '10 min ago', severity: 'High' },
  { event: 'Password changed', user: 'Rahima Begum', time: '1 hr ago', severity: 'Low' },
  { event: 'New admin role granted', user: 'Abdul Karim', time: '3 hr ago', severity: 'Medium' },
  { event: 'Suspicious payment pattern flagged', user: 'system', time: '1 day ago', severity: 'High' },
];

const severityTone = { High: 'primary', Medium: 'secondary', Low: 'success' };

export default function SecurityCenter() {
  return (
    <div>
      <PageHeader title="Security Center" description="Access logs, roles and platform security signals." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Active Sessions" value="842" />
        <StatCard label="Admin Accounts" value="6" />
        <StatCard label="Alerts (24h)" value="3" />
        <StatCard label="2FA Enabled" value="72%" />
      </div>
      <p className="mb-3 text-sm font-semibold text-heading">Recent Activity</p>
      <Table
        columns={['event', 'user', 'time', 'severity']}
        rows={logs.map((log) => ({ ...log, severity: <Badge tone={severityTone[log.severity]}>{log.severity}</Badge> }))}
      />
    </div>
  );
}
