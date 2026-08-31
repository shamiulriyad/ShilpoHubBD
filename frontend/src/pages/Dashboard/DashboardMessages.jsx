import { PageHeader, AsyncState } from '../../components/ui';
import { useConversations } from '../../hooks/useMessaging';

const listOf = (data) => data?.items || data || [];

export default function DashboardMessages() {
  const { data, isLoading, isError, error } = useConversations();
  const conversations = listOf(data);

  return (
    <div>
      <PageHeader title="Messages" description="Conversations with producers, partners and support." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {conversations.map((conversation) => {
            const name = conversation.otherUserName || 'Conversation';
            return (
              <div key={conversation.id} className="flex items-start gap-3 p-4">
                <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                  {name.slice(0, 1)}
                </span>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-semibold text-heading">{name}</p>
                    <p className="text-xs text-body/50">
                      {conversation.lastMessageAt
                        ? new Date(conversation.lastMessageAt).toLocaleDateString()
                        : ''}
                    </p>
                  </div>
                  <p className="mt-1 truncate text-sm text-body/70">{conversation.lastMessageBody}</p>
                </div>
                {conversation.unreadCount > 0 && (
                  <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary" />
                )}
              </div>
            );
          })}
          {conversations.length === 0 && (
            <p className="p-4 text-sm text-body/60">No conversations yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
