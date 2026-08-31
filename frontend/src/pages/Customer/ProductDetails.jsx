import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge, SectionHeader, WishlistButton, AsyncState } from '../../components/ui';
import { ProductCard, ReviewCard, MaterialTraceabilityCard } from '../../components/cards';
import { ProductGallery, Product360Viewer } from '../../components/media';
import { useProduct } from '../../hooks/useProducts';
import { useSimilarProducts } from '../../hooks/useRecommendations';
import { useProductReviews, useReviewMutations } from '../../hooks/useReviews';
import { useCartMutations } from '../../hooks/useCart';
import { useWishlistMutations } from '../../hooks/useWishlist';
import { useAuth } from '../../hooks/useAuth';
import { useCraftStory } from '../../hooks/useCraftStories';
import { useProductTraceability } from '../../hooks/useTraceability';
import { toProductCardItem } from '../../utils/productAdapters';

const tabs = ['Details', 'Craft Story', 'Traceability', 'Reviews'];

export default function ProductDetails() {
  const { productId } = useParams();
  const { isAuthenticated } = useAuth();
  const [activeTab, setActiveTab] = useState(tabs[0]);
  const [view360, setView360] = useState(false);
  const [newReview, setNewReview] = useState({ rating: 5, comment: '' });

  const productQuery = useProduct(productId);
  const product = productQuery.data;
  const similarQuery = useSimilarProducts(productId, 4);
  const reviewsQuery = useProductReviews(productId);
  const { create: createReview } = useReviewMutations(productId);

  const [wishlisted, setWishlisted] = useState(false);
  const { add: addToCart } = useCartMutations();
  const { add: addToWishlist, remove: removeFromWishlist } = useWishlistMutations();

  const craftStoryQuery = useCraftStory(product?.hasCraftStory ? product.categoryId : null);
  const traceabilityQuery = useProductTraceability(productId);
  const craftStory = craftStoryQuery.data;
  const traceability = traceabilityQuery.data;

  const handleSubmitReview = (event) => {
    event.preventDefault();
    createReview.mutate(
      { productId, rating: Number(newReview.rating), comment: newReview.comment, imageUrls: [] },
      { onSuccess: () => setNewReview({ rating: 5, comment: '' }) },
    );
  };

  return (
    <div>
      <AsyncState isLoading={productQuery.isLoading} isError={productQuery.isError} error={productQuery.error}>
        {product && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Dashboard', path: routePaths.customer },
                { label: 'Marketplace', path: routePaths.customerMarketplace },
                { label: product.name },
              ]}
              title={product.name}
            />

            <div className="grid gap-10 lg:grid-cols-2">
              <div className="space-y-3">
                <div className="flex justify-end">
                  <button
                    type="button"
                    onClick={() => setView360((prev) => !prev)}
                    className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-body hover:bg-background"
                  >
                    {view360 ? 'Show Gallery' : 'Show 360° View'}
                  </button>
                </div>
                {view360 ? (
                  <Product360Viewer productName={product.name} />
                ) : (
                  <ProductGallery productName={product.name} />
                )}
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
                  {product.description || `Handcrafted by ${product.producerName} in ${product.districtName}.`}
                </p>

                <div className="mt-6 flex flex-wrap items-center gap-3">
                  <Button
                    variant="primary"
                    disabled={!isAuthenticated || addToCart.isPending}
                    onClick={() => addToCart.mutate({ productId: product.id, quantity: 1 })}
                  >
                    {addToCart.isPending ? 'Adding…' : 'Add to Cart'}
                  </Button>
                  <div className="flex items-center gap-2 rounded-md border border-border bg-surface px-3 py-2">
                    <WishlistButton
                      active={wishlisted}
                      onChange={(next) => {
                        if (!isAuthenticated) return;
                        setWishlisted(next);
                        if (next) {
                          addToWishlist.mutate(product.id);
                        } else {
                          removeFromWishlist.mutate(product.id);
                        }
                      }}
                    />
                    <span className="text-sm font-medium text-title">Wishlist</span>
                  </div>
                </div>
                {!isAuthenticated && (
                  <p className="mt-2 text-xs text-body/50">Log in to add items to your cart or wishlist.</p>
                )}

                <div className="mt-4 flex flex-wrap gap-4 text-sm">
                  <Link
                    to={routePaths.customerAISimilarProducts.replace(':productId', product.id)}
                    className="text-link hover:underline"
                  >
                    Find similar products →
                  </Link>
                  <Link
                    to={routePaths.customerAIInteriorPreview.replace(':productId', product.id)}
                    className="text-link hover:underline"
                  >
                    Preview in your room →
                  </Link>
                  <Link
                    to={routePaths.customerAIFashionMatching.replace(':productId', product.id)}
                    className="text-link hover:underline"
                  >
                    Complete the look →
                  </Link>
                </div>

                <Link
                  to={routePaths.customerProducerProfile.replace(':producerId', product.producerId)}
                  className="mt-8 flex items-center gap-3 rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
                >
                  <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                    {product.producerName.slice(0, 1)}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium text-heading">{product.producerName}</p>
                    <p className="text-xs text-body/60">{product.districtName}</p>
                  </div>
                  <span className="text-sm text-link">View profile →</span>
                </Link>

                {craftStory && (
                  <Link
                    to={routePaths.customerCraftStory.replace(':craftId', product.categoryId)}
                    className="mt-3 flex items-center justify-between rounded-xl border border-border bg-surface p-4 text-sm transition hover:shadow-md"
                  >
                    <span className="font-medium text-heading">Read the story behind {product.categoryName}</span>
                    <span className="text-link">Explore →</span>
                  </Link>
                )}
              </div>
            </div>

            <div className="mt-10 flex gap-2 border-b border-border">
              {tabs.map((tab) => (
                <button
                  key={tab}
                  type="button"
                  onClick={() => setActiveTab(tab)}
                  className={`border-b-2 px-4 py-2 text-sm font-medium ${
                    activeTab === tab ? 'border-primary text-primary' : 'border-transparent text-body/60 hover:text-body'
                  }`}
                >
                  {tab}
                </button>
              ))}
            </div>

            <div className="py-8">
              {activeTab === 'Details' && (
                <div className="max-w-3xl space-y-3 text-sm text-body/80">
                  <p>Category: {product.categoryName}</p>
                  <p>Producer: {product.producerName}</p>
                  <p>Origin: {product.districtName}</p>
                  <p>Stock: {product.stock > 0 ? `${product.stock} available` : 'Out of stock'}</p>
                </div>
              )}

              {activeTab === 'Craft Story' && (
                <div className="max-w-3xl space-y-3 text-sm text-body/80">
                  {craftStory ? (
                    <>
                      <p>
                        This product belongs to the <span className="font-medium text-heading">{product.categoryName}</span>{' '}
                        tradition. {craftStory.summary}
                      </p>
                      <Link
                        to={routePaths.customerCraftStory.replace(':craftId', product.categoryId)}
                        className="inline-block text-link hover:underline"
                      >
                        Read the full craft story →
                      </Link>
                    </>
                  ) : (
                    <p>Craft story unavailable for this product.</p>
                  )}
                </div>
              )}

              {activeTab === 'Traceability' && (
                <div className="max-w-3xl space-y-3">
                  {traceability ? (
                    <>
                      {traceability.summary && <p className="text-sm text-body/80">{traceability.summary}</p>}
                      {traceability.materialSources.map((source, index) => (
                        <MaterialTraceabilityCard
                          key={source.materialName}
                          step={{ stage: source.materialName, location: source.sourceLocation, detail: source.description }}
                          index={index}
                        />
                      ))}
                    </>
                  ) : (
                    <p className="text-sm text-body/60">Traceability data unavailable for this product.</p>
                  )}
                </div>
              )}

              {activeTab === 'Reviews' && (
                <div className="max-w-3xl space-y-4">
                  {isAuthenticated && (
                    <form onSubmit={handleSubmitReview} className="space-y-3 rounded-xl border border-border bg-surface p-4">
                      <select
                        value={newReview.rating}
                        onChange={(event) => setNewReview((prev) => ({ ...prev, rating: event.target.value }))}
                        className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                      >
                        {[5, 4, 3, 2, 1].map((r) => (
                          <option key={r} value={r}>
                            {r} star{r > 1 ? 's' : ''}
                          </option>
                        ))}
                      </select>
                      <textarea
                        required
                        rows={3}
                        placeholder="Share your experience with this product…"
                        value={newReview.comment}
                        onChange={(event) => setNewReview((prev) => ({ ...prev, comment: event.target.value }))}
                        className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
                      />
                      <Button type="submit" variant="primary" disabled={createReview.isPending}>
                        {createReview.isPending ? 'Posting…' : 'Post Review'}
                      </Button>
                    </form>
                  )}
                  <AsyncState isLoading={reviewsQuery.isLoading} isError={reviewsQuery.isError} error={reviewsQuery.error}>
                    {reviewsQuery.data?.items.map((review) => (
                      <ReviewCard
                        key={review.id}
                        review={{ author: review.reviewerName, rating: review.rating, comment: review.comment, date: review.createdAt }}
                      />
                    ))}
                    {reviewsQuery.data?.items.length === 0 && (
                      <p className="text-sm text-body/60">No reviews yet — be the first to share your experience.</p>
                    )}
                  </AsyncState>
                </div>
              )}
            </div>

            {similarQuery.data?.length > 0 && (
              <div className="mt-4">
                <SectionHeader eyebrow="You may also like" title="Related Products" />
                <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                  {similarQuery.data.map((item) => (
                    <ProductCard
                      key={item.id}
                      product={toProductCardItem(item)}
                      to={routePaths.customerProductDetails.replace(':productId', item.id)}
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
