import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { products } from '../../data/mockData';

export default function Wishlist() {
  const items = products.slice(0, 4);

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
        {items.map((product) => (
          <div key={product.id} className="space-y-2">
            <ProductCard product={product} to={routePaths.customerProductDetails.replace(':productId', product.id)} />
            <Button variant="secondary" className="w-full">
              Remove
            </Button>
          </div>
        ))}
      </div>
    </div>
  );
}
