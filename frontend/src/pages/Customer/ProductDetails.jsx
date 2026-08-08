import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge, SectionHeader, WishlistButton } from '../../components/ui';
import { ProductCard, ReviewCard, MaterialTraceabilityCard } from '../../components/cards';
import { ProductGallery, Product360Viewer } from '../../components/media';
import { products, crafts, producers, reviews, materialTraceability } from '../../data/mockData';

const tabs = ['Details', 'Craft Story', 'Traceability', 'Reviews'];

export default function ProductDetails() {
  const { productId } = useParams();
  const [activeTab, setActiveTab] = useState(tabs[0]);
  const [view360, setView360] = useState(false);
  const product = products.find((p) => p.id === productId) || products[0];
  const related = products.filter((p) => p.id !== product.id).slice(0, 4);
  const craft = crafts.find((c) => c.name === product.category);
  const producer = producers.find((p) => p.name === product.producer) || producers[0];
  const traceability = materialTraceability.find((t) => t.craftId === craft?.id);

  return (
    <div>
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
          {view360 ? <Product360Viewer productName={product.name} /> : <ProductGallery productName={product.name} />}
        </div>

        <div>
          <Badge tone="secondary">{product.category}</Badge>
          <p className="mt-3 text-2xl font-semibold text-primary">৳ {product.price.toLocaleString()}</p>
          <p className="mt-2 text-sm text-body/70">
            Handcrafted by {product.producer} in {product.district}. Placeholder product description highlighting
            materials, technique and cultural significance.
          </p>

          <div className="mt-6 flex flex-wrap items-center gap-3">
            <Button variant="primary">Add to Cart</Button>
            <div className="flex items-center gap-2 rounded-md border border-border bg-surface px-3 py-2">
              <WishlistButton />
              <span className="text-sm font-medium text-title">Wishlist</span>
            </div>
          </div>

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
            to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
            className="mt-8 flex items-center gap-3 rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
          >
            <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
              {product.producer.slice(0, 1)}
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-heading">{product.producer}</p>
              <p className="text-xs text-body/60">{product.district}</p>
            </div>
            <span className="text-sm text-link">View profile →</span>
          </Link>

          {craft && (
            <Link
              to={routePaths.customerCraftStory.replace(':craftId', craft.id)}
              className="mt-3 flex items-center justify-between rounded-xl border border-border bg-surface p-4 text-sm transition hover:shadow-md"
            >
              <span className="font-medium text-heading">Read the story behind {craft.name}</span>
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
            <p>Category: {product.category}</p>
            <p>Producer: {product.producer}</p>
            <p>Origin: {product.district}</p>
            <p>Placeholder detail text describing dimensions, care instructions and authenticity verification.</p>
          </div>
        )}

        {activeTab === 'Craft Story' && (
          <div className="max-w-3xl space-y-3 text-sm text-body/80">
            {craft ? (
              <>
                <p>
                  This product belongs to the <span className="font-medium text-heading">{craft.name}</span> tradition,
                  practiced by {craft.producers}+ producers across Bangladesh.
                </p>
                <Link
                  to={routePaths.customerCraftStory.replace(':craftId', craft.id)}
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
              traceability.steps.map((step, index) => (
                <MaterialTraceabilityCard key={step.stage} step={step} index={index} />
              ))
            ) : (
              <p className="text-sm text-body/60">Traceability data unavailable for this product.</p>
            )}
          </div>
        )}

        {activeTab === 'Reviews' && (
          <div className="max-w-3xl space-y-4">
            {reviews.map((review) => (
              <ReviewCard key={review.id} review={review} />
            ))}
          </div>
        )}
      </div>

      <div className="mt-4">
        <SectionHeader eyebrow="You may also like" title="Related Products" />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {related.map((item) => (
            <ProductCard
              key={item.id}
              product={item}
              to={routePaths.customerProductDetails.replace(':productId', item.id)}
            />
          ))}
        </div>
      </div>
    </div>
  );
}
