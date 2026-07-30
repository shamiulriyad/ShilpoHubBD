import { useState } from 'react';
import { Link } from 'react-router-dom';
import { userMenu } from '../../data/navigation';
import { useAuth } from '../../contexts/AuthContext';

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
        className="flex items-center gap-2 rounded-full border border-border py-1 pl-1 pr-3 hover:bg-background"
      >
        <span className="flex h-7 w-7 items-center justify-center rounded-full bg-primary text-xs font-semibold text-surface">
          {(user?.name || 'U').slice(0, 1).toUpperCase()}
        </span>
        <span className="text-sm font-medium text-body">{user?.name || 'Account'}</span>
        <span aria-hidden="true" className="text-[10px] text-body/50">▾</span>
      </button>

      {open && (
        <div className="absolute right-0 top-full z-40 mt-2 w-56 rounded-xl border border-border bg-surface p-2 shadow-lg">
          {userMenu.map((item) => (
            <Link
              key={item.label}
              to={item.path}
              onClick={() => setOpen(false)}
              className="block rounded-lg px-3 py-2 text-sm text-body hover:bg-background"
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
            className="mt-1 block w-full rounded-lg px-3 py-2 text-left text-sm text-primary hover:bg-background"
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
}
