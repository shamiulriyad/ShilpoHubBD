import { useState } from 'react';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useCategories } from '../../hooks/useCategories';
import { useSupplierSearch, useSupplierProfile } from '../../hooks/useSupplierDiscovery';
import SupplierProfilePanel from '../../components/business/SupplierProfilePanel';
import { useExpertiseOptions } from '../../hooks/useProfile';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

// Plain filter search (no AI): pick a category and a price range, then open any producer to see their details.
export default function SupplierMatching() {
  const categoriesQuery = useCategories();
  const [categoryId, setCategoryId] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [expertise, setExpertise] = useState('');
  const expertiseOptions = useExpertiseOptions().data || [];
  const [selectedProducerId, setSelectedProducerId] = useState(null);

  const priceRangeInvalid = minPrice !== '' && maxPrice !== '' && Number(minPrice) > Number(maxPrice);
  const searchQuery = useSupplierSearch({
    categoryId: categoryId || undefined,
    expertise: expertise || undefined,
    minPrice: minPrice !== '' ? Number(minPrice) : undefined,
    maxPrice: maxPrice !== '' ? Number(maxPrice) : undefined,
    pageSize: 50,
  });
  const profileQuery = useSupplierProfile(selectedProducerId);
  const results = priceRangeInvalid ? [] : searchQuery.data?.items || [];

  return (
    <div>
      <PageHeader title="Find Suppliers" description="Filter verified producers by category, expertise and price, then open one to see their details and products." />

      <div className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-4">
        <select aria-label="Category" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className={inputClass}>
          <option value="">Any category</option>
          {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <select aria-label="Producer expertise" value={expertise} onChange={(e) => setExpertise(e.target.value)} className={inputClass}>
          <option value="">Any expertise</option>
          {expertiseOptions.map((e) => <option key={e} value={e}>{e}</option>)}
        </select>
        <input aria-label="Minimum price" type="number" min="0" placeholder="Min price (৳)" value={minPrice} onChange={(e) => setMinPrice(e.target.value)} className={inputClass} />
        <input aria-label="Maximum price" type="number" min="0" placeholder="Max price (৳)" value={maxPrice} onChange={(e) => setMaxPrice(e.target.value)} className={inputClass} />
        {priceRangeInvalid && <p role="alert" className="text-sm text-error sm:col-span-4">Minimum price cannot be higher than maximum price.</p>}
      </div>

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <AsyncState isLoading={searchQuery.isLoading} isError={searchQuery.isError} error={searchQuery.error}>
          <div className="space-y-3">
            {results.map((r) => (
              <button
                key={r.producerId}
                type="button"
                onClick={() => setSelectedProducerId(r.producerId)}
                className={`block w-full rounded-xl border p-4 text-left transition ${selectedProducerId === r.producerId ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'}`}
              >
                <div className="flex items-center justify-between">
                  <p className="text-sm font-semibold text-heading">{r.producerName}</p>
                  {r.isHandmadeVerified && <Badge tone="success">Verified</Badge>}
                </div>
                <p className="text-xs text-body/60">{[r.workshopName, r.primaryCraft, r.districtName].filter(Boolean).join(' · ') || 'No workshop details yet'}</p>
                <p className="mt-1 text-xs text-body/60">★ {Number(r.averageRating ?? 0).toFixed(1)} ({r.totalReviewCount}) · {r.productCount} products · ৳{Number(r.minPrice ?? 0).toLocaleString()}–{Number(r.maxPrice ?? 0).toLocaleString()}</p>
              </button>
            ))}
            {results.length === 0 && !priceRangeInvalid && <p className="text-sm text-body/60">No producers match these filters.</p>}
          </div>
        </AsyncState>
        <div className="h-fit rounded-xl border border-border bg-surface p-5">
          <SupplierProfilePanel profile={selectedProducerId ? profileQuery.data : null} isLoading={Boolean(selectedProducerId) && profileQuery.isLoading} />
        </div>
      </div>
    </div>
  );
}
