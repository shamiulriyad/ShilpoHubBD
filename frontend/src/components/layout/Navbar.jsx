import { useEffect, useRef, useState } from 'react';
import { Link, NavLink } from 'react-router-dom';
import { mainNav, megaMenus } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';
import { useAuth } from '../../hooks/useAuth';
import Button from '../ui/Button';
import MegaMenu from './MegaMenu';
import ProfileDropdown from './ProfileDropdown';
import NotificationBell from '../notifications/NotificationBell';
import BrandLogo from '../brand/BrandLogo';

export default function Navbar() {
  const [activeMenu, setActiveMenu] = useState(null);
  const [mobileOpen, setMobileOpen] = useState(false);
  const closeTimer = useRef(null);
  const { isAuthenticated, homePath } = useAuth();

  const cancelClose = () => { if (closeTimer.current) window.clearTimeout(closeTimer.current); };
  const scheduleClose = () => { cancelClose(); closeTimer.current = window.setTimeout(() => setActiveMenu(null), 180); };
  useEffect(() => () => cancelClose(), []);

  return (
    <header className="sticky top-0 z-40 border-b border-border/80 bg-surface/90 backdrop-blur-xl">
      <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3.5 lg:px-8">
        <Link to={routePaths.home} className="flex shrink-0 items-center gap-3 text-xl font-bold tracking-[-0.04em] text-title">
          <BrandLogo />
        </Link>

        <nav className="hidden items-center gap-1 lg:flex" onMouseEnter={cancelClose} onMouseLeave={scheduleClose} onKeyDown={(event) => { if (event.key === 'Escape') setActiveMenu(null); }}>
          {mainNav.map((item) => (
            <div key={item.label} className="relative" onMouseEnter={() => { cancelClose(); setActiveMenu(item.menu || null); }}>
              <NavLink
                to={item.path}
                onFocus={() => item.menu && setActiveMenu(item.menu)}
                aria-haspopup={item.menu ? 'true' : undefined}
                aria-expanded={item.menu ? activeMenu === item.menu : undefined}
                className={({ isActive }) =>
                  `flex items-center gap-1 rounded-full px-3.5 py-2 text-sm font-semibold text-body/75 transition hover:bg-primary-soft hover:text-heading ${
                    isActive ? 'bg-primary-soft text-primary' : ''
                  }`
                }
              >
                {item.label}
                {item.menu && (
                  <span aria-hidden="true" className="text-xs">
                    ▾
                  </span>
                )}
              </NavLink>
            </div>
          ))}
        </nav>

        <div className="hidden items-center gap-2 lg:flex">
          {isAuthenticated ? (
            <div className="flex items-center gap-3"><NotificationBell /><ProfileDropdown /></div>
          ) : (
            <>
              <Link to={routePaths.login}>
                <Button variant="secondary" size="lg">Login</Button>
              </Link>
              <Link to={routePaths.register}>
                <Button variant="primary" size="lg">Register</Button>
              </Link>
            </>
          )}
        </div>

        <button
          type="button"
          className="rounded-xl border border-border bg-surface p-2.5 text-lg text-body shadow-sm lg:hidden"
          onClick={() => setMobileOpen((open) => !open)}
          aria-label="Toggle menu"
          aria-expanded={mobileOpen}
        >
          {mobileOpen ? '✕' : '☰'}
        </button>
      </div>

      {activeMenu && (
        <div className="border-t border-border/70 bg-surface/95 shadow-xl" onMouseEnter={cancelClose} onMouseLeave={scheduleClose}>
          <MegaMenu menu={megaMenus[activeMenu]} />
        </div>
      )}

      {mobileOpen && (
        <MobileMenu isAuthenticated={isAuthenticated} homePath={homePath} onNavigate={() => setMobileOpen(false)} />
      )}
    </header>
  );
}

function MobileMenu({ isAuthenticated, homePath, onNavigate }) {
  return (
    <div className="border-t border-border bg-surface px-4 py-4 lg:hidden">
      <nav className="space-y-1">
        {mainNav.map((item) =>
          item.menu ? (
            <details key={item.label} className="group rounded-lg">
              <summary className="flex cursor-pointer list-none items-center justify-between rounded-lg px-3 py-2.5 text-base font-medium text-body hover:bg-background">
                {item.label}
                <span aria-hidden="true" className="text-xs group-open:rotate-180">
                  ▾
                </span>
              </summary>
              <div className="ml-3 mt-1 space-y-1 border-l border-border pl-3">
                {megaMenus[item.menu].links.map((link) => (
                  <Link
                    key={link.label}
                    to={link.path}
                    onClick={onNavigate}
                    className="block rounded-lg px-3 py-2 text-base text-body/80 hover:bg-background"
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
              className="block rounded-lg px-3 py-2.5 text-base font-medium text-body hover:bg-background"
            >
              {item.label}
            </Link>
          ),
        )}
      </nav>

      <div className="mt-4 flex gap-2 border-t border-border pt-4">
        {isAuthenticated ? (
          <Link to={homePath || routePaths.dashboard} onClick={onNavigate} className="w-full">
            <Button variant="primary" size="lg" className="w-full">
              Go to Dashboard
            </Button>
          </Link>
        ) : (
          <>
            <Link to={routePaths.login} onClick={onNavigate} className="w-full">
              <Button variant="secondary" size="lg" className="w-full">
                Login
              </Button>
            </Link>
            <Link to={routePaths.register} onClick={onNavigate} className="w-full">
              <Button variant="primary" size="lg" className="w-full">
                Register
              </Button>
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
