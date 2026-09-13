import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, Badge, Button, AsyncState } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { useAuth } from '../../hooks/useAuth';
import { useOrders } from '../../hooks/useOrders';
import { useWishlist } from '../../hooks/useWishlist';
import { useFollowedProducers } from '../../hooks/useProducerFollows';
import { useRecommendedForMe } from '../../hooks/useRecommendations';
import { useLiveEvents } from '../../hooks/useLiveEvents';
import { toProductCardItem } from '../../utils/productAdapters';

const statusTone = {
  Delivered: 'success',
  Shipped: 'primary',
  Processing: 'secondary',
};

const asCount = (data) => (Array.isArray(data) ? data.length : data?.items?.length ?? 0);

export default function CustomerDashboard() {
  const { user, isAuthenticated } = useAuth();
  const ordersQuery = useOrders({ pageSize: 5 }, isAuthenticated);
  const wishlistQuery = useWishlist(isAuthenticated);
  const followsQuery = useFollowedProducers();
  const recommendedQuery = useRecommendedForMe(4);
  const liveEventsQuery = useLiveEvents({ pageSize: 3 });

  const orders = ordersQuery.data?.items || [];
  const liveEvents = liveEventsQuery.data?.items || [];

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

      <div className="mb-10 grid gap-4 sm:grid-cols-3">
        <StatCard label="Orders Placed" value={ordersQuery.data?.totalCount ?? 0} />
        <StatCard label="Wishlist Items" value={asCount(wishlistQuery.data)} />
        <StatCard label="Following Producers" value={asCount(followsQuery.data)} />
      </div>

      <SectionHeader
        eyebrow="Orders"
        title="Recent Orders"
        action={
          <Link to={routePaths.customerOrders} className="text-sm font-medium text-link hover:underline">
            View orders →
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
      <AsyncState isLoading={liveEventsQuery.isLoading} isError={liveEventsQuery.isError} error={liveEventsQuery.error}>
        <div className="grid gap-4 sm:grid-cols-3">
          {liveEvents.map((event) => (
            <Link
              key={event.id}
              to={routePaths.customerLiveShopping.replace(':workshopId', event.id)}
              className="rounded-xl border border-border bg-surface p-4 transition hover:border-primary/30 hover:shadow-sm"
            >
              <Badge tone={event.status === 'Live' ? 'success' : event.status === 'Scheduled' ? 'secondary' : 'neutral'}>
                {event.status === 'Live' ? 'Live Now' : event.status}
              </Badge>
              <p className="mt-3 text-sm font-semibold text-heading">{event.title}</p>
              <p className="mt-1 text-xs text-body/60">{event.producerName} · {event.productName}</p>
            </Link>
          ))}
          {liveEvents.length === 0 && <p className="text-sm text-body/60">No live shopping events are available yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}