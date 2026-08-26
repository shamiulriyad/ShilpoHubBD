import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, CartItem, AsyncState } from '../../components/ui';
import { useCart, useCartSummary, useCartMutations } from '../../hooks/useCart';

export default function ShoppingCart() {
  const cartQuery = useCart();
  const summaryQuery = useCartSummary();
  const { updateQuantity, remove } = useCartMutations();

  const items = (cartQuery.data || []).map((item) => ({
    id: item.id,
    name: item.productName,
    producer: item.variantName || '',
    price: item.unitPrice,
    qty: item.quantity,
  }));
  const subtotal = summaryQuery.data?.subtotal ?? items.reduce((sum, item) => sum + item.price * item.qty, 0);

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
          <AsyncState isLoading={cartQuery.isLoading} isError={cartQuery.isError} error={cartQuery.error}>
            {items.map((item) => (
              <CartItem
                key={item.id}
                item={item}
                onIncrement={() => updateQuantity.mutate({ itemId: item.id, quantity: item.qty + 1 })}
                onDecrement={() =>
                  item.qty > 1 && updateQuantity.mutate({ itemId: item.id, quantity: item.qty - 1 })
                }
                onRemove={() => remove.mutate(item.id)}
              />
            ))}
            {items.length === 0 && <p className="p-6 text-center text-sm text-body/60">Your cart is empty.</p>}
          </AsyncState>
        </div>

        <div className="h-fit space-y-4 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">Order Summary</p>
          <div className="flex justify-between text-sm text-body/70">
            <span>Subtotal</span>
            <span>৳ {subtotal.toLocaleString()}</span>
          </div>
          <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
            <span>Total</span>
            <span>৳ {subtotal.toLocaleString()}</span>
          </div>
          <Link to={routePaths.customerCheckout}>
            <Button variant="primary" className="w-full" disabled={items.length === 0}>
              Proceed to Checkout
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
