import { useEffect, useRef, useState } from 'react';
import { Link, Outlet } from 'react-router-dom';
import Footer from '../components/layout/Footer';
import Sidebar from '../components/layout/Sidebar';
import ProfileDropdown from '../components/layout/ProfileDropdown';
import NotificationBell from '../components/notifications/NotificationBell';
import { sidebarNav, roleSidebars } from '../data/navigation';
import { routePaths } from '../routes/routePaths';
import { useAuth } from '../hooks/useAuth';
import BrandLogo from '../components/brand/BrandLogo';
import GlobalSearch from '../components/layout/GlobalSearch';
import LanguageMenu from '../components/layout/LanguageMenu';
import HelplineChip from '../components/layout/HelplineChip';
import ProfileStatusBanner from '../components/profile/ProfileStatusBanner';
import NavigationIcon from '../components/layout/NavigationIcon';
import { AIAssistantWidget, ConfirmDialog } from '../components/ui';
import { useBackLogoutGuard } from '../hooks/useBackLogoutGuard';
import { useLogoutFlow } from '../hooks/useLogoutFlow';

export default function DashboardLayout({ navItems, sidebarTitle }) {
  const sidebarRef = useRef(null);
  const menuTrigger = useRef(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [compact, setCompact] = useState(()=>{try{return localStorage.getItem('sh:sidebar:compact')==='true';}catch{return false;}});
  const toggleCompact = ()=>setCompact(current=>{const next=!current;try{localStorage.setItem('sh:sidebar:compact',String(next));}catch{/* Visual preference is optional. */}return next;});
  useEffect(() => { const close = event => { if (event.key === 'Escape') setSidebarOpen(false); }; window.addEventListener('keydown',close); return () => window.removeEventListener('keydown',close); }, []);
  useEffect(()=>{
    if(!sidebarOpen)return;
    const priorOverflow=document.body.style.overflow;
    document.body.style.overflow='hidden';
    const focusables=()=>Array.from(sidebarRef.current?.querySelectorAll('a[href],button:not(:disabled)') || []).filter(element=>element.getClientRects().length);
    focusables()[0]?.focus();
    const trap=event=>{if(event.key!=='Tab')return;const targets=focusables(),first=targets[0],last=targets[targets.length-1];if(event.shiftKey&&document.activeElement===first){event.preventDefault();last?.focus();}else if(!event.shiftKey&&document.activeElement===last){event.preventDefault();first?.focus();}};
    document.addEventListener('keydown',trap);
    return ()=>{document.body.style.overflow=priorOverflow;document.removeEventListener('keydown',trap);menuTrigger.current?.focus();};
  },[sidebarOpen]);
  const { activeRole } = useAuth();
  const backGuard = useBackLogoutGuard();
  const backLogout = useLogoutFlow();

  const roleConfig = activeRole ? roleSidebars[activeRole] : null;
  const items = navItems ?? roleConfig?.nav ?? sidebarNav;
  const title = sidebarTitle ?? roleConfig?.title ?? 'Workspace';
  // Quick-access picks follow the workspace being shown (e.g. a SuperAdmin inside /government), not the login role.
  const presentationRole = Object.entries(roleSidebars).find(([, config]) => config.nav === items)?.[0] ?? activeRole;

  return (
    <div className={`dashboard-shell ${compact ? 'sidebar-compact' : ''}`} data-workspace={activeRole}>
      <a href="#main-content" className="skip-link">Skip to content</a>
      <header className="workspace-topbar">
        <button
          type="button"
          className="rounded-md border border-border p-2 text-body lg:hidden"
          ref={menuTrigger}
          onClick={() => setSidebarOpen(true)}
          aria-label="Open workspace navigation"
          aria-expanded={sidebarOpen}
          aria-controls="workspace-sidebar"
        >
          <NavigationIcon label="Menu"/>
        </button>
        <Link to={activeRole === 'Tourist' ? routePaths.tourist : activeRole === 'HeritageInnovationHub' ? routePaths.researcher : routePaths.home} className="flex shrink-0 items-center gap-2 text-base font-bold text-title">
          <BrandLogo size="sm" className="[&>span:last-child]:hidden sm:[&>span:last-child]:inline" />
        </Link>
        <GlobalSearch navItems={items}/>
        <div className="topbar-actions">
          <HelplineChip />
          <NotificationBell />
          <LanguageMenu/>
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

      <div className="workspace-body">
        <div
          id="workspace-sidebar"
          ref={sidebarRef}
          className={`workspace-sidebar ${sidebarOpen ? 'mobile-open' : ''}`}
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
          <Sidebar items={items} title={title} presentationRole={presentationRole} compact={compact && !sidebarOpen} onExpand={()=>setCompact(false)} onToggle={toggleCompact} onNavigate={() => setSidebarOpen(false)} />
        </div>

        <main id="main-content" className="workspace-content">
          <ProfileStatusBanner />
          <Outlet />
        </main>
      </div>
      <Footer />
      <AIAssistantWidget />
      <ConfirmDialog
        open={backGuard.state === 'blocked'}
        title="Leave and log out?"
        message="Going back will end your session and take you to the home page. Stay here to keep working."
        confirmLabel="Log out and leave"
        cancelLabel="Stay here"
        busy={backLogout.busy}
        onConfirm={() => { backGuard.reset?.(); backLogout.confirm(); }}
        onCancel={() => backGuard.reset?.()}
      />
    </div>
  );
}
