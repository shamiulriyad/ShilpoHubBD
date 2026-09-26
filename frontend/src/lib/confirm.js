// Promise-based "are you sure?" that works from any click handler:
//   if (await confirmAction('Delete this route?')) remove.mutate(id);
// The dialog itself is rendered once by <ConfirmHost /> (mounted in main.jsx).
export const CONFIRM_EVENT = 'shilpohub:confirm';

export function confirmAction(message, options = {}) {
  if (typeof window === 'undefined') return Promise.resolve(true);
  return new Promise((resolve) => {
    window.dispatchEvent(
      new CustomEvent(CONFIRM_EVENT, {
        detail: {
          title: options.title || 'Are you sure?',
          message,
          confirmLabel: options.confirmLabel || 'Yes, continue',
          cancelLabel: options.cancelLabel || 'No, go back',
          resolve,
        },
      }),
    );
  });
}
