import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, CartItem } from '../../components/ui';
import { products } from '../../data/mockData';

export default function ShoppingCart() {
  const [items, setItems] = useState(products.slice(0, 3).map((p, i) => ({ ...p, qty: i + 1 })));
  const subtotal = items.reduce((sum, item) => sum + item.price * item.qty, 0);

  function updateQty(id, delta) {
    setItems((prev) =>
      prev.map((item) => (item.id === id ? { ...item, qty: Math.max(1, item.qty + delta) } : item)),
    );
  }

  function removeItem(id) {
    setItems((prev) => prev.filter((item) => item.id !== id));
  }

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
            <CartItem
              key={item.id}
              item={item}
              onIncrement={() => updateQty(item.id, 1)}
              onDecrement={() => updateQty(item.id, -1)}
              onRemove={() => removeItem(item.id)}
            />
          ))}
          {items.length === 0 && <p className="p-6 text-center text-sm text-body/60">Your cart is empty.</p>}
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
