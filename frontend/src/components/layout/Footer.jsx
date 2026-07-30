import { Link } from 'react-router-dom';
import { footerLinks, socialLinks } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';

const columns = [
  { title: 'About', key: 'about' },
  { title: 'Explore Heritage', key: 'explore' },
  { title: 'Marketplace', key: 'marketplace' },
  { title: 'Resources', key: 'resources' },
  { title: 'Support', key: 'support' },
];

export default function Footer() {
  return (
    <footer className="border-t border-border bg-surface">
      <div className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
        <div className="grid grid-cols-2 gap-8 sm:grid-cols-3 lg:grid-cols-6">
          <div className="col-span-2 sm:col-span-3 lg:col-span-1">
            <Link to={routePaths.home} className="text-lg font-bold text-title">
              ShilpoHub
            </Link>
            <p className="mt-2 text-sm text-body/70">
              A national heritage ecosystem connecting artisans, producers, tourists and partners.
            </p>
          </div>
          {columns.map((col) => (
            <div key={col.key}>
              <p className="text-sm font-semibold text-heading">{col.title}</p>
              <ul className="mt-3 space-y-2">
                {footerLinks[col.key].map((link) => (
                  <li key={link.label}>
                    <Link to={link.path} className="text-sm text-body/70 hover:text-primary">
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>
      <div className="border-t border-border">
        <div className="mx-auto flex max-w-7xl flex-col gap-3 px-4 py-5 text-xs text-body/60 sm:flex-row sm:items-center sm:justify-between lg:px-8">
          <p>© {new Date().getFullYear()} ShilpoHub. All rights reserved.</p>
          <div className="flex items-center gap-4">
            {socialLinks.map((social) => (
              <a key={social.label} href={social.href} className="hover:text-primary">
                {social.label}
              </a>
            ))}
            <select
              defaultValue="en"
              aria-label="Language"
              className="rounded-md border border-border bg-surface px-2 py-1 text-xs text-body"
            >
              <option value="en">English</option>
              <option value="bn">বাংলা</option>
            </select>
          </div>
        </div>
      </div>
    </footer>
  );
}
