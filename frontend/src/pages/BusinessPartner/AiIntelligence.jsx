import { useState } from 'react';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useCategories } from '../../hooks/useCategories';
import { useAiIntelligenceTools } from '../../hooks/useAiIntelligence';

function ToolCard({ title, children, mutation }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <p className="mb-3 text-sm font-semibold text-heading">{title}</p>
      <div className="space-y-3">{children}</div>
      {mutation.error && <p className="mt-2 text-xs text-red-600">{mutation.error.response?.data?.title || mutation.error.message}</p>}
      {mutation.data && (
        <pre className="mt-3 whitespace-pre-wrap rounded-lg bg-background p-3 text-xs text-body/70">
          {JSON.stringify(mutation.data, null, 2)}
        </pre>
      )}
    </div>
  );
}

export default function AiIntelligence() {
  const tools = useAiIntelligenceTools();
  const categoriesQuery = useCategories();
  const [categoryId, setCategoryId] = useState('');
  const [producerId, setProducerId] = useState('');
  const [quantity, setQuantity] = useState('');

  return (
    <div>
      <PageHeader title="AI Intelligence" description="AI-assisted supplier ranking, quality, pricing and risk tools." action={<Badge tone="primary">AI Powered</Badge>} />

      <div className="grid gap-6 lg:grid-cols-2">
        <ToolCard title="Supplier Ranking" mutation={tools.rankSuppliers}>
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm">
            <option value="">Any category</option>
            {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <Button variant="primary" onClick={() => tools.rankSuppliers.mutate({ categoryId: categoryId || undefined, maxResults: 10 })} disabled={tools.rankSuppliers.isPending}>
            Rank Suppliers
          </Button>
        </ToolCard>

        <ToolCard title="Quality Prediction" mutation={tools.predictQuality}>
          <input placeholder="Producer ID" value={producerId} onChange={(e) => setProducerId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button variant="primary" onClick={() => tools.predictQuality.mutate(producerId)} disabled={!producerId || tools.predictQuality.isPending}>
            Predict Quality
          </Button>
        </ToolCard>

        <ToolCard title="Price Forecast" mutation={tools.forecastPrice}>
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm">
            <option value="">Select category</option>
            {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <Button variant="primary" onClick={() => tools.forecastPrice.mutate({ categoryId, horizonMonths: 3 })} disabled={!categoryId || tools.forecastPrice.isPending}>
            Forecast Price
          </Button>
        </ToolCard>

        <ToolCard title="Delivery Prediction" mutation={tools.predictDelivery}>
          <input placeholder="Producer ID" value={producerId} onChange={(e) => setProducerId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input type="number" placeholder="Quantity (optional)" value={quantity} onChange={(e) => setQuantity(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button variant="primary" onClick={() => tools.predictDelivery.mutate({ producerId, quantity: quantity ? Number(quantity) : undefined })} disabled={!producerId || tools.predictDelivery.isPending}>
            Predict Delivery
          </Button>
        </ToolCard>

        <ToolCard title="Risk Assessment" mutation={tools.assessRisk}>
          <input placeholder="Producer ID" value={producerId} onChange={(e) => setProducerId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button variant="primary" onClick={() => tools.assessRisk.mutate(producerId)} disabled={!producerId || tools.assessRisk.isPending}>
            Assess Risk
          </Button>
        </ToolCard>
      </div>
    </div>
  );
}
