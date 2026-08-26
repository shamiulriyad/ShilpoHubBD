import { useState } from 'react';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useCategories } from '../../hooks/useCategories';
import { useSupplierMatch } from '../../hooks/useSupplierMatching';

export default function SupplierMatching() {
  const categoriesQuery = useCategories();
  const match = useSupplierMatch();
  const [form, setForm] = useState({ categoryId: '', productKeyword: '', quantity: '', maxBudgetPerUnit: '' });

  const handleSubmit = (event) => {
    event.preventDefault();
    match.mutate({
      categoryId: form.categoryId || undefined,
      productKeyword: form.productKeyword || undefined,
      quantity: form.quantity ? Number(form.quantity) : undefined,
      maxBudgetPerUnit: form.maxBudgetPerUnit ? Number(form.maxBudgetPerUnit) : undefined,
    });
  };

  return (
    <div>
      <PageHeader title="Supplier Matching" description="Get a ranked list of producers matched to your requirements." action={<Badge tone="primary">AI Powered</Badge>} />

      <form onSubmit={handleSubmit} className="mb-8 grid gap-3 rounded-xl border border-border bg-surface p-5 sm:grid-cols-2">
        <select value={form.categoryId} onChange={(e) => setForm((p) => ({ ...p, categoryId: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm">
          <option value="">Any category</option>
          {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <input placeholder="Product keyword" value={form.productKeyword} onChange={(e) => setForm((p) => ({ ...p, productKeyword: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <input type="number" placeholder="Quantity needed" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <input type="number" placeholder="Max budget per unit (৳)" value={form.maxBudgetPerUnit} onChange={(e) => setForm((p) => ({ ...p, maxBudgetPerUnit: e.target.value }))} className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
        <Button type="submit" variant="primary" className="sm:col-span-2" disabled={match.isPending}>
          {match.isPending ? 'Matching…' : 'Find Matches'}
        </Button>
      </form>

      <div className="space-y-3">
        {(match.data || []).map((result) => (
          <div key={result.producerId} className="rounded-xl border border-border bg-surface p-4">
            <div className="flex items-center justify-between">
              <p className="text-sm font-semibold text-heading">{result.producerName}</p>
              <Badge tone="success">{Math.round(result.matchScore)}% match</Badge>
            </div>
            <p className="text-xs text-body/60">{result.workshopName} · {result.primaryCraft} · {result.districtName}</p>
            <p className="mt-1 text-xs text-body/60">★ {result.averageRating.toFixed(1)} · from ৳{result.minPrice.toLocaleString()} · {result.estimatedProductionCapacity} units/mo</p>
            {result.matchReasons?.length > 0 && (
              <ul className="mt-2 list-inside list-disc text-xs text-body/60">
                {result.matchReasons.map((r, i) => <li key={i}>{r}</li>)}
              </ul>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
