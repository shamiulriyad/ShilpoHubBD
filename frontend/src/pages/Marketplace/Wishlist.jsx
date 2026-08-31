import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useWishlist, useWishlistMutations } from '../../hooks/useWishlist';
import { useAuth } from '../../hooks/useAuth';

export default function Wishlist() {
  const { isAuthenticated } = useAuth();
  const { data, isLoading, isError, error } = useWishlist(isAuthenticated);
  const { remove } = useWishlistMutations();
  const items = data || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Wishlist' },
        ]}
        title="Wishlist"
        description={isAuthenticated ? `${items.length} saved items` : undefined}
      />

      {!isAuthenticated ? (
        <p className="rounded-xl border border-border bg-surface p-6 text-sm text-body/70">
          <Link to={routePaths.login} className="font-medium text-link hover:underline">
            Log in
          </Link>{' '}
          to view your wishlist.
        </p>
      ) : (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          <AsyncState isLoading={isLoading} isError={isError} error={error}>
            {items.map((item) => (
              <div key={item.id} className="space-y-2">
                <ProductCard
                  product={{
                    id: item.productId,
                    name: item.productName,
                    price: item.discountPrice ?? item.price,
                  }}
                  to={routePaths.marketplaceProductDetails.replace(':productId', item.productId)}
                />
                <Button variant="secondary" className="w-full" onClick={() => remove.mutate(item.productId)}>
                  Remove
                </Button>
              </div>
            ))}
            {items.length === 0 && <p className="col-span-full text-sm text-body/60">Your wishlist is empty.</p>}
          </AsyncState>
        </div>
      )}
    </div>
  );
}
