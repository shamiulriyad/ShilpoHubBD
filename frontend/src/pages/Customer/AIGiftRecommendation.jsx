import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { products, crafts } from '../../data/mockData';

const occasions = ['Birthday', 'Wedding', 'Anniversary', 'Housewarming'];

export default function AIGiftRecommendation() {
  const [submitted, setSubmitted] = useState(false);
  const recommendations = products.slice(0, 4);

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'AI Gift Recommendation' }]}
        title="AI Gift Recommendation"
        description="Answer a few questions and let AI suggest a heritage gift they'll love."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <form
        onSubmit={(event) => {
          event.preventDefault();
          setSubmitted(true);
        }}
        className="grid gap-8 lg:grid-cols-[2fr_1fr]"
      >
        <div className="space-y-6 rounded-xl border border-border bg-surface p-6">
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Occasion</p>
            <div className="grid gap-3 sm:grid-cols-2">
              {occasions.map((occasion) => (
                <label key={occasion} className="flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm">
                  <input type="radio" name="occasion" />
                  {occasion}
                </label>
              ))}
            </div>
          </div>

          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Craft Interests</p>
            <div className="flex flex-wrap gap-2">
              {crafts.map((craft) => (
                <label
                  key={craft.id}
                  className="flex items-center gap-2 rounded-full border border-border bg-background px-3 py-1.5 text-sm"
                >
                  <input type="checkbox" />
                  {craft.name}
                </label>
              ))}
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <input placeholder="Budget range (৳)" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
            <input placeholder="Recipient's name (optional)" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
          </div>

          <Button type="submit" variant="primary" className="w-full">
            Get Recommendations
          </Button>
        </div>

        <div className="h-fit space-y-3 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">How it Works</p>
          <ol className="space-y-2 text-sm text-body/70">
            <li>1. Tell us the occasion and interests.</li>
            <li>2. AI matches heritage products to your answers.</li>
            <li>3. Browse the curated recommendations.</li>
          </ol>
        </div>
      </form>

      {submitted && (
        <div className="mt-10">
          <SectionHeader eyebrow="AI Curated" title="Recommended Gifts" />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {recommendations.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                to={routePaths.customerProductDetails.replace(':productId', product.id)}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
