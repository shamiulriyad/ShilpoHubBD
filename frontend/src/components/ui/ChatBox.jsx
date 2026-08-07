import { useState } from 'react';
import Button from './Button';

export default function ChatBox({ title, messages = [], onSend, placeholder = 'Say something…', bordered = true, className = '' }) {
  const [draft, setDraft] = useState('');

  function handleSubmit(event) {
    event.preventDefault();
    if (!draft.trim()) return;
    onSend?.(draft.trim());
    setDraft('');
  }

  return (
    <div className={`flex flex-col ${bordered ? 'rounded-xl border border-border bg-surface' : ''} ${className}`}>
      {title && <p className="border-b border-border p-4 text-sm font-semibold text-heading">{title}</p>}
      <div className="flex-1 space-y-3 overflow-y-auto p-4">
        {messages.map((msg) => (
          <div key={msg.id} className={`flex ${msg.self ? 'justify-end' : 'justify-start'}`}>
            <div
              className={`max-w-[75%] rounded-xl px-3 py-2 text-sm ${
                msg.self ? 'bg-primary text-surface' : 'border border-border bg-background text-body'
              }`}
            >
              {!msg.self && <span className="mr-1 font-medium text-heading">{msg.from}:</span>}
              {msg.text}
            </div>
          </div>
        ))}
      </div>
      <form onSubmit={handleSubmit} className="flex gap-2 border-t border-border p-3">
        <input
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          placeholder={placeholder}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <Button type="submit" variant="primary">
          Send
        </Button>
      </form>
    </div>
  );
}
