import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge, SectionHeader, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useProduct, useProducts } from '../../hooks/useProducts';
import { useCartMutations } from '../../hooks/useCart';
import { useWishlistMutations } from '../../hooks/useWishlist';
import { useAuth } from '../../hooks/useAuth';
import { toProductCardItem } from '../../utils/productAdapters';

export default function ProductDetails() {
  const { productId } = useParams();
  const { isAuthenticated } = useAuth();
  const productQuery = useProduct(productId);
  const product = productQuery.data;
  const relatedQuery = useProducts(product ? { categoryId: product.categoryId, pageSize: 5 } : {});
  const related = (relatedQuery.data?.items || []).filter((p) => p.id !== productId).slice(0, 4);

  const { add: addToCart } = useCartMutations();
  const { add: addToWishlist } = useWishlistMutations();

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={productQuery.isLoading} isError={productQuery.isError} error={productQuery.error}>
        {product && (
          <>
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
                  {product.imageUrls?.[0] ? (
                    <img src={product.imageUrls[0]} alt={product.name} className="h-full w-full rounded-2xl object-cover" />
                  ) : (
                    'Product Image'
                  )}
                </div>
                <div className="grid grid-cols-4 gap-3">
                  {(product.imageUrls?.slice(1, 5).length ? product.imageUrls.slice(1, 5) : Array.from({ length: 4 })).map((url, i) => (
                    <div key={i} className="flex aspect-square items-center justify-center rounded-lg border border-border bg-background text-[10px] text-body/30">
                      {url ? <img src={url} alt="" className="h-full w-full rounded-lg object-cover" /> : 'Thumb'}
                    </div>
                  ))}
                </div>
              </div>

              <div>
                <Badge tone="secondary">{product.categoryName}</Badge>
                <p className="mt-3 text-2xl font-semibold text-primary">
                  ৳ {(product.discountPrice ?? product.price).toLocaleString()}
                  {product.discountPrice && (
                    <span className="ml-2 text-base font-normal text-body/40 line-through">
                      ৳ {product.price.toLocaleString()}
                    </span>
                  )}
                </p>
                <p className="mt-2 text-sm text-body/70">
                  {product.description ||
                    `Handcrafted by ${product.producerName} in ${product.districtName}.`}
                </p>

                <div className="mt-6 flex flex-wrap gap-3">
                  <Button
                    variant="primary"
                    disabled={!isAuthenticated || addToCart.isPending}
                    onClick={() => addToCart.mutate({ productId: product.id, quantity: 1 })}
                  >
                    {addToCart.isPending ? 'Adding…' : 'Add to Cart'}
                  </Button>
                  <Button
                    variant="secondary"
                    disabled={!isAuthenticated || addToWishlist.isPending}
                    onClick={() => addToWishlist.mutate(product.id)}
                  >
                    Add to Wishlist
                  </Button>
                </div>
                {!isAuthenticated && (
                  <p className="mt-2 text-xs text-body/50">Log in to add items to your cart or wishlist.</p>
                )}

                <div className="mt-8 space-y-3 rounded-xl border border-border bg-surface p-4">
                  <p className="text-sm font-semibold text-heading">Producer</p>
                  <div className="flex items-center gap-3">
                    <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                      {product.producerName.slice(0, 1)}
                    </span>
                    <div>
                      <p className="text-sm font-medium text-heading">{product.producerName}</p>
                      <p className="text-xs text-body/60">{product.districtName}</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {related.length > 0 && (
              <div className="mt-12">
                <SectionHeader eyebrow="You may also like" title="Related Products" />
                <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                  {related.map((item) => (
                    <ProductCard
                      key={item.id}
                      product={toProductCardItem(item)}
                      to={routePaths.marketplaceProductDetails.replace(':productId', item.id)}
                    />
                  ))}
                </div>
              </div>
            )}
          </>
        )}
      </AsyncState>
    </div>
  );
}
