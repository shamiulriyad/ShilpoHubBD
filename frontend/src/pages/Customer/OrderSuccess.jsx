import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Button, Badge } from '../../components/ui';
import { orders } from '../../data/mockData';

export default function OrderSuccess() {
  const order = orders[0];

  return (
    <div className="mx-auto max-w-lg py-16 text-center">
      <span className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-success/10 text-3xl text-success">
        ✓
      </span>
      <h1 className="mt-6 text-2xl font-semibold text-heading">Order Placed Successfully</h1>
      <p className="mt-2 text-sm text-body/70">
        Thank you for supporting Bangladesh’s artisans. A confirmation has been sent to your email.
      </p>

      <div className="mt-8 space-y-3 rounded-xl border border-border bg-surface p-5 text-left">
        <div className="flex items-center justify-between">
          <p className="text-sm font-semibold text-heading">{order.id}</p>
          <Badge tone="success">{order.status}</Badge>
        </div>
        <div className="flex justify-between text-sm text-body/70">
          <span>Items</span>
          <span>{order.items}</span>
        </div>
        <div className="flex justify-between text-sm text-body/70">
          <span>Order Date</span>
          <span>{order.date}</span>
        </div>
        <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
          <span>Total Paid</span>
          <span>৳ {order.total.toLocaleString()}</span>
        </div>
      </div>

      <div className="mt-8 flex flex-wrap justify-center gap-3">
        <Link to={routePaths.customer}>
          <Button variant="secondary">View My Orders</Button>
        </Link>
        <Link to={routePaths.customerMarketplace}>
          <Button variant="primary">Continue Shopping</Button>
        </Link>
      </div>
    </div>
  );
}
