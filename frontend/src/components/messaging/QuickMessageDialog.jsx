import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../ui';
import { useMessagingMutations } from '../../hooks/useMessaging';
import { routePaths } from '../../routes/routePaths';

// A small popup to start (or continue) a direct conversation with someone -- e.g. the producer
// talking to a customer about payment and order status right after accepting a custom order.
export default function QuickMessageDialog({ open, recipientId, recipientName, title, hint, onClose }) {
  const { startConversation } = useMessagingMutations();
  const [body, setBody] = useState('');
  const inputRef = useRef(null);

  useEffect(() => {
    if (!open) return undefined;
    setBody('');
    startConversation.reset();
    inputRef.current?.focus();
    const onKey = (event) => {
      if (event.key === 'Escape') onClose?.();
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  if (!open) return null;

  const send = (event) => {
    event.preventDefault();
    if (!body.trim()) return;
    startConversation.mutate({ recipientId, body: body.trim() });
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <button type="button" aria-label="Close" className="absolute inset-0 bg-title/40" onClick={onClose} />
      <div role="dialog" aria-modal="true" aria-labelledby="quick-message-title" className="relative w-full max-w-md rounded-xl border border-border bg-surface p-5 shadow-xl">
        <h2 id="quick-message-title" className="text-base font-semibold text-heading">{title || `Message ${recipientName}`}</h2>
        {hint && <p className="mt-1 text-sm text-body/70">{hint}</p>}
        {startConversation.isSuccess ? (
          <div className="mt-4 space-y-3">
            <p className="text-sm text-success">Message sent to {recipientName}. They have been notified.</p>
            <div className="flex justify-end gap-2">
              <Link to={routePaths.dashboardMessages} className="self-center text-sm text-primary hover:underline">Open inbox</Link>
              <Button type="button" variant="secondary" onClick={onClose}>Done</Button>
            </div>
          </div>
        ) : (
          <form onSubmit={send} className="mt-4 space-y-3">
            <textarea
              ref={inputRef}
              aria-label="Message"
              rows={4}
              required
              maxLength={2000}
              placeholder="Write about payment, order status, timeline…"
              value={body}
              onChange={(e) => setBody(e.target.value)}
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
            {startConversation.isError && <p role="alert" className="text-sm text-error">Could not send the message. Please try again.</p>}
            <div className="flex justify-end gap-2">
              <Button type="button" variant="secondary" onClick={onClose}>Not now</Button>
              <Button type="submit" variant="primary" disabled={startConversation.isPending || !body.trim()}>
                {startConversation.isPending ? 'Sending…' : 'Send message'}
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
