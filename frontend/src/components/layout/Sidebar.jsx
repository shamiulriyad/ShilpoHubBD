import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { roleLabel } from '../../utils/roles';

const STORAGE_KEY = 'sh:sidebar:collapsed';

function readCollapsed() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}') || {};
  } catch {
    return {};
  }
}

function writeCollapsed(next) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  } catch {
    /* storage unavailable — ignore */
  }
}

function NavItem({ item }) {
  return (
    <NavLink
      to={item.path}
      end
      className={({ isActive }) =>
        `group relative flex items-center gap-2.5 rounded-lg py-2 pl-3 pr-2 text-sm font-medium transition-colors ${
          isActive
            ? 'bg-primary/10 text-primary'
            : 'text-body/80 hover:bg-background hover:text-body'
        }`
      }
    >
      {({ isActive }) => (
        <>
          <span
            aria-hidden="true"
            className={`absolute left-0 top-1/2 h-5 w-1 -translate-y-1/2 rounded-r-full bg-primary transition-opacity ${
              isActive ? 'opacity-100' : 'opacity-0'
            }`}
          />
          {item.icon && (
            <span
              aria-hidden="true"
              className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-md text-[13px] ${
                isActive ? 'bg-primary/15' : 'bg-background group-hover:bg-surface'
              }`}
            >
              {item.icon}
            </span>
          )}
          <span className="truncate">{item.label}</span>
        </>
      )}
    </NavLink>
  );
}

function FlatNav({ items }) {
  return (
    <nav className="space-y-1">
      {items.map((item) => (
        <NavItem key={item.label} item={item} />
      ))}
    </nav>
  );
}

function GroupedNav({ groups }) {
  const [collapsed, setCollapsed] = useState(readCollapsed);

  const toggle = (section) => {
    setCollapsed((prev) => {
      const next = { ...prev, [section]: !prev[section] };
      writeCollapsed(next);
      return next;
    });
  };

  return (
    <nav className="space-y-5">
      {groups.map((group) => {
        const isCollapsed = Boolean(collapsed[group.section]);
        return (
          <div key={group.section}>
            <button
              type="button"
              onClick={() => toggle(group.section)}
              className="mb-1.5 flex w-full items-center justify-between px-3 text-[11px] font-semibold uppercase tracking-wider text-body/40 hover:text-body/70"
            >
              <span>{group.section}</span>
              <span aria-hidden="true" className={`transition-transform ${isCollapsed ? '' : 'rotate-90'}`}>
                ›
              </span>
            </button>
            {!isCollapsed && (
              <div className="space-y-1">
                {group.items.map((item) => (
                  <NavItem key={item.label} item={item} />
                ))}
              </div>
            )}
          </div>
        );
      })}
    </nav>
  );
}

export default function Sidebar({ items = [], title = 'Menu', className = '' }) {
  const grouped = items.length > 0 && Array.isArray(items[0]?.items);
  const { activeRole } = useAuth();

  return (
    <aside
      className={`w-full shrink-0 lg:sticky lg:top-[4.5rem] lg:max-h-[calc(100vh-6rem)] lg:w-64 lg:overflow-y-auto lg:border-r lg:border-border lg:pr-4 ${className}`}
    >
      <div className="mb-4 flex items-center gap-2.5 rounded-xl border border-border bg-surface px-3 py-2.5">
        <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs font-bold text-surface">
          {title.slice(0, 1).toUpperCase()}
        </span>
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-heading">{title} workspace</p>
          <p className="truncate text-[11px] font-medium text-primary">
            {activeRole ? `Role: ${roleLabel(activeRole)}` : 'Signed in'}
          </p>
        </div>
      </div>

      {grouped ? <GroupedNav groups={items} /> : <FlatNav items={items} />}
    </aside>
  );
}
