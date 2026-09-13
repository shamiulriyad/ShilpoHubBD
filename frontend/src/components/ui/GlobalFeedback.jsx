import { useEffect, useState } from 'react';

export const MUTATION_ERROR_EVENT = 'shilpohub:mutation-error';

export default function GlobalFeedback() {
  const [message, setMessage] = useState('');

  useEffect(() => {
    let timer;
    const handleError = (event) => {
      const nextMessage = event.detail?.message;
      if (!nextMessage) return;
      setMessage(nextMessage);
      window.clearTimeout(timer);
      timer = window.setTimeout(() => setMessage(''), 6000);
    };

    window.addEventListener(MUTATION_ERROR_EVENT, handleError);
    return () => {
      window.removeEventListener(MUTATION_ERROR_EVENT, handleError);
      window.clearTimeout(timer);
    };
  }, []);

  if (!message) return null;

  return (
    <div className="pointer-events-none fixed inset-x-3 top-3 z-[100] flex justify-center sm:inset-x-auto sm:right-4 sm:top-4">
      <div
        role="alert"
        className="pointer-events-auto flex w-full max-w-md items-start gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800 shadow-lg"
      >
        <span className="min-w-0 flex-1">{message}</span>
        <button
          type="button"
          onClick={() => setMessage('')}
          className="shrink-0 rounded px-1 font-semibold text-red-700 hover:bg-red-100"
          aria-label="Dismiss error message"
        >
          ×
        </button>
      </div>
    </div>
  );
}