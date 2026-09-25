import { useEffect, useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import NavigationIcon from './NavigationIcon';
import { presentNavigation } from './workspaceNavigation';

export default function Sidebar({ items = [], title = 'Menu', className = '', onNavigate, compact = false, onExpand, presentationRole }) {
  const { activeRole } = useAuth();
  const { pathname } = useLocation();
  const { primary, secondary } = presentNavigation(items, presentationRole || activeRole);
  const activeGroup = secondary.find(group => group.items.some(item => item.path === pathname))?.section;
  const [more, setMore] = useState(Boolean(activeGroup));
  const [groupOpen, setGroupOpen] = useState(activeGroup || null);
  useEffect(() => { if (activeGroup) { setMore(true); setGroupOpen(activeGroup); } }, [activeGroup, pathname]);
  const itemLink = item => <NavLink key={item.path + item.label} to={item.path} end onClick={onNavigate} title={item.label} aria-label={compact ? item.label : undefined} className={({isActive})=>`workspace-link ${isActive ? 'is-active' : ''}`}>
    <NavigationIcon label={item.label}/><span className={compact ? 'sr-only' : 'workspace-link-label'}>{item.label}</span>
  </NavLink>;
  return <aside className={`workspace-navigation ${compact ? 'is-compact' : ''} ${className}`}>
    <div className="workspace-caption">{compact ? <span aria-hidden="true">—</span> : <><span className="workspace-dot"/>{title} workspace</>}</div>
    <nav aria-label={`${title} navigation`}>
      <div className="space-y-1">{primary.map(itemLink)}</div>
      {!!secondary.length && <div className="navigation-more">
        <button type="button" className={`workspace-link w-full ${more ? 'more-active' : ''}`} aria-label="More navigation" aria-expanded={more && !compact} onClick={()=>{ if(compact) { onExpand?.(); setMore(true); } else setMore(value=>!value); }} title={compact ? 'More navigation' : undefined}>
          <NavigationIcon label="More"/><span className={compact ? 'sr-only' : 'flex-1 text-left'}>More</span>{!compact && <span aria-hidden="true">{more ? '−' : '+'}</span>}
        </button>
        {more && !compact && <div className="mt-3 space-y-2">{secondary.map(group=><div key={group.section}>
          <button type="button" className="workspace-group" aria-expanded={groupOpen===group.section} onClick={()=>setGroupOpen(current=>current===group.section ? null : group.section)}><span>{group.section}</span><span aria-hidden="true">{groupOpen===group.section ? '−' : '+'}</span></button>
          {groupOpen===group.section && <div className="workspace-subnav">{group.items.map(itemLink)}</div>}
        </div>)}</div>}
      </div>}
    </nav>
    {!compact && <div className="workspace-note"><span className="workspace-note-rule"/><p>Rooted in heritage.<br/>Made for what’s next.</p><span>ShilpoHub · Bangladesh</span></div>}
  </aside>;
}
