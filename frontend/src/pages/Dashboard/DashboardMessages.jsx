import { PageHeader } from '../../components/ui';
import { messages } from '../../data/mockData';

export default function DashboardMessages() {
  return (
    <div>
      <PageHeader title="Messages" description="Conversations with producers, partners and support." />
      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {messages.map((message) => (
          <div key={message.id} className="flex items-start gap-3 p-4">
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
              {message.from.slice(0, 1)}
            </span>
            <div className="min-w-0 flex-1">
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold text-heading">{message.from}</p>
                <p className="text-xs text-body/50">{message.time}</p>
              </div>
              <p className="mt-1 truncate text-sm text-body/70">{message.preview}</p>
            </div>
            {message.unread && <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary" />}
          </div>
        ))}
      </div>
    </div>
  );
}
