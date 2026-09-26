import { PageHeader } from '../../components/ui';
import DirectMessages from '../../components/messaging/DirectMessages';

export default function DashboardMessages() {
  return (
    <div>
      <PageHeader title="Messages" description="Direct conversations with customers, producers, partners and support. Reply and send pictures here." />
      <DirectMessages />
    </div>
  );
}
