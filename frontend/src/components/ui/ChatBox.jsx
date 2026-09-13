import { useState } from 'react';
import Button from './Button';

export default function ChatBox({ title, messages = [], onSend, placeholder = 'Say something…', bordered = true, className = '' }) {
  const [draft, setDraft] = useState('');
  const canSend = typeof onSend === 'function';

  function handleSubmit(event) {
    event.preventDefault();
    const message = draft.trim();
    if (!canSend || !message) return;
    onSend(message);
    setDraft('');
  }

  return (
    <div className={`flex flex-col ${bordered ? 'rounded-xl border border-border bg-surface' : ''} ${className}`}>
      {title && <p className="border-b border-border p-4 text-sm font-semibold text-heading">{title}</p>}
      <div className="flex-1 space-y-3 overflow-y-auto p-4" aria-live="polite">
        {messages.map((message) => (
          <div key={message.id} className={`flex ${message.self ? 'justify-end' : 'justify-start'}`}>
            <div
              className={`max-w-[82%] break-words rounded-xl px-3 py-2 text-sm ${
                message.self ? 'bg-primary text-surface' : 'border border-border bg-background text-body'
              }`}
            >
              {!message.self && <span className="mr-1 font-medium text-heading">{message.from}:</span>}
              {message.text}
            </div>
          </div>
        ))}
        {messages.length === 0 && <p className="text-sm text-body/50">No messages yet.</p>}
      </div>
      {canSend ? (
        <form onSubmit={handleSubmit} className="flex gap-2 border-t border-border p-3">
          <label htmlFor="chat-message" className="sr-only">Chat message</label>
          <input aria-label="Draft"
            id="chat-message"
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            placeholder={placeholder}
            maxLength={1000}
            className="min-w-0 flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <Button type="submit" variant="primary" disabled={!draft.trim()}>
            Send
          </Button>
        </form>
      ) : (
        <p className="border-t border-border p-3 text-xs text-body/50">Chat is available only while this event is live.</p>
      )}
    </div>
  );
}
