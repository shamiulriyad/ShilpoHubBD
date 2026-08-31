import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader } from '../../components/ui';
import { useCategories } from '../../hooks/useCategories';
import { useGiftRecommendations } from '../../hooks/useAiShopping';

const occasions = ['Birthday', 'Wedding', 'Anniversary', 'Housewarming'];

export default function AIGiftRecommendation() {
  const categoriesQuery = useCategories();
  const giftRecommendations = useGiftRecommendations();
  const [occasion, setOccasion] = useState(occasions[0]);
  const [recipientInterest, setRecipientInterest] = useState('');
  const [budget, setBudget] = useState('');

  const handleSubmit = (event) => {
    event.preventDefault();
    giftRecommendations.mutate({
      occasion,
      recipientInterest,
      budget: budget ? Number(budget) : undefined,
    });
  };

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'AI Gift Recommendation' }]}
        title="AI Gift Recommendation"
        description="Answer a few questions and let AI suggest a heritage gift they'll love."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <form onSubmit={handleSubmit} className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-6 rounded-xl border border-border bg-surface p-6">
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Occasion</p>
            <div className="grid gap-3 sm:grid-cols-2">
              {occasions.map((item) => (
                <label key={item} className="flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm">
                  <input type="radio" name="occasion" checked={occasion === item} onChange={() => setOccasion(item)} />
                  {item}
                </label>
              ))}
            </div>
          </div>

          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Recipient's Interests</p>
            <div className="flex flex-wrap gap-2">
              {(categoriesQuery.data || []).map((category) => (
                <button
                  key={category.id}
                  type="button"
                  onClick={() => setRecipientInterest(category.name)}
                  className={`rounded-full border px-3 py-1.5 text-sm ${
                    recipientInterest === category.name ? 'border-primary bg-primary text-surface' : 'border-border bg-background text-body'
                  }`}
                >
                  {category.name}
                </button>
              ))}
            </div>
          </div>

          <input
            type="number"
            placeholder="Budget range (৳)"
            value={budget}
            onChange={(event) => setBudget(event.target.value)}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />

          <Button type="submit" variant="primary" className="w-full" disabled={giftRecommendations.isPending}>
            {giftRecommendations.isPending ? 'Thinking…' : 'Get Recommendations'}
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

      {giftRecommendations.data && (
        <div className="mt-10">
          <SectionHeader eyebrow="AI Curated" title="Recommended Gifts" />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {giftRecommendations.data.map((gift, i) => (
              <div key={i} className="rounded-xl border border-border bg-surface p-4">
                <Badge tone="secondary">{gift.category}</Badge>
                <p className="mt-2 text-sm font-semibold text-heading">{gift.productName}</p>
                <p className="mt-1 text-xs text-body/60">{gift.reason}</p>
                <p className="mt-2 text-sm font-semibold text-primary">৳ {gift.estimatedPrice.toLocaleString()}</p>
              </div>
            ))}
            {giftRecommendations.data.length === 0 && (
              <p className="col-span-full text-sm text-body/60">No suggestions came back — try different interests.</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
