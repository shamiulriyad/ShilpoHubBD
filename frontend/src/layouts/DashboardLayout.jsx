import { useState } from 'react';
import { Outlet, Link } from 'react-router-dom';
import Sidebar from '../components/layout/Sidebar';
import ProfileDropdown from '../components/layout/ProfileDropdown';
import { sidebarNav, roleSidebars } from '../data/navigation';
import { useAuth } from '../hooks/useAuth';
import { routePaths } from '../routes/routePaths';

export default function DashboardLayout({ navItems, sidebarTitle }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { activeRole } = useAuth();

  const roleConfig = activeRole ? roleSidebars[activeRole] : null;
  const items = navItems ?? roleConfig?.nav ?? sidebarNav;
  const title = sidebarTitle ?? roleConfig?.title ?? 'Workspace';

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-30 flex items-center gap-3 border-b border-border bg-surface px-4 py-3 transition-[padding] duration-200 ease-in-out lg:pr-6 lg:pl-[calc(1.5rem_+_var(--sh-sidebar-w,260px))]">
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
        <Link to={routePaths.home} className="flex shrink-0 items-center gap-2 text-base font-bold text-title lg:hidden">
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs text-surface">
            শি
          </span>
          <span className="hidden sm:inline">ShilpoHub</span>
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

      <div className="mx-auto flex max-w-[1600px] items-start gap-6 px-4 py-6 transition-[padding] duration-200 ease-in-out lg:pr-6 lg:pl-[calc(1.5rem_+_var(--sh-sidebar-w,260px))]">
        <div
          id="workspace-sidebar"
          className={`fixed inset-y-0 left-0 z-50 w-[min(86vw,19rem)] overflow-y-auto bg-surface p-4 shadow-xl transition-transform lg:static lg:z-auto lg:block lg:w-auto lg:transform-none lg:overflow-visible lg:bg-transparent lg:p-0 lg:shadow-none ${
            sidebarOpen ? 'translate-x-0' : '-translate-x-full'
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
    </div>
  );
}
