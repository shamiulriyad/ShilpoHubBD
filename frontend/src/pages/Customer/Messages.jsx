import { useEffect, useState } from 'react';
import { PageHeader, ChatBox, AsyncState } from '../../components/ui';
import { useConversations, useConversation, useMessagingMutations } from '../../hooks/useMessaging';
import { useAuth } from '../../hooks/useAuth';

export default function Messages() {
  const { user } = useAuth();
  const [activeId, setActiveId] = useState(null);
  const conversationsQuery = useConversations();
  const conversationQuery = useConversation(activeId);
  const { sendMessage, markAsRead } = useMessagingMutations();

  const conversations = conversationsQuery.data?.items || [];
  const active = conversations.find((c) => c.id === activeId);

  useEffect(() => {
    if (!activeId && conversations.length > 0) {
      setActiveId(conversations[0].id);
    }
  }, [activeId, conversations]);

  useEffect(() => {
    if (activeId) markAsRead.mutate(activeId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeId]);

  const messages = (conversationQuery.data?.messages || []).map((message) => ({
    id: message.id,
    from: message.senderName,
    text: message.body,
    self: message.senderId === user?.id,
  }));

  return (
    <div>
      <PageHeader title="Messages" description="Conversations with the producers you’ve contacted." />

      <div className="grid h-[560px] grid-cols-1 overflow-hidden rounded-xl border border-border bg-surface sm:grid-cols-[280px_1fr]">
        <div className="divide-y divide-border overflow-y-auto border-b border-border sm:border-b-0 sm:border-r">
          <AsyncState isLoading={conversationsQuery.isLoading} isError={conversationsQuery.isError} error={conversationsQuery.error}>
            {conversations.map((conversation) => (
              <button
                key={conversation.id}
                type="button"
                onClick={() => setActiveId(conversation.id)}
                className={`flex w-full items-start gap-3 p-4 text-left transition ${
                  conversation.id === activeId ? 'bg-primary/5' : 'hover:bg-background'
                }`}
              >
                <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                  {conversation.otherUserName.slice(0, 1)}
                </span>
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between">
                    <p className="truncate text-sm font-semibold text-heading">{conversation.otherUserName}</p>
                    {conversation.lastMessageAt && (
                      <p className="text-xs text-body/50">{new Date(conversation.lastMessageAt).toLocaleDateString()}</p>
                    )}
                  </div>
                  <p className="truncate text-xs text-body/60">{conversation.lastMessageBody}</p>
                </div>
                {conversation.unreadCount > 0 && <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary" />}
              </button>
            ))}
            {conversations.length === 0 && <p className="p-6 text-center text-sm text-body/60">No conversations yet.</p>}
          </AsyncState>
        </div>

        {active && (
          <div className="flex flex-col">
            <div className="border-b border-border p-4">
              <p className="text-sm font-semibold text-heading">{active.otherUserName}</p>
            </div>
            <ChatBox
              className="flex-1"
              bordered={false}
              messages={messages}
              onSend={(text) => sendMessage.mutate({ id: activeId, body: text })}
              placeholder="Write a message…"
            />
          </div>
        )}
      </div>
    </div>
  );
}
