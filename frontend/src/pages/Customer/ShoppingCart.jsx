import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { products } from '../../data/mockData';

export default function ShoppingCart() {
  const items = products.slice(0, 3).map((p, i) => ({ ...p, qty: i + 1 }));
  const subtotal = items.reduce((sum, item) => sum + item.price * item.qty, 0);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'Cart' },
        ]}
        title="Shopping Cart"
      />

      <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {items.map((item) => (
            <div key={item.id} className="flex items-center gap-4 p-4">
              <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-lg bg-background text-[10px] text-body/40">
                Image
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-heading">{item.name}</p>
                <p className="text-xs text-body/60">{item.producer}</p>
              </div>
              <div className="flex items-center gap-2 text-sm">
                <button type="button" className="h-7 w-7 rounded-md border border-border">
                  −
                </button>
                <span className="w-6 text-center">{item.qty}</span>
                <button type="button" className="h-7 w-7 rounded-md border border-border">
                  +
                </button>
              </div>
              <p className="w-24 shrink-0 text-right text-sm font-semibold text-primary">
                ৳ {(item.price * item.qty).toLocaleString()}
              </p>
            </div>
          ))}
        </div>

        <div className="h-fit space-y-4 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">Order Summary</p>
          <div className="flex justify-between text-sm text-body/70">
            <span>Subtotal</span>
            <span>৳ {subtotal.toLocaleString()}</span>
          </div>
          <div className="flex justify-between text-sm text-body/70">
            <span>Shipping</span>
            <span>৳ 120</span>
          </div>
          <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
            <span>Total</span>
            <span>৳ {(subtotal + 120).toLocaleString()}</span>
          </div>
          <Link to={routePaths.customerCheckout}>
            <Button variant="primary" className="w-full">
              Proceed to Checkout
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
