import { useEffect, useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useLogoutFlow } from '../../hooks/useLogoutFlow';
import { ConfirmDialog } from '../ui';
import NavigationIcon from './NavigationIcon';
import UserAvatar from '../profile/UserAvatar';
import { presentNavigation } from './workspaceNavigation';

const Chevron = ({ open }) => <svg viewBox="0 0 24 24" className={`sb-chevron ${open ? 'is-open' : ''}`} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>;
const SignOutIcon = () => <svg viewBox="0 0 24 24" className="h-[18px] w-[18px]" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4m7 14 5-5-5-5m5 5H9"/></svg>;

/**
 * Two-state workspace sidebar (icon rail / expanded panel).
 * - Destinations come from `items` (the role navigation); nothing is invented here.
 * - Every remaining section is a row (like Quick access) that expands to its nested sub-list.
 *   Pass `sections` + `selectedSection` (open section) + `onSectionChange` to control it; otherwise it is derived from `items`.
 * - An item may carry `badge` (number); it renders as a count when expanded and a dot in the rail.
 */
export default function Sidebar({ items = [], title = 'Menu', className = '', onNavigate, compact = false, onExpand, onToggle, presentationRole, sections, selectedSection, onSectionChange }) {
  const { activeRole, user } = useAuth();
  const { pathname } = useLocation();
  const logout = useLogoutFlow();
  const { primary, secondary } = presentNavigation(items, presentationRole || activeRole);
  const isPinned = group => /^(general|account|settings|help)$/i.test(group.section);
  const pinned = (sections ?? secondary).filter(isPinned).flatMap(group => group.items);
  const options = (sections ?? secondary).filter(group => !isPinned(group));
  const activeGroup = options.find(group => group.items.some(item => item.path === pathname))?.section;
  const [internalOpen, setInternalOpen] = useState(activeGroup || null);
  const open = selectedSection !== undefined ? selectedSection : internalOpen;
  const toggle = section => { const next = open === section ? null : section; setInternalOpen(next); onSectionChange?.(next); };
  useEffect(() => { if (activeGroup && selectedSection === undefined) setInternalOpen(activeGroup); }, [activeGroup, selectedSection]);

  const badge = item => item.badge ? (compact ? <span className="sb-dot" aria-hidden="true"/> : <span className="sb-badge">{item.badge}</span>) : null;
  const itemLink = (item, nested = false) => <NavLink key={item.path + item.label} to={item.path} end onClick={onNavigate} title={item.label} aria-label={compact ? item.label : undefined} className={({ isActive }) => `sb-link ${nested ? 'is-nested' : ''} ${isActive ? 'is-active' : ''}`}>
    <NavigationIcon label={item.label}/>{!compact && <span className="sb-label">{item.label}</span>}{badge(item)}
  </NavLink>;

  return <aside className={`workspace-navigation ${compact ? 'is-compact' : ''} ${className}`}>
    {!compact && <div className="sb-header">
      <UserAvatar className="sb-avatar" name={user?.name} />
      <div className="min-w-0 flex-1"><p className="sb-name">{user?.name || `${title} workspace`}</p><p className="sb-status">{user?.roleLabel || title} · Signed in</p></div>
      <button type="button" className="sb-round" aria-label="Sign out" title="Sign out" onClick={logout.request}><SignOutIcon/></button>
    </div>}
    <nav aria-label={`${title} navigation`} className="sb-nav">
      <div className="sb-group">
        {!compact && <p className="sb-group-label">Quick access</p>}
        {primary.map(item => itemLink(item))}
      </div>
      {!!options.length && <div className="sb-group">
        {compact ? <hr className="sb-divider"/> : <p className="sb-group-label">Browse by section</p>}
        {options.map(group => {
          const isOpen = open === group.section && !compact;
          const hasActive = group.items.some(item => item.path === pathname);
          return <div key={group.section}>
            <button type="button" className={`sb-link sb-section ${hasActive ? 'has-active' : ''} ${isOpen ? 'is-open' : ''}`} aria-expanded={isOpen} aria-label={compact ? group.section : undefined} title={compact ? group.section : undefined}
              onClick={() => { if (compact) { onExpand?.(); setInternalOpen(group.section); } else toggle(group.section); }}>
              <NavigationIcon label={group.section}/>{!compact && <><span className="sb-label">{group.section}</span><span className="sb-menu-count">{group.items.length}</span><Chevron open={isOpen}/></>}
            </button>
            {isOpen && <div className="sb-subnav">{group.items.map(item => itemLink(item, true))}</div>}
          </div>;
        })}
      </div>}
    </nav>
    {(onToggle || !!pinned.length) && <div className="sb-bottom">
      {compact && <hr className="sb-divider"/>}
      {pinned.map(item => itemLink(item))}
      {onToggle && <>      <button type="button" className="sb-link sb-collapse" onClick={onToggle} aria-expanded={!compact} aria-label={compact ? 'Expand sidebar' : 'Collapse sidebar'} title={compact ? 'Expand sidebar' : undefined}>
        <NavigationIcon label="Menu"/>{!compact && <span className="sb-label">Collapse navigation</span>}
      </button></>}
    </div>}
    <ConfirmDialog open={logout.confirming} title="Sign out?" message="You will be signed out of your ShilpoHub workspace." confirmLabel="Sign out" cancelLabel="Stay" busy={logout.busy} onConfirm={logout.confirm} onCancel={logout.cancel}/>
  </aside>;
}
