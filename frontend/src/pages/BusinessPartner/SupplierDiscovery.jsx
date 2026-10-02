import { useState } from 'react';
import { PageHeader, Badge, AsyncState, Pagination } from '../../components/ui';
import { useSupplierSearch, useSupplierProfile, useSupplierBusinessProfile } from '../../hooks/useSupplierDiscovery';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';
import SupplierProfilePanel from '../../components/business/SupplierProfilePanel';
import ProducerBusinessProfilePanel from '../../components/business/ProducerBusinessProfilePanel';
import { useExpertiseOptions } from '../../hooks/useProfile';

const SORT_OPTIONS = [
  { value: 'RatingDesc', label: 'Highest rated' },
  { value: 'Newest', label: 'Newest' },
  { value: 'ProductCountDesc', label: 'Most products' },
  { value: 'PriceLowToHigh', label: 'Price: low to high' },
  { value: 'PriceHighToLow', label: 'Price: high to low' },
  { value: 'ProductionCapacityDesc', label: 'Production capacity' },
];

export default function SupplierDiscovery() {
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [districtId, setDistrictId] = useState('');
  const [expertise, setExpertise] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [handmadeVerifiedOnly, setHandmadeVerifiedOnly] = useState(false);
  const [certifiedOnly, setCertifiedOnly] = useState(false);
  const [sortBy, setSortBy] = useState('RatingDesc');
  const [page, setPage] = useState(1);
  const [selectedProducerId, setSelectedProducerId] = useState(null);

  const expertiseOptions = useExpertiseOptions().data || [];
  const categoriesQuery = useCategories();
  const districtsQuery = useDistricts();
  const searchQuery = useSupplierSearch({
    search: search || undefined,
    categoryId: categoryId || undefined,
    districtId: districtId || undefined,
    expertise: expertise || undefined,
    minPrice: minPrice || undefined,
    maxPrice: maxPrice || undefined,
    handmadeVerifiedOnly: handmadeVerifiedOnly || undefined,
    certifiedOnly: certifiedOnly || undefined,
    sortBy,
    page,
    pageSize: 20,
  });
  const profileQuery = useSupplierProfile(selectedProducerId);
  const businessProfileQuery = useSupplierBusinessProfile(selectedProducerId);

  const results = searchQuery.data?.items || [];
  const totalPages = Math.max(1, Math.ceil((searchQuery.data?.totalCount || 0) / (searchQuery.data?.pageSize || 20)));
  const profile = profileQuery.data;

  const resetToFirstPage = (setter) => (value) => {
    setter(value);
    setPage(1);
  };

  return (
    <div>
      <PageHeader title="Supplier Discovery" description="Search verified heritage producers by craft, price and rating." />

      <div className="mb-4 flex flex-wrap gap-3">
        <input aria-label="Search producers, workshops"
          placeholder="Search producers, workshops…"
          value={search}
          onChange={(event) => resetToFirstPage(setSearch)(event.target.value)}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <select aria-label="Category" value={categoryId} onChange={(event) => resetToFirstPage(setCategoryId)(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          <option value="">All categories</option>
          {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <select aria-label="District" value={districtId} onChange={(event) => resetToFirstPage(setDistrictId)(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          <option value="">All districts</option>
          {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
        </select>
        <select aria-label="Producer expertise" value={expertise} onChange={(event) => resetToFirstPage(setExpertise)(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          <option value="">Any expertise</option>
          {expertiseOptions.map((e) => <option key={e} value={e}>{e}</option>)}
        </select>
        <select aria-label="Sort by" value={sortBy} onChange={(event) => resetToFirstPage(setSortBy)(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          {SORT_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>
      </div>

      <div className="mb-6 flex flex-wrap items-center gap-4">
        <input aria-label="Minimum price" type="number" min="0" placeholder="Min price"
          value={minPrice} onChange={(event) => resetToFirstPage(setMinPrice)(event.target.value)}
          className="w-28 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <input aria-label="Maximum price" type="number" min="0" placeholder="Max price"
          value={maxPrice} onChange={(event) => resetToFirstPage(setMaxPrice)(event.target.value)}
          className="w-28 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={handmadeVerifiedOnly} onChange={(event) => resetToFirstPage(setHandmadeVerifiedOnly)(event.target.checked)} />
          Handmade-verified only
        </label>
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={certifiedOnly} onChange={(event) => resetToFirstPage(setCertifiedOnly)(event.target.checked)} />
          Certified only
        </label>
      </div>

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <div>
          <AsyncState isLoading={searchQuery.isLoading} isError={searchQuery.isError} error={searchQuery.error}>
            <div className="space-y-3">
              {results.map((r) => (
                <button
                  key={r.producerId}
                  type="button"
                  onClick={() => !r.isDemo && setSelectedProducerId(r.producerId)}
                  className={`block w-full rounded-xl border p-4 text-left transition ${selectedProducerId === r.producerId ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'} ${r.isDemo ? 'cursor-default' : ''}`}
                >
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-semibold text-heading">{r.producerName}</p>
                    {r.isHandmadeVerified && <Badge tone="success">Verified</Badge>}
                  </div>
                  <p className="text-xs text-body/60">{r.workshopName} · {r.primaryCraft} · {r.districtName}</p>
                  <p className="mt-1 text-xs text-body/60">★ {r.averageRating.toFixed(1)} ({r.totalReviewCount}) · {r.productCount} products · ৳{r.minPrice.toLocaleString()}–{r.maxPrice.toLocaleString()}</p>
                </button>
              ))}
              {results.length === 0 && <p className="text-sm text-body/60">No suppliers match your search.</p>}
            </div>
          </AsyncState>

          {totalPages > 1 && (
            <div className="mt-6">
              <Pagination currentPage={page} totalPages={totalPages} onPageChange={setPage} />
            </div>
          )}
        </div>

        <div className="space-y-4">
          <div className="h-fit rounded-xl border border-border bg-surface p-5">
            <SupplierProfilePanel profile={selectedProducerId ? profile : null} isLoading={Boolean(selectedProducerId) && profileQuery.isLoading} />
          </div>
          {selectedProducerId && (
            <ProducerBusinessProfilePanel profile={businessProfileQuery.data} isLoading={businessProfileQuery.isLoading} />
          )}
        </div>
      </div>
    </div>
  );
}
