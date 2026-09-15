import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from '../components/layout/Sidebar';
import NotificationPanel from '../components/layout/NotificationPanel';
import ProfileDropdown from '../components/layout/ProfileDropdown';
import SearchBar from '../components/ui/SearchBar';
import { sidebarNav, roleSidebars } from '../data/navigation';
import { useAuth } from '../hooks/useAuth';

export default function DashboardLayout({ navItems, sidebarTitle }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);
  const { activeRole } = useAuth();

  // A route may pin a specific sidebar (role landing areas do). Otherwise the
  // sidebar is driven by the signed-in user's role, so the shared /dashboard/*
  // shell shows each role its own navigation instead of one generic menu.
  const roleConfig = activeRole ? roleSidebars[activeRole] : null;
  const items = navItems ?? roleConfig?.nav ?? sidebarNav;
  const title = sidebarTitle ?? roleConfig?.title ?? 'Workspace';

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-30 flex items-center gap-3 border-b border-border bg-surface px-4 py-3 transition-[padding] duration-200 ease-in-out lg:pr-6 lg:pl-[calc(1.5rem+var(--sh-sidebar-w,260px))]">
        <button
          type="button"
          className="rounded-md border border-border p-2 text-body lg:hidden"
          onClick={() => setSidebarOpen((open) => !open)}
          aria-label="Toggle sidebar"
        >
          ☰
        </button>
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

      <div className="mx-auto flex max-w-[1600px] items-start gap-6 px-4 py-6 transition-[padding] duration-200 ease-in-out lg:pr-6 lg:pl-[calc(1.5rem+var(--sh-sidebar-w,260px))]">
        <div className={`${sidebarOpen ? 'block' : 'hidden'} w-full lg:block lg:w-auto`}>
          <Sidebar items={items} title={title} />
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
