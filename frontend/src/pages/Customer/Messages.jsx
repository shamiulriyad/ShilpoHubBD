import { PageHeader } from '../../components/ui';
import DirectMessages from '../../components/messaging/DirectMessages';

export default function Messages() {
  return (
    <div>
      <PageHeader title="Messages" description="Direct conversations with producers. You can send pictures too." />
      <DirectMessages />
    </div>
  );
}
