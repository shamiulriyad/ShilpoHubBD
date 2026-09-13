import { Link } from 'react-router-dom';
import { footerLinks } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';

const columns = [
  { title: 'About', key: 'about' },
  { title: 'Explore Heritage', key: 'explore' },
  { title: 'Marketplace', key: 'marketplace' },
  { title: 'Resources', key: 'resources' },
];

export default function Footer() {
  return (
    <footer className="border-t border-title/10 bg-title text-surface">
      <div className="mx-auto max-w-7xl px-4 py-14 lg:px-8">
        <div className="grid grid-cols-2 gap-8 sm:grid-cols-3 lg:grid-cols-5">
          <div className="col-span-2 sm:col-span-3 lg:col-span-1">
            <Link to={routePaths.home} className="flex items-center gap-2 text-xl font-bold tracking-[-0.04em] text-surface">
              <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-xs">শি</span> ShilpoHub
            </Link>
            <p className="mt-3 text-sm leading-6 text-surface/65">
              A heritage ecosystem connecting artisans, producers, tourists and partners across Bangladesh.
            </p>
          </div>
          {columns.map((col) => (
            <div key={col.key}>
              <p className="text-sm font-semibold text-surface">{col.title}</p>
              <ul className="mt-3 space-y-2">
                {footerLinks[col.key].map((link) => (
                  <li key={link.label}>
                    <Link to={link.path} className="text-sm text-surface/60 transition hover:text-[#F3C79D]">
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>
      <div className="border-t border-surface/10">
        <div className="mx-auto max-w-7xl px-4 py-5 text-xs text-surface/50 lg:px-8">
          © {new Date().getFullYear()} ShilpoHub. All rights reserved.
        </div>
      </div>
    </footer>
  );
}
