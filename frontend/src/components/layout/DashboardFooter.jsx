import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { useSiteContent } from '../../hooks/useSiteContent';
import { routePaths } from '../../routes/routePaths';
import NavigationIcon from './NavigationIcon';

const topics = { about: 'About ShilpoHub', privacy: 'Privacy Policy', terms: 'Terms of Service' };

export default function DashboardFooter({ role }) {
  const { data, isLoading, isError } = useSiteContent();
  const [topic, setTopic] = useState(null);
  const dialog = useRef(null);
  const opener = useRef(null);
  const email = import.meta.env.VITE_SUPPORT_EMAIL?.trim() || 'helpCenter@shilpohubbd.com';
  useEffect(() => {
    if (topic) dialog.current?.showModal();
    else if (dialog.current?.open) { dialog.current.close(); opener.current?.focus(); }
  }, [topic]);
  const records = (Array.isArray(data) ? data : []).filter(item =>
    (item.group === topic || item.group?.startsWith(`${topic}-`)) && (item.body || item.description || item.content));
  return <>
    <footer className="workspace-footer">
      <span className="workspace-copyright">© {new Date().getFullYear()} ShilpoHub</span>
      <nav aria-label="Dashboard information">
        {Object.entries(topics).map(([key, title]) => <button key={key} type="button" onClick={event => { opener.current = event.currentTarget; setTopic(key); }}>{title}</button>)}
      </nav>
      <div className="workspace-footer-contact">
        {role !== 'SuperAdmin' && <a href="tel:98675" aria-label="Call admin helpline 98675" title="Admin helpline"><svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth="1.7" aria-hidden="true"><path d="M4 3h4l2 5-3 2a15 15 0 0 0 7 7l2-3 5 2v4c-9 3-20-8-17-17Z"/></svg><span>98675</span></a>}
        {email ? <a href={`mailto:${email}`} aria-label={`Email support at ${email}`} title={email}><NavigationIcon label="Messages"/></a> : <Link to={routePaths.dashboardMessages} aria-label="Open messages" title="Messages"><NavigationIcon label="Messages"/></Link>}
      </div>
    </footer>
    <dialog ref={dialog} className="workspace-info-dialog" aria-labelledby="workspace-info-title" onCancel={() => setTopic(null)} onClick={event => { if (event.target === dialog.current) setTopic(null); }}>
      <div className="workspace-info-heading"><h2 id="workspace-info-title">{topics[topic]}</h2><button type="button" autoFocus onClick={() => setTopic(null)} aria-label="Close details">×</button></div>
      <div className="workspace-info-body">
        {isLoading ? <p>Loading details…</p> : isError ? <p>Details could not be loaded. Please try again later.</p> : records.length ? records.map(item => <section key={item.id}><h3>{item.title}</h3><p>{item.body || item.description || item.content}</p></section>) : topic === 'about' ? <p>ShilpoHub connects artisans, producers, tourists and partners across Bangladesh to discover, support and preserve living heritage.</p> : <p>This information has not been published yet. Please contact the administrator for details.</p>}
      </div>
    </dialog>
  </>;
}
