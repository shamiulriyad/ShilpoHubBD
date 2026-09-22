import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

export default function Breadcrumbs({ items = [] }) {
  const { activeRole, isAuthenticated, homePath } = useAuth();
  const workspaceItems = isAuthenticated && activeRole === 'Tourist'
    ? items.map(item => item.path === '/' ? { label: 'Dashboard', path: homePath } : item)
    : items;
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
