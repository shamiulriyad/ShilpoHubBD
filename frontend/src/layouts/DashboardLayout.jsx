import { useState } from 'react';
import { Link, Outlet } from 'react-router-dom';
import Sidebar from '../components/layout/Sidebar';
import NotificationPanel from '../components/layout/NotificationPanel';
import ProfileDropdown from '../components/layout/ProfileDropdown';
import SearchBar from '../components/ui/SearchBar';
import { sidebarNav } from '../data/navigation';
import { routePaths } from '../routes/routePaths';

export default function DashboardLayout({ navItems = sidebarNav, sidebarTitle = 'Workspace' }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-30 flex items-center gap-3 border-b border-border bg-surface px-4 py-3 lg:px-6">
        <button
          type="button"
          className="rounded-md border border-border p-2 text-body lg:hidden"
          onClick={() => setSidebarOpen((open) => !open)}
          aria-label="Toggle sidebar"
        >
          ☰
        </button>
        <Link to={routePaths.home} className="hidden shrink-0 items-center gap-2 text-base font-bold text-title lg:flex">
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs text-surface">
            SH
          </span>
          ShilpoHub
        </Link>
        <div className="mx-auto hidden max-w-md flex-1 lg:block">
          <SearchBar placeholder="Search the dashboard…" />
        </div>
        <button
          type="button"
          onClick={() => setNotifOpen((open) => !open)}
          className="ml-auto rounded-md border border-border p-2 text-body lg:hidden"
          aria-label="Toggle notifications"
        >
          🔔
        </button>
        <ProfileDropdown />
      </header>

      <div className="mx-auto flex max-w-[1600px] items-start gap-6 px-4 py-6 lg:px-6">
        <div className={`${sidebarOpen ? 'block' : 'hidden'} w-full lg:block lg:w-auto`}>
          <Sidebar items={navItems} title={sidebarTitle} />
        </div>
        <main className="min-w-0 flex-1">
          <Outlet />
        </main>
        <div className={`${notifOpen ? 'block' : 'hidden'} w-full lg:block lg:w-auto`}>
          <NotificationPanel />
        </div>
      </div>
    </div>
  );
}
