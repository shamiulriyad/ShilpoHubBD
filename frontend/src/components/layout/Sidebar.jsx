import { NavLink } from 'react-router-dom';

export default function Sidebar({ items, title = 'Menu', className = '' }) {
  return (
    <aside className={`w-full shrink-0 lg:w-64 lg:border-r lg:border-border lg:pr-4 ${className}`}>
      <p className="mb-3 px-3 text-xs font-semibold uppercase tracking-wide text-body/50">{title}</p>
      <nav className="space-y-1">
        {items.map((item) => (
          <NavLink
            key={item.label}
            to={item.path}
            end
            className={({ isActive }) =>
              `block rounded-lg px-3 py-2 text-sm font-medium ${
                isActive ? 'bg-primary/10 text-primary' : 'text-body hover:bg-background'
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
