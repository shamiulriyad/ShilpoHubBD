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
export default function ImageAttachButton({ onUploaded, disabled = false, label = 'Attach picture' }) {
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
        className="rounded-md border border-border bg-surface px-3 py-2 text-sm text-body hover:bg-background disabled:opacity-50"
        title={label}
      >
        {upload.isPending ? 'Uploading…' : '📎 Picture'}
      </button>
      {upload.isError && <span role="alert" className="text-xs text-error">Upload failed. Try a smaller JPG, PNG or WebP.</span>}
    </>
  );
}
