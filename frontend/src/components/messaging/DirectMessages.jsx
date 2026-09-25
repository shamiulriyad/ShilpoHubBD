import { useEffect, useRef, useState } from 'react';
import { Button, AsyncState } from '../ui';
import ImageAttachButton, { resolveUploadUrl } from './ImageAttachButton';
import { useConversations, useConversation, useMessagingMutations } from '../../hooks/useMessaging';
import { useAuth } from '../../hooks/useAuth';

// Inbox + chat used by every role: conversation list, message thread, reply box and picture sending.
export default function DirectMessages() {
  const { user } = useAuth();
  const [activeId, setActiveId] = useState(null);
  const [draft, setDraft] = useState('');
  const [image, setImage] = useState('');
  const conversationsQuery = useConversations();
  const conversationQuery = useConversation(activeId);
  const { sendMessage, markAsRead } = useMessagingMutations();
  const endRef = useRef(null);

  const conversations = conversationsQuery.data?.items || [];
  const active = conversations.find((c) => c.id === activeId);
  const messages = conversationQuery.data?.messages || [];

  useEffect(() => {
    if (!activeId && conversations.length > 0) setActiveId(conversations[0].id);
  }, [activeId, conversations]);

  useEffect(() => {
    if (activeId) markAsRead.mutate(activeId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeId, messages.length]);

  useEffect(() => {
    endRef.current?.scrollIntoView?.({ block: 'end' });
  }, [messages.length, activeId]);

  const send = (event) => {
    event.preventDefault();
    if (!draft.trim() && !image) return;
    sendMessage.mutate({ id: activeId, body: draft.trim(), imageUrl: image || undefined }, {
      onSuccess: () => { setDraft(''); setImage(''); },
    });
  };

  return (
    <div className="grid h-[600px] grid-cols-1 overflow-hidden rounded-xl border border-border bg-surface sm:grid-cols-[280px_1fr]">
      <div className="divide-y divide-border overflow-y-auto border-b border-border sm:border-b-0 sm:border-r">
        <AsyncState isLoading={conversationsQuery.isLoading} isError={conversationsQuery.isError} error={conversationsQuery.error}>
          {conversations.map((conversation) => (
            <button
              key={conversation.id}
              type="button"
              onClick={() => setActiveId(conversation.id)}
              className={`flex w-full items-start gap-3 p-4 text-left transition ${conversation.id === activeId ? 'bg-primary/5' : 'hover:bg-background'}`}
            >
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                {conversation.otherUserName.slice(0, 1)}
              </span>
              <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between">
                  <p className="truncate text-sm font-semibold text-heading">{conversation.otherUserName}</p>
                  {conversation.lastMessageAt && <p className="text-xs text-body/50">{new Date(conversation.lastMessageAt).toLocaleDateString()}</p>}
                </div>
                <p className="truncate text-xs text-body/60">{conversation.lastMessageBody === 'Photo' ? '📷 Photo' : conversation.lastMessageBody}</p>
              </div>
              {conversation.unreadCount > 0 && <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary" aria-label="Unread" />}
            </button>
          ))}
          {conversations.length === 0 && <p className="p-6 text-center text-sm text-body/60">No conversations yet. Use “Message producer” on a product or profile to start one.</p>}
        </AsyncState>
      </div>

      {active ? (
        <div className="flex min-h-0 flex-col">
          <div className="border-b border-border p-4"><p className="text-sm font-semibold text-heading">{active.otherUserName}</p></div>
          <div className="flex-1 space-y-3 overflow-y-auto p-4" aria-live="polite">
            {messages.map((m) => {
              const self = m.senderId === user?.id;
              return (
                <div key={m.id} className={`flex ${self ? 'justify-end' : 'justify-start'}`}>
                  <div className={`max-w-[82%] break-words rounded-xl px-3 py-2 text-sm ${self ? 'bg-primary text-surface' : 'border border-border bg-background text-body'}`}>
                    {m.imageUrl && (
                      <a href={resolveUploadUrl(m.imageUrl)} target="_blank" rel="noreferrer">
                        <img src={resolveUploadUrl(m.imageUrl)} alt="Sent by chat" className="mb-1 max-h-56 rounded-lg object-cover" loading="lazy" />
                      </a>
                    )}
                    {m.body && <span>{m.body}</span>}
                    <span className={`mt-1 block text-[10px] ${self ? 'text-surface/70' : 'text-body/40'}`}>{new Date(m.createdAt).toLocaleString()}</span>
                  </div>
                </div>
              );
            })}
            {messages.length === 0 && <p className="text-sm text-body/50">No messages yet.</p>}
            <div ref={endRef} />
          </div>
          <form onSubmit={send} className="space-y-2 border-t border-border p-3">
            {image && (
              <div className="flex items-center gap-2">
                <img src={resolveUploadUrl(image)} alt="To be sent" className="h-14 w-14 rounded-md object-cover" />
                <button type="button" onClick={() => setImage('')} className="text-xs text-danger hover:underline">Remove picture</button>
              </div>
            )}
            <div className="flex gap-2">
              <ImageAttachButton onUploaded={setImage} disabled={sendMessage.isPending} />
              <input aria-label="Message" value={draft} onChange={(e) => setDraft(e.target.value)} placeholder="Write a message…" maxLength={4000} className="min-w-0 flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm" />
              <Button type="submit" variant="primary" disabled={sendMessage.isPending || (!draft.trim() && !image)}>{sendMessage.isPending ? 'Sending…' : 'Send'}</Button>
            </div>
            {sendMessage.isError && <p role="alert" className="text-xs text-error">Could not send. Please try again.</p>}
          </form>
        </div>
      ) : (
        <div className="hidden items-center justify-center p-6 text-sm text-body/50 sm:flex">Select a conversation.</div>
      )}
    </div>
  );
}
