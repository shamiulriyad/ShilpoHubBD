import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState, Pagination } from '../../components/ui';
import { useOrders } from '../../hooks/useOrders';

const filters = ['All', 'Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];
const statusTone = {
  Delivered: 'success',
  Shipped: 'primary',
  Processing: 'secondary',
  Pending: 'secondary',
  Cancelled: 'neutral',
  ReturnRequested: 'secondary',
  Returned: 'neutral',
  Refunded: 'neutral',
};

export default function OrderHistory() {
  const [filter, setFilter] = useState('All');
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useOrders({
    status: filter === 'All' ? undefined : filter,
    page,
    pageSize: 10,
  });
  const orders = data?.items || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Order History' }]}
        title="Order History"
        description={data ? `${data.totalCount} orders placed on ShilpoHub.` : undefined}
      />

      <div className="mb-6 flex flex-wrap gap-2">
        {filters.map((item) => (
          <button
            key={item}
            type="button"
            onClick={() => {
              setFilter(item);
              setPage(1);
            }}
            className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              filter === item ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-body hover:bg-background'
            }`}
          >
            {item}
          </button>
        ))}
      </div>

      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {orders.map((order) => (
            <Link
              key={order.id}
              to={routePaths.customerOrderDetails.replace(':orderId', order.id)}
              className="flex flex-wrap items-center justify-between gap-2 p-4 transition hover:bg-background/40"
            >
              <div>
                <p className="text-sm font-medium text-heading">{order.orderNumber}</p>
                <p className="text-xs text-body/60">
                  {order.itemCount} item{order.itemCount > 1 ? 's' : ''} · {new Date(order.createdAt).toLocaleDateString()}
                </p>
              </div>
              <div className="flex items-center gap-4">
                <p className="text-sm font-semibold text-primary">৳ {order.total.toLocaleString()}</p>
                <Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>
                <span className="text-sm text-link">View details →</span>
              </div>
            </Link>
          ))}
          {orders.length === 0 && <p className="p-6 text-center text-sm text-body/60">No orders match this filter.</p>}
        </AsyncState>
      </div>

      {data?.totalPages > 1 && (
        <div className="mt-6">
          <Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
}
