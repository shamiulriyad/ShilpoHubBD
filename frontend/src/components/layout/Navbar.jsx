import { useState } from 'react';
import { Link, NavLink } from 'react-router-dom';
import { mainNav, megaMenus } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';
import { useAuth } from '../../contexts/AuthContext';
import Button from '../ui/Button';
import MegaMenu from './MegaMenu';
import ProfileDropdown from './ProfileDropdown';

export default function Navbar() {
  const [activeMenu, setActiveMenu] = useState(null);
  const [mobileOpen, setMobileOpen] = useState(false);
  const { isAuthenticated } = useAuth();

  return (
    <header className="sticky top-0 z-40 border-b border-border bg-surface/95 backdrop-blur">
      <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3 lg:px-8">
        <Link to={routePaths.home} className="flex shrink-0 items-center gap-2 text-lg font-bold text-title">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-sm text-surface">
            SH
          </span>
          ShilpoHub
        </Link>

        <nav className="hidden items-center gap-1 lg:flex" onMouseLeave={() => setActiveMenu(null)}>
          {mainNav.map((item) => (
            <div key={item.label} className="relative" onMouseEnter={() => setActiveMenu(item.menu || null)}>
              <NavLink
                to={item.path}
                className={({ isActive }) =>
                  `flex items-center gap-1 rounded-md px-3 py-2 text-sm font-medium text-body hover:bg-background hover:text-heading ${
                    isActive ? 'text-primary' : ''
                  }`
                }
              >
                {item.label}
                {item.menu && (
                  <span aria-hidden="true" className="text-[10px]">
                    ▾
                  </span>
                )}
              </NavLink>
            </div>
          ))}
        </nav>

        <div className="hidden items-center gap-2 lg:flex">
          {isAuthenticated ? (
            <ProfileDropdown />
          ) : (
            <>
              <Link to={routePaths.login}>
                <Button variant="secondary">Login</Button>
              </Link>
              <Link to={routePaths.register}>
                <Button variant="primary">Register</Button>
              </Link>
            </>
          )}
        </div>

        <button
          type="button"
          className="rounded-md border border-border p-2 text-body lg:hidden"
          onClick={() => setMobileOpen((open) => !open)}
          aria-label="Toggle menu"
          aria-expanded={mobileOpen}
        >
          {mobileOpen ? '✕' : '☰'}
        </button>
      </div>

      {activeMenu && (
        <div onMouseEnter={() => setActiveMenu(activeMenu)} onMouseLeave={() => setActiveMenu(null)}>
          <MegaMenu menu={megaMenus[activeMenu]} />
        </div>
      )}

      {mobileOpen && (
        <MobileMenu isAuthenticated={isAuthenticated} onNavigate={() => setMobileOpen(false)} />
      )}
    </header>
  );
}

function MobileMenu({ isAuthenticated, onNavigate }) {
  return (
    <div className="border-t border-border bg-surface px-4 py-4 lg:hidden">
      <nav className="space-y-1">
        {mainNav.map((item) =>
          item.menu ? (
            <details key={item.label} className="group rounded-lg">
              <summary className="flex cursor-pointer list-none items-center justify-between rounded-lg px-3 py-2 text-sm font-medium text-body hover:bg-background">
                {item.label}
                <span aria-hidden="true" className="text-[10px] group-open:rotate-180">
                  ▾
                </span>
              </summary>
              <div className="ml-3 mt-1 space-y-1 border-l border-border pl-3">
                {megaMenus[item.menu].links.map((link) => (
                  <Link
                    key={link.label}
                    to={link.path}
                    onClick={onNavigate}
                    className="block rounded-lg px-3 py-2 text-sm text-body/80 hover:bg-background"
                  >
                    {link.label}
                  </Link>
                ))}
              </div>
            </details>
          ) : (
            <Link
              key={item.label}
              to={item.path}
              onClick={onNavigate}
              className="block rounded-lg px-3 py-2 text-sm font-medium text-body hover:bg-background"
            >
              {item.label}
            </Link>
          ),
        )}
      </nav>

      <div className="mt-4 flex gap-2 border-t border-border pt-4">
        {isAuthenticated ? (
          <Link to={routePaths.dashboard} onClick={onNavigate} className="w-full">
            <Button variant="primary" className="w-full">
              Go to Dashboard
            </Button>
          </Link>
        ) : (
          <>
            <Link to={routePaths.login} onClick={onNavigate} className="w-full">
              <Button variant="secondary" className="w-full">
                Login
              </Button>
            </Link>
            <Link to={routePaths.register} onClick={onNavigate} className="w-full">
              <Button variant="primary" className="w-full">
                Register
              </Button>
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
