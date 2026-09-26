import { Link } from 'react-router-dom';
import { footerLinks } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';
import BrandLogo from '../brand/BrandLogo';

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
              <BrandLogo size="sm" inverse />
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
        <div className="footer-bottom mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-4 px-4 py-5 text-xs text-surface/50 lg:px-8">
          <span>© {new Date().getFullYear()} ShilpoHub. All rights reserved.</span>
          <a href="tel:98675" className="footer-helpline" aria-label="Call admin helpline at 98675">
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1 1 .4 2 .7 2.9a2 2 0 0 1-.4 2.1L8.1 10a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.9.6 2.9.7a2 2 0 0 1 1.6 1.9Z"/></svg>
            <span>Admin helpline <strong>98675</strong></span>
          </a>
        </div>
      </div>
    </footer>
  );
}
