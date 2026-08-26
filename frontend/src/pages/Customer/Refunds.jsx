import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useOrders } from '../../hooks/useOrders';

export default function Refunds() {
  const { data, isLoading, isError, error } = useOrders({ status: 'Refunded', pageSize: 50 });
  const orders = data?.items || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Refunds' }]}
        title="Refunds"
        description="Orders that have been refunded. Open an order for the exact refund amount and reason."
      />

      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {orders.map((order) => (
            <Link
              key={order.id}
              to={routePaths.customerOrderDetails.replace(':orderId', order.id)}
              className="flex flex-wrap items-center justify-between gap-3 p-4 transition hover:bg-background/40"
            >
              <div>
                <p className="text-sm font-medium text-heading">{order.orderNumber}</p>
                <p className="text-xs text-body/60">{new Date(order.createdAt).toLocaleDateString()}</p>
              </div>
              <div className="flex items-center gap-4">
                <p className="text-sm font-semibold text-primary">৳ {order.total.toLocaleString()}</p>
                <Badge tone="success">{order.status}</Badge>
              </div>
            </Link>
          ))}
          {orders.length === 0 && <p className="p-6 text-center text-sm text-body/60">No refunds to show.</p>}
        </AsyncState>
      </div>
    </div>
  );
}
