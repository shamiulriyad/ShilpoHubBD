import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { userMenu } from '../../data/navigation';
import { useAuth } from '../../hooks/useAuth';
import { getApiErrorMessage } from '../../utils/apiError';
import { roleLabel, roleHomePath } from '../../utils/roles';

export default function ProfileDropdown() {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [switching, setSwitching] = useState(null);
  const [switchError, setSwitchError] = useState('');
  const { user, roles, activeRole, homePath, switchRole, logout } = useAuth();

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
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-semibold text-white">
          {(user?.name || 'U').slice(0, 1).toUpperCase()}
        </span>
        <span className="hidden min-w-0 text-left sm:block">
          <span className="block truncate text-sm font-medium leading-tight text-body">{user?.name || 'Account'}</span>
          {activeRole && <span className="block truncate text-xs leading-tight text-primary">{roleLabel(activeRole)}</span>}
        </span>
        <span aria-hidden="true" className="shrink-0 text-xs text-body/50">▾</span>
      </button>

      {open && (
        <div role="menu" className="absolute right-0 top-full z-50 mt-2 w-[min(18rem,calc(100vw-2rem))] rounded-xl border border-border bg-surface p-2 shadow-lg">
          <div className="border-b border-border px-3 pb-3 pt-2">
            <p className="truncate text-sm font-semibold text-heading">{user?.name || 'Account'}</p>
            {user?.email && <p className="truncate text-xs text-body/60">{user.email}</p>}
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
                void logout();
                navigate('/login', { replace: true });
              }}
              className="mt-1 block w-full rounded-lg px-3 py-2.5 text-left text-sm text-primary hover:bg-background"
            >
              Logout
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
