import { useEffect, useRef, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { useNotifications } from '../../hooks/useNotifications';
import NotificationCenter, { BellIcon } from './NotificationCenter';
export default function NotificationBell() {
  const [open, setOpen] = useState(false);
  const container = useRef(null);
  const button = useRef(null);
  const location = useLocation();
  const { data, isError } = useNotifications();
  const count = data?.unreadCount || 0;
  useEffect(() => setOpen(false), [location.pathname]);
  useEffect(() => {
    if (!open) return;
    const outside = e => { if (!container.current?.contains(e.target)) setOpen(false); };
    document.addEventListener('pointerdown', outside);
    return () => document.removeEventListener('pointerdown', outside);
  }, [open]);
  return <div ref={container} className="relative" onBlur={e => { if (!e.currentTarget.contains(e.relatedTarget)) setOpen(false); }} onKeyDown={e => { if (e.key === 'Escape') { setOpen(false); button.current?.focus(); } }}>
    <button ref={button} type="button" aria-label={isError ? 'Notifications unavailable. Open to retry.' : `Notifications${count ? `, ${count} unread` : ''}`} aria-expanded={open} aria-controls={open ? 'notification-popover' : undefined} onClick={() => setOpen(value => !value)} className="relative flex h-11 w-11 items-center justify-center rounded-full border border-border text-heading transition hover:bg-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary">
      <BellIcon />{count > 0 && <span aria-hidden="true" className="absolute -right-1 -top-1 min-w-5 rounded-full bg-primary px-1 text-center text-[10px] font-bold leading-5 text-white">{count > 99 ? '99+' : count}</span>}
    </button>
    {open && <div id="notification-popover" className="fixed left-3 right-3 top-20 z-50 max-h-[calc(100dvh-6rem)] overflow-y-auto rounded-2xl shadow-xl sm:absolute sm:left-auto sm:right-0 sm:top-full sm:mt-3 sm:w-[26rem]"><NotificationCenter compact onNavigate={() => setOpen(false)} /></div>}
  </div>;
}
