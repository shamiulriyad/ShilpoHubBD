import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, Badge, Button, AsyncState } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { useAuth } from '../../hooks/useAuth';
import { useOrders } from '../../hooks/useOrders';
import { useWishlist } from '../../hooks/useWishlist';
import { useFollowedProducers } from '../../hooks/useProducerFollows';
import { useRecommendedForMe } from '../../hooks/useRecommendations';
import { toProductCardItem } from '../../utils/productAdapters';

const statusTone = {
  Delivered: 'success',
  Shipped: 'primary',
  Processing: 'secondary',
};

const asCount = (data) => (Array.isArray(data) ? data.length : data?.items?.length ?? 0);

// TODO(backend): no general "upcoming workshops" endpoint (only per-producer
// galleries) and no reward-points resource — placeholder content.
const upcomingWorkshops = [
  { id: 'workshop-1', title: 'Live Jamdani Loom Session', producer: 'Rahima Begum', craft: 'Jamdani Weaving', status: 'live' },
  { id: 'workshop-2', title: 'Nakshi Kantha Stitch Circle', producer: 'Abdul Karim', craft: 'Nakshi Kantha', status: 'upcoming' },
  { id: 'workshop-3', title: 'Terracotta Throwing Demo', producer: 'Shefali Rani', craft: 'Terracotta Art', status: 'upcoming' },
];

export default function CustomerDashboard() {
  const { user, isAuthenticated } = useAuth();
  const ordersQuery = useOrders({ pageSize: 5 }, isAuthenticated);
  const wishlistQuery = useWishlist(isAuthenticated);
  const followsQuery = useFollowedProducers();
  const recommendedQuery = useRecommendedForMe(4);

  const orders = ordersQuery.data?.items || [];

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
        <StatCard label="Orders Placed" value={ordersQuery.data?.totalCount ?? 0} />
        <StatCard label="Wishlist Items" value={asCount(wishlistQuery.data)} />
        <StatCard label="Following Producers" value={asCount(followsQuery.data)} />
        <StatCard label="Reward Points" value="—" />
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
        <AsyncState isLoading={ordersQuery.isLoading} isError={ordersQuery.isError} error={ordersQuery.error}>
          {orders.map((order) => (
            <div key={order.id} className="flex flex-wrap items-center justify-between gap-2 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{order.orderNumber}</p>
                <p className="text-xs text-body/60">
                  {order.itemCount} item{order.itemCount > 1 ? 's' : ''} ·{' '}
                  {new Date(order.createdAt).toLocaleDateString()}
                </p>
              </div>
              <div className="flex items-center gap-4">
                <p className="text-sm font-semibold text-primary">৳ {order.total.toLocaleString()}</p>
                <Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>
              </div>
            </div>
          ))}
          {orders.length === 0 && <p className="p-4 text-sm text-body/60">You haven’t placed any orders yet.</p>}
        </AsyncState>
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
        <AsyncState isLoading={recommendedQuery.isLoading} isError={recommendedQuery.isError} error={recommendedQuery.error}>
          {(recommendedQuery.data || []).map((product) => (
            <ProductCard
              key={product.id}
              product={toProductCardItem(product)}
              to={routePaths.customerProductDetails.replace(':productId', product.id)}
            />
          ))}
        </AsyncState>
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
