import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from '../components/layout/Sidebar';
import ProfileDropdown from '../components/layout/ProfileDropdown';
import Footer from '../components/layout/Footer';
import { sidebarNav, roleSidebars } from '../data/navigation';
import { useAuth } from '../hooks/useAuth';

export default function DashboardLayout({ navItems, sidebarTitle }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { activeRole } = useAuth();

  const roleConfig = activeRole ? roleSidebars[activeRole] : null;
  const items = navItems ?? roleConfig?.nav ?? sidebarNav;
  const title = sidebarTitle ?? roleConfig?.title ?? 'Workspace';

  return (
<<<<<<< HEAD
    <div className="flex min-h-screen flex-col bg-background">
      <header className="sticky top-0 z-40 flex h-[4.5rem] shrink-0 items-center gap-3 border-b border-border bg-surface px-4 py-3 lg:px-6">
=======
    <div className="min-h-screen bg-background">
<<<<<<< HEAD
      <header className="sticky top-0 z-40 flex items-center gap-3 border-b border-border bg-surface px-4 py-3 lg:px-6">
=======
      <header className="sticky top-0 z-30 flex items-center gap-3 border-b border-border bg-surface px-4 py-3 transition-[padding] duration-200 ease-in-out lg:pr-6 lg:pl-[calc(1.5rem+var(--sh-sidebar-w,260px))]">
>>>>>>> Riyad
>>>>>>> main
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
<<<<<<< HEAD
        <Link to={routePaths.home} className="flex shrink-0 items-center gap-2 text-base font-bold text-title">
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs text-surface">
            শি
          </span>
          <span className="hidden sm:inline">ShilpoHub</span>
        </Link>
        <div className="ml-auto">
          <ProfileDropdown />
=======
        <div className="mx-auto hidden max-w-md flex-1 lg:block">
          <SearchBar placeholder="Search the dashboard…" />
>>>>>>> Riyad
        </div>
      </header>

<<<<<<< HEAD
      {sidebarOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-title/35 lg:hidden"
          onClick={() => setSidebarOpen(false)}
          aria-label="Close workspace navigation"
        />
      )}

      <div className="mx-auto flex w-full max-w-[1600px] flex-1 items-start gap-6 px-4 py-6 lg:px-6">
        <div
          id="workspace-sidebar"
          onKeyDown={(event) => { if (event.key === 'Escape') setSidebarOpen(false); }}
          className={`workspace-scroll fixed inset-y-0 left-0 z-50 w-[min(86vw,19rem)] overflow-y-auto overscroll-contain bg-surface p-4 shadow-xl transition-transform lg:sticky lg:top-[6rem] lg:z-30 lg:block lg:max-h-[calc(100dvh-7.5rem)] lg:w-64 lg:shrink-0 lg:translate-x-0 lg:bg-transparent lg:p-0 lg:shadow-none ${
            sidebarOpen ? 'visible translate-x-0' : 'invisible -translate-x-full lg:visible'
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
=======
      <div className="mx-auto flex max-w-[1600px] items-start gap-6 px-4 py-6 transition-[padding] duration-200 ease-in-out lg:pr-6 lg:pl-[calc(1.5rem+var(--sh-sidebar-w,260px))]">
        <div className={`${sidebarOpen ? 'block' : 'hidden'} w-full lg:block lg:w-auto`}>
          <Sidebar items={items} title={title} />
>>>>>>> Riyad
        </div>

        <main className="min-w-0 flex-1">
          <Outlet />
        </main>
      </div>
      <Footer />
    </div>
  );
}
