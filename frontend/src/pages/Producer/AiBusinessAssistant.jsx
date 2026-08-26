import { useState } from 'react';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useAiBusinessTools } from '../../hooks/useAiBusiness';

function ToolCard({ title, children, result, mutation }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <p className="mb-3 text-sm font-semibold text-heading">{title}</p>
      <div className="space-y-3">{children}</div>
      {mutation.error && <p className="mt-2 text-xs text-red-600">{mutation.error.response?.data?.title || mutation.error.message}</p>}
      {result && (
        <pre className="mt-3 whitespace-pre-wrap rounded-lg bg-background p-3 text-xs text-body/70">
          {JSON.stringify(result, null, 2)}
        </pre>
      )}
    </div>
  );
}

export default function AiBusinessAssistant() {
  const tools = useAiBusinessTools();
  const [priceForm, setPriceForm] = useState({ categoryId: '', estimatedCost: '', desiredMarginPercent: '' });
  const [descForm, setDescForm] = useState({ productName: '', categoryName: '', keywords: '' });

  return (
    <div>
      <PageHeader title="AI Business Assistant" description="AI-powered tools to help price, describe and forecast your products." action={<Badge tone="primary">AI Powered</Badge>} />

      <div className="grid gap-6 lg:grid-cols-2">
        <ToolCard title="Price Suggestion" result={tools.suggestPrice.data} mutation={tools.suggestPrice}>
          <input placeholder="Category ID" value={priceForm.categoryId} onChange={(e) => setPriceForm((p) => ({ ...p, categoryId: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input type="number" placeholder="Estimated cost (optional)" value={priceForm.estimatedCost} onChange={(e) => setPriceForm((p) => ({ ...p, estimatedCost: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button
            variant="primary"
            onClick={() => tools.suggestPrice.mutate({ categoryId: priceForm.categoryId, estimatedCost: priceForm.estimatedCost ? Number(priceForm.estimatedCost) : undefined })}
            disabled={!priceForm.categoryId || tools.suggestPrice.isPending}
          >
            Suggest Price
          </Button>
        </ToolCard>

        <ToolCard title="Product Description Generator" result={tools.generateDescription.data} mutation={tools.generateDescription}>
          <input placeholder="Product name" value={descForm.productName} onChange={(e) => setDescForm((p) => ({ ...p, productName: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input placeholder="Keywords (comma separated)" value={descForm.keywords} onChange={(e) => setDescForm((p) => ({ ...p, keywords: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button
            variant="primary"
            onClick={() => tools.generateDescription.mutate({ productName: descForm.productName, keywords: descForm.keywords.split(',').map((k) => k.trim()).filter(Boolean) })}
            disabled={!descForm.productName || tools.generateDescription.isPending}
          >
            Generate Description
          </Button>
        </ToolCard>

        <ToolCard title="Sales Insights" result={tools.generateSalesInsights.data} mutation={tools.generateSalesInsights}>
          <p className="text-xs text-body/60">Analyzes your recent sales and surfaces actionable insights.</p>
          <Button variant="primary" onClick={() => tools.generateSalesInsights.mutate({})} disabled={tools.generateSalesInsights.isPending}>
            Generate Insights
          </Button>
        </ToolCard>

        <ToolCard title="Seasonal Trend Prediction" result={tools.predictSeasonalTrend.data} mutation={tools.predictSeasonalTrend}>
          <p className="text-xs text-body/60">Predicts monthly demand scores across the year.</p>
          <Button variant="primary" onClick={() => tools.predictSeasonalTrend.mutate({})} disabled={tools.predictSeasonalTrend.isPending}>
            Predict Trend
          </Button>
        </ToolCard>
      </div>
    </div>
  );
}
