import { useState } from 'react';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useSupplierSearch, useSupplierProfile } from '../../hooks/useSupplierDiscovery';
import { useCategories } from '../../hooks/useCategories';

export default function SupplierDiscovery() {
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [selectedProducerId, setSelectedProducerId] = useState(null);
  const categoriesQuery = useCategories();
  const searchQuery = useSupplierSearch({ search: search || undefined, categoryId: categoryId || undefined, pageSize: 20 });
  const profileQuery = useSupplierProfile(selectedProducerId);

  const results = searchQuery.data?.items || [];
  const profile = profileQuery.data;

  return (
    <div>
      <PageHeader title="Supplier Discovery" description="Search verified heritage producers by craft, price and rating." />

      <div className="mb-6 flex flex-wrap gap-3">
        <input
          placeholder="Search producers, workshops…"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <select value={categoryId} onChange={(event) => setCategoryId(event.target.value)} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          <option value="">All categories</option>
          {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
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
                <p className="text-xs text-body/60">{r.workshopName} · {r.primaryCraft} · {r.districtName}</p>
                <p className="mt-1 text-xs text-body/60">★ {r.averageRating.toFixed(1)} ({r.totalReviewCount}) · {r.productCount} products · ৳{r.minPrice.toLocaleString()}–{r.maxPrice.toLocaleString()}</p>
              </button>
            ))}
            {results.length === 0 && <p className="text-sm text-body/60">No suppliers match your search.</p>}
          </div>
        </AsyncState>

        <div className="h-fit rounded-xl border border-border bg-surface p-5">
          {profile ? (
            <div className="space-y-2">
              <p className="text-sm font-semibold text-heading">{profile.producerName}</p>
              <p className="text-xs text-body/60">{profile.workshopName} · {profile.primaryCraft}</p>
              <p className="text-sm text-body/70">{profile.workshopDescription}</p>
              <p className="text-xs text-body/60">★ {profile.averageRating.toFixed(1)} · {profile.productCount} products · {profile.estimatedProductionCapacity} units/mo capacity</p>
              {profile.certifications?.length > 0 && (
                <div className="flex flex-wrap gap-1">
                  {profile.certifications.map((c, i) => <Badge key={i} tone="secondary">{c.name}</Badge>)}
                </div>
              )}
            </div>
          ) : (
            <p className="text-sm text-body/60">Select a producer to view their full profile.</p>
          )}
        </div>
      </div>
    </div>
  );
}
