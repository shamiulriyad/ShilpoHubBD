import { useState } from 'react';
import { Link } from 'react-router-dom';
import { userMenu } from '../../data/navigation';
import { useAuth } from '../../hooks/useAuth';

export default function ProfileDropdown() {
  const [open, setOpen] = useState(false);
  const { user, logout } = useAuth();

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
        className="flex items-center gap-2 rounded-full border border-border py-1.5 pl-1.5 pr-3.5 hover:bg-background"
      >
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-sm font-semibold text-surface">
          {(user?.name || 'U').slice(0, 1).toUpperCase()}
        </span>
        <span className="text-base font-medium text-body">{user?.name || 'Account'}</span>
        <span aria-hidden="true" className="text-xs text-body/50">▾</span>
      </button>

      {open && (
        <div className="absolute right-0 top-full z-40 mt-2 w-56 rounded-xl border border-border bg-surface p-2 shadow-lg">
          {userMenu.map((item) => (
            <Link
              key={item.label}
              to={item.path}
              onClick={() => setOpen(false)}
              className="block rounded-lg px-3 py-2.5 text-base text-body hover:bg-background"
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
            className="mt-1 block w-full rounded-lg px-3 py-2.5 text-left text-base text-primary hover:bg-background"
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
}
