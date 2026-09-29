import { useRef } from 'react';
import { useChatImageUpload } from '../../hooks/useMessaging';
import { API_BASE_URL } from '../../config/runtime';

export const resolveUploadUrl = (url) => {
  if (!url) return '';
  if (/^https?:/i.test(url)) return url;
  // API_BASE_URL ends with /api; uploads are served from the API host root.
  return `${API_BASE_URL.replace(/\/api$/, '')}${url}`;
};

// Paperclip-style button: picks a picture, uploads it and hands back its URL.
export default function ImageAttachButton({ onUploaded, disabled = false, label = 'Attach picture', compact = false }) {
  const inputRef = useRef(null);
  const upload = useChatImageUpload();

  const pick = (event) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file) return;
    if (!/^image\/(jpeg|png|webp)$/.test(file.type)) {
      upload.reset();
      window.alert('Please choose a JPG, PNG or WebP picture.');
      return;
    }
    if (file.size > 8 * 1024 * 1024) {
      window.alert('Please choose a picture smaller than 8 MB.');
      return;
    }
    upload.mutate(file, { onSuccess: (data) => onUploaded(data.url) });
  };

  return (
    <>
      <input ref={inputRef} type="file" accept="image/jpeg,image/png,image/webp" className="sr-only" aria-label={label} onChange={pick} />
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={disabled || upload.isPending}
        className={compact ? "flex h-11 w-11 shrink-0 items-center justify-center rounded-full text-heading hover:bg-background disabled:opacity-50" : "rounded-md border border-border bg-surface px-3 py-2 text-sm text-body hover:bg-background disabled:opacity-50"}
        title={label} aria-label={label}
      >
        {upload.isPending ? "…" : compact ? <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" className="h-6 w-6"><path d="m21 11-9 9a6 6 0 0 1-8.5-8.5l9-9a4 4 0 0 1 5.7 5.7l-9 9a2 2 0 0 1-2.8-2.8l8.5-8.5" /></svg> : "📎 Picture"}
      </button>
      {upload.isError && <span role="alert" className="text-xs text-error">Upload failed. Try a smaller JPG, PNG or WebP.</span>}
    </>
  );
}
