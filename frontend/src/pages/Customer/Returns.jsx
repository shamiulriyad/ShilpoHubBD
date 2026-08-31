import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useOrders } from '../../hooks/useOrders';

const statusTone = { ReturnRequested: 'secondary', Returned: 'success' };

export default function Returns() {
  const requestedQuery = useOrders({ status: 'ReturnRequested', pageSize: 50 });
  const returnedQuery = useOrders({ status: 'Returned', pageSize: 50 });

  const isLoading = requestedQuery.isLoading || returnedQuery.isLoading;
  const isError = requestedQuery.isError || returnedQuery.isError;
  const orders = [...(requestedQuery.data?.items || []), ...(returnedQuery.data?.items || [])];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Returns' }]}
        title="Returns"
        description="Return requests are started from an order's details page — see Order History for delivered orders."
      />

      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        <AsyncState isLoading={isLoading} isError={isError} error={requestedQuery.error || returnedQuery.error}>
          {orders.map((order) => (
            <Link
              key={order.id}
              to={routePaths.customerOrderDetails.replace(':orderId', order.id)}
              className="flex flex-wrap items-center justify-between gap-3 p-4 transition hover:bg-background/40"
            >
              <div>
                <p className="text-sm font-medium text-heading">{order.orderNumber}</p>
                <p className="text-xs text-body/60">
                  {order.itemCount} item{order.itemCount > 1 ? 's' : ''} · ৳ {order.total.toLocaleString()}
                </p>
              </div>
              <Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>
            </Link>
          ))}
          {orders.length === 0 && (
            <p className="p-6 text-center text-sm text-body/60">You have no return requests.</p>
          )}
        </AsyncState>
      </div>
    </div>
  );
}
