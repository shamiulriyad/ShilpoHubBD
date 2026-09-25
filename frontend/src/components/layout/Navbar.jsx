import { useEffect, useState } from 'react';
import { Link, NavLink, useLocation } from 'react-router-dom';
import { mainNav, megaMenus } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';
import { useAuth } from '../../hooks/useAuth';
import ProfileDropdown from './ProfileDropdown';
import NotificationBell from '../notifications/NotificationBell';
import BrandLogo from '../brand/BrandLogo';
import GlobalSearch from './GlobalSearch';
import LanguageMenu from './LanguageMenu';
import NavigationIcon from './NavigationIcon';

export default function Navbar() {
  const { isAuthenticated } = useAuth();
  const { pathname } = useLocation();
  const [menu,setMenu]=useState(false);
  const [section,setSection]=useState(null);
  const close=()=>{setMenu(false);setSection(null);};
  useEffect(()=>{setMenu(false);setSection(null);},[pathname]);
  return <header className="public-header" onKeyDown={event=>{if(event.key==='Escape')close();}}>
    <div className="public-topbar"><Link to={routePaths.home} aria-label="ShilpoHub home"><BrandLogo/></Link><GlobalSearch/><div className="topbar-actions">{isAuthenticated&&<NotificationBell/>}<LanguageMenu/>{isAuthenticated ? <ProfileDropdown/> : <><Link className="header-login" to={routePaths.login}>Login</Link><Link className="header-register" to={routePaths.register}>Register</Link></>}</div></div>
    <nav className="public-nav" aria-label="Main navigation"><button type="button" className="public-menu-toggle" onClick={()=>{setMenu(value=>!value);setSection(null);}} aria-expanded={menu}><NavigationIcon label="Menu"/>Explore ShilpoHub</button><div className={`public-nav-links ${menu ? 'mobile-open' : ''}`}>{mainNav.map(item=><div key={item.label} className="public-nav-item"><NavLink to={item.path} end={item.path==='/'} onClick={close} className={({isActive})=>isActive?'is-active':''}>{item.label}</NavLink>{item.menu&&<button type="button" aria-label={`Open ${item.label} menu`} aria-expanded={section===item.menu} onClick={()=>setSection(current=>current===item.menu?null:item.menu)}>⌄</button>}</div>)}</div><span className="public-nav-note">Preserving heritage. Empowering people.</span></nav>
    {section && <div className="public-mega-menu"><div><p className="menu-eyebrow">{megaMenus[section].heading}</p><p className="mt-3 max-w-xs text-sm leading-6 text-muted">{megaMenus[section].description}</p></div><div className="public-mega-links">{megaMenus[section].links.map(item=><Link key={item.path} to={item.path} onClick={close} aria-current={pathname===item.path?'page':undefined}><span className="font-medium">{item.label} <span aria-hidden="true">↗</span></span><span className="mt-1 block text-xs leading-5 text-muted">{item.description}</span></Link>)}</div><button type="button" className="mega-close" onClick={close} aria-label="Close navigation menu">×</button></div>}
  </header>;
}
