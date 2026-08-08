import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { orders } from '../../data/mockData';

const filters = ['All', 'Delivered', 'Shipped', 'Processing'];
const statusTone = { Delivered: 'success', Shipped: 'primary', Processing: 'secondary' };

export default function OrderHistory() {
  const [filter, setFilter] = useState('All');
  const visible = orders.filter((order) => filter === 'All' || order.status === filter);

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Order History' }]}
        title="Order History"
        description={`${orders.length} orders placed on ShilpoHub.`}
      />

      <div className="mb-6 flex flex-wrap gap-2">
        {filters.map((item) => (
          <button
            key={item}
            type="button"
            onClick={() => setFilter(item)}
            className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${
              filter === item ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-body hover:bg-background'
            }`}
          >
            {item}
          </button>
        ))}
      </div>

      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {visible.map((order) => (
          <Link
            key={order.id}
            to={routePaths.customerOrderDetails.replace(':orderId', order.id)}
            className="flex flex-wrap items-center justify-between gap-2 p-4 transition hover:bg-background/40"
          >
            <div>
              <p className="text-sm font-medium text-heading">{order.id}</p>
              <p className="text-xs text-body/60">
                {order.items} item{order.items > 1 ? 's' : ''} · {order.date}
              </p>
            </div>
            <div className="flex items-center gap-4">
              <p className="text-sm font-semibold text-primary">৳ {order.total.toLocaleString()}</p>
              <Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>
              <span className="text-sm text-link">View details →</span>
            </div>
          </Link>
        ))}
        {visible.length === 0 && <p className="p-6 text-center text-sm text-body/60">No orders match this filter.</p>}
      </div>
    </div>
  );
}
