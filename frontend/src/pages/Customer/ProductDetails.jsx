import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge, SectionHeader } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { products, crafts, producers, reviews } from '../../data/mockData';

const tabs = ['Details', 'Craft Story', 'Reviews'];

export default function ProductDetails() {
  const { productId } = useParams();
  const [activeTab, setActiveTab] = useState(tabs[0]);
  const product = products.find((p) => p.id === productId) || products[0];
  const related = products.filter((p) => p.id !== product.id).slice(0, 4);
  const craft = crafts.find((c) => c.name === product.category);
  const producer = producers.find((p) => p.name === product.producer) || producers[0];

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

        {activeTab === 'Reviews' && (
          <div className="max-w-3xl space-y-4">
            {reviews.map((review) => (
              <div key={review.id} className="rounded-xl border border-border bg-surface p-4">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-medium text-heading">{review.author}</p>
                  <span className="text-xs text-secondary">{'★'.repeat(review.rating)}</span>
                </div>
                <p className="mt-2 text-sm text-body/70">{review.comment}</p>
                <p className="mt-1 text-xs text-body/50">{review.date}</p>
              </div>
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
