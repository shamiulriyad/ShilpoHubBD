import { useState } from 'react';
import { Link } from 'react-router-dom';
import { userMenu } from '../../data/navigation';
import { useAuth } from '../../hooks/useAuth';
import { roleLabel, roleHomePath } from '../../utils/roles';

export default function ProfileDropdown() {
  const [open, setOpen] = useState(false);
  const [switching, setSwitching] = useState(null);
  const { user, roles, activeRole, switchRole, logout } = useAuth();

  const otherRoles = (roles || []).filter((r) => r !== activeRole);

  const handleSwitch = async (role) => {
    setSwitching(role);
    try {
      await switchRole(role);
      window.location.assign(roleHomePath(role));
    } catch {
      setSwitching(null);
    }
  };

  return (
    <div
      className="relative"
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false);
      }}
    >
      <button
        type="button"
        onClick={() => setOpen((current) => !current)}
        className="flex items-center gap-2 rounded-full border border-border py-1.5 pl-1.5 pr-3 hover:bg-background"
      >
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-sm font-semibold text-surface">
          {(user?.name || 'U').slice(0, 1).toUpperCase()}
        </span>
        <span className="hidden text-left sm:block">
          <span className="block text-sm font-medium leading-tight text-body">{user?.name || 'Account'}</span>
          {activeRole && (
            <span className="block text-[11px] leading-tight text-primary">{roleLabel(activeRole)}</span>
          )}
        </span>
        {activeRole && (
          <span className="rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-medium text-primary sm:hidden">
            {roleLabel(activeRole)}
          </span>
        )}
        <span aria-hidden="true" className="text-xs text-body/50">▾</span>
      </button>

      {open && (
        <div className="absolute right-0 top-full z-40 mt-2 w-64 rounded-xl border border-border bg-surface p-2 shadow-lg">
          <div className="border-b border-border px-3 pb-3 pt-2">
            <p className="truncate text-sm font-semibold text-heading">{user?.name || 'Account'}</p>
            {user?.email && <p className="truncate text-xs text-body/60">{user.email}</p>}
            {activeRole && (
              <p className="mt-2">
                <span className="inline-flex items-center gap-1.5 rounded-full bg-primary/10 px-2.5 py-1 text-[11px] font-medium text-primary">
                  <span aria-hidden="true">●</span>
                  Signed in as {roleLabel(activeRole)}
                </span>
              </p>
            )}
          </div>

          {otherRoles.length > 0 && (
            <div className="border-b border-border py-2">
              <p className="px-3 pb-1 text-[11px] font-semibold uppercase tracking-wide text-body/40">
                Switch workspace
              </p>
              {otherRoles.map((role) => (
                <button
                  key={role}
                  type="button"
                  disabled={Boolean(switching)}
                  onClick={() => handleSwitch(role)}
                  className="block w-full rounded-lg px-3 py-2 text-left text-sm text-body hover:bg-background disabled:opacity-50"
                >
                  {switching === role ? 'Switching…' : roleLabel(role)}
                </button>
              ))}
            </div>
          )}

          <div className="py-1">
            {userMenu.map((item) => (
              <Link
                key={item.label}
                to={item.path}
                onClick={() => setOpen(false)}
                className="block rounded-lg px-3 py-2.5 text-sm text-body hover:bg-background"
              >
                {item.label}
              </Link>
            ))}
            <button
              type="button"
              onClick={() => {
                logout();
                setOpen(false);
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
