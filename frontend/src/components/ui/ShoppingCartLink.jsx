import { Link } from 'react-router-dom';
import { useCart } from '../../hooks/useCart';
import { useAuth } from '../../hooks/useAuth';
import { routePaths } from '../../routes/routePaths';

export default function ShoppingCartLink() {
  const { isAuthenticated } = useAuth();
  const cart = useCart(isAuthenticated);
  const count = (cart.data || []).reduce((sum, item) => sum + item.quantity, 0);
  return <Link to={routePaths.customerCart} className="inline-flex min-h-11 items-center gap-2 rounded-full border border-border bg-surface px-4 py-2 font-medium text-heading shadow-sm hover:border-primary focus-visible:outline focus-visible:outline-2 focus-visible:outline-primary" aria-label={`Open shopping cart${cart.isSuccess ? `, ${count} items` : ''}`}>
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><path d="M3 3h2l3 12h11l2-9H6"/><circle cx="9" cy="20" r="1"/><circle cx="18" cy="20" r="1"/></svg>
    Cart <span className="rounded-full bg-primary-soft px-2 text-primary" aria-live="polite">{cart.isSuccess ? count : '—'}</span>
  </Link>;
}
