import { PageHeader } from '../../components/ui';
import { DashboardCard } from '../../components/cards';

const settingSections = [
  { title: 'Profile Information', description: 'Name, bio, profile photo and contact details.' },
  { title: 'Account Security', description: 'Password, two-factor authentication and sessions.' },
  { title: 'Notification Preferences', description: 'Choose what updates you receive and how.' },
  { title: 'Language & Region', description: 'Interface language and regional format.' },
];

export default function DashboardSettings() {
  return (
    <div>
      <PageHeader title="Settings" description="Manage your account preferences." />
      <div className="space-y-4">
        {settingSections.map((section) => (
          <DashboardCard key={section.title} title={section.title} description={section.description} />
        ))}
      </div>
    </div>
  );
}
