import { useRef, useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { useMyProfile, useProfilePhoto } from '../../hooks/useProfile';
import { resolveMediaUrl } from '../media/CardMedia';
import { Button } from '../ui';
import { getApiErrorMessage } from '../../utils/apiError';

const TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_BYTES = 5 * 1024 * 1024;

// Lets a member add, change or remove their own profile photo (shown in the navbar and sidebar).
export default function ProfilePhotoField() {
  const { user } = useAuth();
  const { data } = useMyProfile();
  const { upload, remove } = useProfilePhoto();
  const input = useRef(null);
  const [localError, setLocalError] = useState('');
  const src = resolveMediaUrl(data?.photoUrl);
  const busy = upload.isPending || remove.isPending;
  const error = localError || (upload.isError && getApiErrorMessage(upload.error)) || (remove.isError && getApiErrorMessage(remove.error)) || '';

  const choose = (event) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file) return;
    if (!TYPES.includes(file.type)) return setLocalError('Choose a JPG, PNG or WebP image.');
    if (file.size > MAX_BYTES) return setLocalError('Choose an image smaller than 5 MB.');
    setLocalError('');
    upload.mutate(file);
  };

  return (
    <section aria-label="Profile photo" className="flex flex-wrap items-center gap-5 rounded-xl border border-border bg-surface p-5">
      <span className="grid h-24 w-24 shrink-0 place-items-center overflow-hidden rounded-full bg-primary text-3xl font-bold text-surface ring-4 ring-primary/15">
        {src ? <img src={src} alt="Your profile photo" className="h-full w-full object-cover" /> : (user?.name || 'U').trim().slice(0, 1).toUpperCase()}
      </span>
      <div className="min-w-0 flex-1">
        <h2 className="text-base font-semibold text-heading">Profile photo</h2>
        <p className="mt-1 text-xs text-body/60">Shown in the top bar and sidebar. JPG, PNG or WebP, up to 5 MB.</p>
        <div className="mt-3 flex flex-wrap gap-2">
          <input ref={input} type="file" accept="image/jpeg,image/png,image/webp" className="sr-only" onChange={choose} aria-label="Choose a profile photo" tabIndex={-1} />
          <Button type="button" variant="primary" disabled={busy} onClick={() => input.current?.click()}>
            {upload.isPending ? 'Uploading…' : src ? 'Change photo' : 'Add photo'}
          </Button>
          {src && <Button type="button" variant="secondary" disabled={busy} onClick={() => remove.mutate()}>{remove.isPending ? 'Removing…' : 'Remove'}</Button>}
        </div>
        {error && <p role="alert" className="mt-3 text-sm text-error">{error}</p>}
      </div>
    </section>
  );
}
