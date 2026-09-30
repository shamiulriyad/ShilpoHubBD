import { PageHeader } from '../../components/ui';
import DirectMessages from '../../components/messaging/DirectMessages';

export default function Messages() {
  return (
    <div className="flex h-full min-h-[560px] flex-col lg:min-h-0">
      <div className="shrink-0">
        <PageHeader action={<div id="messages-help-slot" className="flex h-12 w-12 shrink-0 items-center justify-center" />} title="Messages" description="Direct conversations with producers. You can send pictures too." />
      </div>
      <DirectMessages className="min-h-[460px] flex-1 lg:min-h-0" />
    </div>
  );
}
