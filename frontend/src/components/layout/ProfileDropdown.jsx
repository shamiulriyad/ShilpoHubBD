import { useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { userMenu } from '../../data/navigation';
import { useAuth } from '../../hooks/useAuth';
import { useMyProfile, useProfilePhoto } from '../../hooks/useProfile';
import { getApiErrorMessage } from '../../utils/apiError';
import { roleLabel, roleHomePath } from '../../utils/roles';
import { ConfirmDialog } from '../ui';
import UserAvatar from '../profile/UserAvatar';
import { useLogoutFlow } from '../../hooks/useLogoutFlow';

export default function ProfileDropdown() {
  const navigate = useNavigate();
  const { data: profile } = useMyProfile();
  const { upload } = useProfilePhoto();
  const photoInput = useRef(null);
  const picking = useRef(false);
  const [photoError, setPhotoError] = useState('');
  const hasPhoto = Boolean(profile?.photoUrl);
  // Opening the file dialog moves focus out of the menu; without this flag the blur handler would close (and unmount) it first.
  const pickPhoto = () => { picking.current = true; photoInput.current?.click(); };
  const photoChosen = (event) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    picking.current = false;
    if (!file) return;
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) return setPhotoError('Choose a JPG, PNG or WebP image.');
    if (file.size > 5 * 1024 * 1024) return setPhotoError('Choose an image smaller than 5 MB.');
    setPhotoError('');
    upload.mutate(file, { onError: (error) => setPhotoError(getApiErrorMessage(error, 'Unable to upload the photo.')) });
  };
  const [open, setOpen] = useState(false);
  const [switching, setSwitching] = useState(null);
  const [switchError, setSwitchError] = useState('');
  const { user, roles, activeRole, homePath, switchRole } = useAuth();
  const logoutFlow = useLogoutFlow();

  const otherRoles = (roles || []).filter((role) => role !== activeRole);

  const handleSwitch = async (role) => {
    setSwitching(role);
    setSwitchError('');
    try {
      await switchRole(role);
      setOpen(false);
      navigate(roleHomePath(role), { replace: true });
    } catch (error) {
      setSwitchError(getApiErrorMessage(error, 'Unable to switch workspace.'));
    } finally {
      setSwitching(null);
    }
  };

  const menuItems = userMenu.map((item) =>
    item.label === 'Dashboard' && homePath ? { ...item, path: homePath } : item,
  );

  return (
    <div
      className="relative"
      onBlur={(event) => {
        if (picking.current) return;
        if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false);
      }}
      onKeyDown={(event) => {
        if (event.key === 'Escape') setOpen(false);
      }}
    >
      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        className="profile-trigger"
        aria-haspopup="menu"
        aria-expanded={open}
      >
        <UserAvatar className="profile-avatar" />
        <span className="hidden min-w-0 text-left sm:block">
          <span className="block truncate text-[13px] font-semibold leading-tight text-title">{user?.name || 'Account'}</span>
          {activeRole && <span className="mt-0.5 block truncate text-[11.5px] font-medium leading-tight text-primary">{roleLabel(activeRole)}</span>}
        </span>
        <svg viewBox="0 0 24 24" className="h-4 w-4 shrink-0 text-muted" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>
      </button>

      {open && (
        <div role="menu" className="profile-menu">
          <div className="border-b border-border px-3 pb-3 pt-2">
            <div className="flex items-center gap-3">
              <UserAvatar className="profile-avatar profile-avatar-lg" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-semibold text-heading">{user?.name || 'Account'}</p>
                {user?.email && <p className="truncate text-xs text-body/60">{user.email}</p>}
                <button type="button" role="menuitem" onClick={pickPhoto} disabled={upload.isPending} className="mt-1 text-xs font-semibold text-primary hover:underline disabled:opacity-60">
                  {upload.isPending ? 'Uploading…' : hasPhoto ? 'Change photo' : 'Add photo'}
                </button>
              </div>
            </div>
            {photoError && <p role="alert" className="mt-2 text-xs text-error">{photoError}</p>}
            {activeRole && (
              <p className="mt-2">
                <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">
                  <span aria-hidden="true">●</span>
                  Signed in as {roleLabel(activeRole)}
                </span>
              </p>
            )}
          </div>

          {otherRoles.length > 0 && (
            <div className="border-b border-border py-2">
              <p className="px-3 pb-1 text-xs font-semibold uppercase tracking-wide text-body/40">Switch workspace</p>
              {otherRoles.map((role) => (
                <button
                  key={role}
                  type="button"
                  role="menuitem"
                  disabled={Boolean(switching)}
                  onClick={() => handleSwitch(role)}
                  className="block w-full rounded-lg px-3 py-2 text-left text-sm text-body hover:bg-background disabled:opacity-50"
                >
                  {switching === role ? 'Switching…' : roleLabel(role)}
                </button>
              ))}
              {switchError && <p role="alert" className="px-3 py-2 text-xs text-error">{switchError}</p>}
            </div>
          )}

          <div className="py-1">
            {menuItems.map((item) => (
              <Link
                key={item.label}
                to={item.path}
                role="menuitem"
                onClick={() => setOpen(false)}
                className="block rounded-lg px-3 py-2.5 text-sm text-body hover:bg-background"
              >
                {item.label}
              </Link>
            ))}
            <button
              type="button"
              role="menuitem"
              onClick={() => {
                setOpen(false);
                logoutFlow.request();
              }}
              className="mt-1 block w-full rounded-lg px-3 py-2.5 text-left text-sm text-primary hover:bg-background"
            >
              Logout
            </button>
          </div>
        </div>
      )}
      <input ref={photoInput} type="file" accept="image/jpeg,image/png,image/webp" className="sr-only" tabIndex={-1} aria-label="Choose a profile photo" onChange={photoChosen} onCancel={() => { picking.current = false; }} />
      <ConfirmDialog
        open={logoutFlow.confirming}
        title="Log out of ShilpoHub?"
        message="You will be signed out and taken to the home page. Any unsaved changes will be lost."
        confirmLabel="Log out"
        cancelLabel="Stay signed in"
        busy={logoutFlow.busy}
        onConfirm={logoutFlow.confirm}
        onCancel={logoutFlow.cancel}
      />
    </div>
  );
}
