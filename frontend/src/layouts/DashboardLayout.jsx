import { useEffect, useState } from 'react';
import { Link, Outlet } from 'react-router-dom';
import Footer from '../components/layout/Footer';
import Sidebar from '../components/layout/Sidebar';
import ProfileDropdown from '../components/layout/ProfileDropdown';
import { sidebarNav, roleSidebars } from '../data/navigation';
import { routePaths } from '../routes/routePaths';
import { useAuth } from '../hooks/useAuth';
import BrandLogo from '../components/brand/BrandLogo';
import { AIAssistantWidget } from '../components/ui';

export default function DashboardLayout({ navItems, sidebarTitle }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  useEffect(() => { const close = event => { if (event.key === 'Escape') setSidebarOpen(false); }; window.addEventListener('keydown',close); return () => window.removeEventListener('keydown',close); }, []);
  const { activeRole } = useAuth();

  const roleConfig = activeRole ? roleSidebars[activeRole] : null;
  const items = navItems ?? roleConfig?.nav ?? sidebarNav;
  const title = sidebarTitle ?? roleConfig?.title ?? 'Workspace';

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-40 flex h-[4.5rem] items-center gap-3 border-b border-border bg-surface px-4 py-3 lg:px-6">
        <button
          type="button"
          className="rounded-md border border-border p-2 text-body lg:hidden"
          onClick={() => setSidebarOpen(true)}
          aria-label="Open workspace navigation"
          aria-expanded={sidebarOpen}
          aria-controls="workspace-sidebar"
        >
          ☰
        </button>
        <Link to={activeRole === 'Tourist' ? routePaths.tourist : routePaths.home} className="flex shrink-0 items-center gap-2 text-base font-bold text-title">
          <BrandLogo size="sm" className="[&>span:last-child]:hidden sm:[&>span:last-child]:inline" />
        </Link>
        <div className="ml-auto">
          <ProfileDropdown />
        </div>
      </header>

      {sidebarOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-title/35 lg:hidden"
          onClick={() => setSidebarOpen(false)}
          aria-label="Close workspace navigation"
        />
      )}

      <div className="mx-auto flex max-w-[1600px] items-start gap-6 px-4 py-6 lg:px-6">
        <div
          id="workspace-sidebar"
          className={`fixed inset-y-0 left-0 z-50 w-[min(86vw,19rem)] overflow-y-auto bg-surface p-4 shadow-xl transition-transform lg:sticky lg:top-24 lg:max-h-[calc(100dvh-7rem)] lg:z-auto lg:block lg:w-64 lg:shrink-0 lg:translate-x-0 lg:overflow-y-auto lg:visible overscroll-contain [scrollbar-width:thin] lg:bg-transparent lg:p-0 lg:shadow-none ${
            sidebarOpen ? 'visible translate-x-0' : 'invisible -translate-x-full'
          }`}
        >
          <div className="mb-4 flex justify-end lg:hidden">
            <button
              type="button"
              onClick={() => setSidebarOpen(false)}
              className="rounded-md border border-border px-3 py-2 text-sm text-body"
              aria-label="Close workspace navigation"
            >
              Close
            </button>
          </div>
          <Sidebar items={items} title={title} onNavigate={() => setSidebarOpen(false)} />
        </div>

        <main className="min-w-0 flex-1">
          <Outlet />
        </main>
      </div>
      <Footer />
      <AIAssistantWidget />
    </div>
  );
}
