import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

export default function Breadcrumbs({ items = [] }) {
  const { activeRole, isAuthenticated, homePath } = useAuth();
  const { pathname } = useLocation();
  const innovationWorkspace = isAuthenticated && activeRole === 'HeritageInnovationHub' && (pathname.startsWith('/research/') || pathname.startsWith('/explore'));
  const workspaceItems = isAuthenticated && activeRole === 'Tourist'
    ? items.map(item => item.path === '/' ? { label: 'Dashboard', path: homePath } : item)
    : innovationWorkspace ? items.map(item => item.path === '/' || item.path === '/research' ? { label: item.path === '/' ? 'Workspace' : item.label, path: '/researcher' } : item) : items;
  return (
    <nav aria-label="Breadcrumb" className="mb-4 text-sm text-body/70">
      <ol className="flex flex-wrap items-center gap-1.5">
        {workspaceItems.map((item, index) => {
          const isLast = index === workspaceItems.length - 1;
          return (
            <li key={`${item.label}-${index}`} className="flex items-center gap-1.5">
              {index > 0 && <span className="text-border">/</span>}
              {item.path && !isLast ? (
                <Link to={item.path} className="text-link hover:underline">
                  {item.label}
                </Link>
              ) : (
                <span className={isLast ? 'font-medium text-heading' : ''}>{item.label}</span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
