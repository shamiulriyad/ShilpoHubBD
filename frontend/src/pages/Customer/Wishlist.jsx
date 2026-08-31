import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useWishlist, useWishlistMutations } from '../../hooks/useWishlist';

export default function Wishlist() {
  const { data, isLoading, isError, error } = useWishlist();
  const { remove } = useWishlistMutations();
  const items = data || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'Wishlist' },
        ]}
        title="Wishlist"
        description={`${items.length} saved items`}
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {items.map((item) => (
            <div key={item.id} className="space-y-2">
              <ProductCard
                product={{ id: item.productId, name: item.productName, price: item.discountPrice ?? item.price }}
                to={routePaths.customerProductDetails.replace(':productId', item.productId)}
              />
              <Button variant="secondary" className="w-full" onClick={() => remove.mutate(item.productId)}>
                Remove
              </Button>
            </div>
          ))}
          {items.length === 0 && <p className="col-span-full text-sm text-body/60">Your wishlist is empty.</p>}
        </AsyncState>
      </div>
    </div>
  );
}
