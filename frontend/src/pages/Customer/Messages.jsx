import { useState } from 'react';
import { PageHeader, Button } from '../../components/ui';
import { conversations } from '../../data/mockData';

export default function Messages() {
  const [activeId, setActiveId] = useState(conversations[0]?.id);
  const [draft, setDraft] = useState('');
  const active = conversations.find((c) => c.id === activeId) || conversations[0];

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
            <div className="flex-1 space-y-3 overflow-y-auto p-4">
              {active.thread.map((msg) => (
                <div key={msg.id} className={`flex ${msg.self ? 'justify-end' : 'justify-start'}`}>
                  <div
                    className={`max-w-[75%] rounded-xl px-3 py-2 text-sm ${
                      msg.self ? 'bg-primary text-surface' : 'border border-border bg-background text-body'
                    }`}
                  >
                    {msg.text}
                  </div>
                </div>
              ))}
            </div>
            <form
              onSubmit={(event) => {
                event.preventDefault();
                setDraft('');
              }}
              className="flex gap-2 border-t border-border p-3"
            >
              <input
                value={draft}
                onChange={(event) => setDraft(event.target.value)}
                placeholder="Write a message…"
                className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <Button type="submit" variant="primary">
                Send
              </Button>
            </form>
          </div>
        )}
      </div>
    </div>
  );
}
