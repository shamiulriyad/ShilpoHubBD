import { Link } from 'react-router-dom';

export default function MegaMenu({ menu }) {
  if (!menu) return null;

  return (
    <div className="absolute inset-x-0 top-full z-30 border-b border-border bg-surface shadow-lg">
      <div className="mx-auto grid max-w-7xl gap-8 px-4 py-8 lg:grid-cols-[1fr_2fr] lg:px-8">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-primary">{menu.heading}</p>
          <p className="mt-2 max-w-xs text-sm text-body/70">{menu.description}</p>
        </div>
        <div className="grid grid-cols-2 gap-x-8 gap-y-4 sm:grid-cols-3">
          {menu.links.map((link) => (
            <Link key={link.label} to={link.path} className="group block rounded-lg p-2 hover:bg-background">
              <p className="text-sm font-medium text-heading group-hover:text-primary">{link.label}</p>
              <p className="mt-0.5 text-xs text-body/60">{link.description}</p>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
