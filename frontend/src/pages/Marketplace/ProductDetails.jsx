import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge, SectionHeader } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { products } from '../../data/mockData';

export default function ProductDetails() {
  const { productId } = useParams();
  const product = products.find((p) => p.id === productId) || products[0];
  const related = products.filter((p) => p.id !== product.id).slice(0, 4);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Products', path: routePaths.marketplaceProducts },
          { label: product.name },
        ]}
        title={product.name}
      />

      <div className="grid gap-10 lg:grid-cols-2">
        <div className="space-y-3">
          <div className="flex aspect-square items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
            Product Image
          </div>
          <div className="grid grid-cols-4 gap-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex aspect-square items-center justify-center rounded-lg border border-border bg-background text-[10px] text-body/30">
                Thumb
              </div>
            ))}
          </div>
        </div>

        <div>
          <Badge tone="secondary">{product.category}</Badge>
          <p className="mt-3 text-2xl font-semibold text-primary">৳ {product.price.toLocaleString()}</p>
          <p className="mt-2 text-sm text-body/70">
            Handcrafted by {product.producer} in {product.district}. Placeholder product description highlighting
            materials, technique and cultural significance.
          </p>

          <div className="mt-6 flex flex-wrap gap-3">
            <Button variant="primary">Add to Cart</Button>
            <Button variant="secondary">Add to Wishlist</Button>
          </div>

          <div className="mt-8 space-y-3 rounded-xl border border-border bg-surface p-4">
            <p className="text-sm font-semibold text-heading">Producer</p>
            <div className="flex items-center gap-3">
              <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                {product.producer.slice(0, 1)}
              </span>
              <div>
                <p className="text-sm font-medium text-heading">{product.producer}</p>
                <p className="text-xs text-body/60">{product.district}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-12">
        <SectionHeader eyebrow="You may also like" title="Related Products" />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {related.map((item) => (
            <ProductCard key={item.id} product={item} to={routePaths.marketplaceProductDetails.replace(':productId', item.id)} />
          ))}
        </div>
      </div>
    </div>
  );
}
