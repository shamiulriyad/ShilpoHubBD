import { useState } from 'react';
import { PageHeader, ChatBox } from '../../components/ui';
import { conversations } from '../../data/mockData';

export default function Messages() {
  const [activeId, setActiveId] = useState(conversations[0]?.id);
  const [threads, setThreads] = useState(() =>
    Object.fromEntries(conversations.map((c) => [c.id, c.thread])),
  );
  const active = conversations.find((c) => c.id === activeId) || conversations[0];

  function sendMessage(text) {
    setThreads((prev) => ({
      ...prev,
      [active.id]: [...(prev[active.id] || []), { id: (prev[active.id]?.length || 0) + 1, from: 'You', text, self: true }],
    }));
  }

  return (
    <div>
      <PageHeader title="Messages" description="Conversations with the producers you’ve contacted." />

      <div className="grid h-[560px] grid-cols-1 overflow-hidden rounded-xl border border-border bg-surface sm:grid-cols-[280px_1fr]">
        <div className="divide-y divide-border overflow-y-auto border-b border-border sm:border-b-0 sm:border-r">
          {conversations.map((conversation) => (
            <button
              key={conversation.id}
              type="button"
              onClick={() => setActiveId(conversation.id)}
              className={`flex w-full items-start gap-3 p-4 text-left transition ${
                conversation.id === active?.id ? 'bg-primary/5' : 'hover:bg-background'
              }`}
            >
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                {conversation.producer.slice(0, 1)}
              </span>
              <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between">
                  <p className="truncate text-sm font-semibold text-heading">{conversation.producer}</p>
                  <p className="text-xs text-body/50">{conversation.time}</p>
                </div>
                <p className="truncate text-xs text-body/60">{conversation.lastMessage}</p>
              </div>
              {conversation.unread && <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary" />}
            </button>
          ))}
        </div>

        {active && (
          <div className="flex flex-col">
            <div className="border-b border-border p-4">
              <p className="text-sm font-semibold text-heading">{active.producer}</p>
              <p className="text-xs text-body/60">{active.craft}</p>
            </div>
            <ChatBox
              className="flex-1"
              bordered={false}
              messages={threads[active.id] || []}
              onSend={sendMessage}
              placeholder="Write a message…"
            />
          </div>
        )}
      </div>
    </div>
  );
}
