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
    <div className="supplier-workspace">
      <PageHeader title="Find Suppliers" description="Filter verified producers by category, expertise and price, then open one to see their details and products." />

      <section className="supplier-filter-panel" aria-label="Supplier filters">
      <div className="supplier-filter-heading"><h2>Refine your search</h2><button type="button" onClick={() => { setCategoryId(''); setExpertise(''); setMinPrice(''); setMaxPrice(''); }}>Reset filters</button></div>
      <div className="supplier-filters grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <label className="supplier-filter-label">Category
        <select aria-label="Category" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className={inputClass}>
          <option value="">Any category</option>
          {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select></label>
        <label className="supplier-filter-label">Expertise
        <select aria-label="Producer expertise" value={expertise} onChange={(e) => setExpertise(e.target.value)} className={inputClass}>
          <option value="">Any expertise</option>
          {expertiseOptions.map((e) => <option key={e} value={e}>{e}</option>)}
        </select></label>
        <label className="supplier-filter-label">Minimum price
        <input aria-label="Minimum price" type="number" min="0" placeholder="Min price (৳)" value={minPrice} onChange={(e) => setMinPrice(e.target.value)} className={inputClass} />
        </label><label className="supplier-filter-label">Maximum price
        <input aria-label="Maximum price" type="number" min="0" placeholder="Max price (৳)" value={maxPrice} onChange={(e) => setMaxPrice(e.target.value)} className={inputClass} />
        </label>
        {priceRangeInvalid && <p role="alert" className="text-sm text-error sm:col-span-4">Minimum price cannot be higher than maximum price.</p>}
      </div>
      </section>

      <div className="supplier-results-heading"><h2>Available producers</h2><span>{searchQuery.isLoading ? 'Finding suppliers…' : `${searchQuery.data?.totalCount ?? results.length} results`}</span></div>
      <div className="supplier-results-layout grid gap-5 xl:grid-cols-[minmax(0,1.5fr)_minmax(300px,1fr)]">
        <AsyncState isLoading={searchQuery.isLoading} isError={searchQuery.isError} error={searchQuery.error}>
          <div className="supplier-results space-y-3">
            {results.map((r) => (
              <button
                key={r.producerId}
                type="button"
                onClick={() => setSelectedProducerId(r.producerId)}
                aria-pressed={selectedProducerId === r.producerId}
                className={`supplier-result block w-full rounded-xl border p-4 text-left transition ${selectedProducerId === r.producerId ? 'border-primary bg-primary/5' : 'border-border bg-surface'}`}
              >
                <span className="supplier-avatar" aria-hidden="true">{(r.producerName || 'P').slice(0, 1).toUpperCase()}</span>
                <div className="supplier-result-body">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-semibold text-heading">{r.producerName}</p>
                  {r.isHandmadeVerified && <Badge tone="success">Verified</Badge>}
                </div>
                <p className="text-xs text-body/60">{[r.workshopName, r.primaryCraft, r.districtName].filter(Boolean).join(' · ') || 'No workshop details yet'}</p>
                <p className="mt-1 text-xs text-body/60">★ {Number(r.averageRating ?? 0).toFixed(1)} ({r.totalReviewCount}) · {r.productCount} products · ৳{Number(r.minPrice ?? 0).toLocaleString()}–{Number(r.maxPrice ?? 0).toLocaleString()}</p>
                </div><span className="supplier-open" aria-hidden="true">›</span>
              </button>
            ))}
            {results.length === 0 && !priceRangeInvalid && <p className="text-sm text-body/60">No producers match these filters.</p>}
          </div>
        </AsyncState>
        <aside className="supplier-detail h-fit rounded-xl border border-border bg-surface p-5" aria-label="Producer profile">
          <h2 className="supplier-detail-title">Producer profile</h2>
          {selectedProducerId ? <SupplierProfilePanel profile={profileQuery.data} isLoading={profileQuery.isLoading} /> : <div className="supplier-profile-empty"><span className="supplier-empty-icon" aria-hidden="true">↖</span><h3>Meet your next supplier</h3><p>Select a producer to explore their workshop, expertise, and available products.</p></div>}
        </aside>
      </div>
    </div>
  );
}
