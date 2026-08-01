import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, Badge, Button } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { useAuth } from '../../hooks/useAuth';
import { products, orders, workshops } from '../../data/mockData';

const statusTone = {
  Delivered: 'success',
  Shipped: 'primary',
  Processing: 'secondary',
};

export default function CustomerDashboard() {
  const { user } = useAuth();
  const recommended = products.slice(0, 4);
  const upcomingWorkshops = workshops.filter((w) => w.status !== 'past').slice(0, 3);

  return (
    <div>
      <PageHeader
        title={`Welcome back${user?.name ? `, ${user.name}` : ''}`}
        description="Track your orders, revisit your wishlist and discover new heritage products."
        action={
          <Link to={routePaths.customerMarketplace}>
            <Button variant="primary">Browse Marketplace</Button>
          </Link>
        }
      />

      <div className="mb-10 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Orders Placed" value={orders.length} />
        <StatCard label="Wishlist Items" value="14" />
        <StatCard label="Favorite Producers" value="5" />
        <StatCard label="Reward Points" value="320" />
      </div>

      <SectionHeader
        eyebrow="Orders"
        title="Recent Orders"
        action={
          <Link to={routePaths.customerCart} className="text-sm font-medium text-link hover:underline">
            View cart →
          </Link>
        }
      />
      <div className="mb-10 divide-y divide-border rounded-xl border border-border bg-surface">
        {orders.map((order) => (
          <div key={order.id} className="flex flex-wrap items-center justify-between gap-2 p-4">
            <div>
              <p className="text-sm font-medium text-heading">{order.id}</p>
              <p className="text-xs text-body/60">
                {order.items} item{order.items > 1 ? 's' : ''} · {order.date}
              </p>
            </div>
            <div className="flex items-center gap-4">
              <p className="text-sm font-semibold text-primary">৳ {order.total.toLocaleString()}</p>
              <Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>
            </div>
          </div>
        ))}
      </div>

      <SectionHeader
        eyebrow="For You"
        title="Recommended Products"
        action={
          <Link to={routePaths.customerMarketplace} className="text-sm font-medium text-link hover:underline">
            View all →
          </Link>
        }
      />
      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {recommended.map((product) => (
          <ProductCard
            key={product.id}
            product={product}
            to={routePaths.customerProductDetails.replace(':productId', product.id)}
          />
        ))}
      </div>

      <SectionHeader
        eyebrow="Live Commerce"
        title="Upcoming & Live Workshops"
        action={
          <Link to={routePaths.customerWorkshops} className="text-sm font-medium text-link hover:underline">
            View gallery →
          </Link>
        }
      />
      <div className="grid gap-4 sm:grid-cols-3">
        {upcomingWorkshops.map((workshop) => (
          <div key={workshop.id} className="rounded-xl border border-border bg-surface p-4">
            <Badge tone={workshop.status === 'live' ? 'success' : 'secondary'}>
              {workshop.status === 'live' ? 'Live Now' : 'Upcoming'}
            </Badge>
            <p className="mt-3 text-sm font-semibold text-heading">{workshop.title}</p>
            <p className="text-xs text-body/60">
              {workshop.producer} · {workshop.craft}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
