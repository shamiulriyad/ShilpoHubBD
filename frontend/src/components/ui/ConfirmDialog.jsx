import { useEffect, useRef, useId } from 'react';
import Button from './Button';

// Small modal that asks before a hard-to-undo action (logout, delete). Focus starts on the safe
// "cancel" choice; Escape and clicking the backdrop also cancel.
export default function ConfirmDialog({
  open,
  title,
  message,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  busy = false,
  onConfirm,
  onCancel,
}) {
  const cancelRef = useRef(null);
  const dialogRef = useRef(null);
  const titleId = useId();
  const messageId = useId();

  useEffect(() => {
    if (!open) return undefined;
    const previousFocus = document.activeElement;
    cancelRef.current?.focus();
    const onKey = (event) => {
      if (event.key === 'Escape' && !busy) onCancel?.();
      if (event.key === 'Tab') {
        const targets = [...(dialogRef.current?.querySelectorAll('button:not(:disabled), a[href], input:not(:disabled)') || [])];
        const first = targets[0], last = targets[targets.length - 1];
        if (!first) { event.preventDefault(); dialogRef.current?.focus(); }
        else if (event.shiftKey && (document.activeElement === first || !dialogRef.current?.contains(document.activeElement))) { event.preventDefault(); last.focus(); }
        else if (!event.shiftKey && (document.activeElement === last || !dialogRef.current?.contains(document.activeElement))) { event.preventDefault(); first.focus(); }
      }
    };
    document.addEventListener('keydown', onKey);
    return () => { document.removeEventListener('keydown', onKey); previousFocus?.focus?.(); };
  }, [open, onCancel, busy]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <button type="button" tabIndex={-1} disabled={busy} aria-label={cancelLabel} className="absolute inset-0 bg-title/40" onClick={onCancel} />
      <div
        role="alertdialog"
        ref={dialogRef}
        tabIndex={-1}
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={messageId}
        className="relative w-full max-w-sm rounded-xl border border-border bg-surface p-5 shadow-xl"
      >
        <h2 id={titleId} className="text-base font-semibold text-heading">{title}</h2>
        <p id={messageId} className="mt-2 text-sm text-body/80">{message}</p>
        <div className="mt-5 flex justify-end gap-2">
          <button
            ref={cancelRef}
            type="button"
            onClick={onCancel}
            disabled={busy}
            className="rounded-lg border border-border bg-surface px-4 py-2 text-sm font-semibold text-title hover:bg-primary-soft disabled:opacity-50"
          >
            {cancelLabel}
          </button>
          <Button type="button" onClick={onConfirm} disabled={busy}>{busy ? 'Please wait…' : confirmLabel}</Button>
        </div>
      </div>
    </div>
  );
}
