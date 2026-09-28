import { useState } from 'react';
import { ProducerSelect } from '../../components/forms/EntityPickers';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useCategories } from '../../hooks/useCategories';
import { useAiIntelligenceTools } from '../../hooks/useAiIntelligence';

const LEVEL_TONE = { High: 'success', Medium: 'primary', Low: 'neutral' };
const RISK_TONE = { Low: 'success', Medium: 'primary', High: 'neutral' };
const QUALITY_TONE = { Excellent: 'success', Good: 'primary', Average: 'secondary', NeedsImprovement: 'neutral' };
const TREND_TONE = { Increasing: 'primary', Decreasing: 'neutral', Stable: 'secondary' };

function ToolCard({ title, children, mutation, renderResult }) {
  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      <p className="mb-3 text-sm font-semibold text-heading">{title}</p>
      <div className="space-y-3">{children}</div>
      {mutation.error && <p className="mt-2 text-xs text-red-600">{mutation.error.response?.data?.title || mutation.error.message}</p>}
      {mutation.data && <div className="mt-3">{renderResult(mutation.data)}</div>}
    </div>
  );
}

function SupplierRankingView({ data }) {
  if (!data.rankings?.length) {
    return <p className="rounded-lg bg-background p-3 text-sm text-body/60">No suppliers matched these filters.</p>;
  }
  return (
    <ol className="space-y-2">
      {data.rankings.map((r, i) => (
        <li key={r.producerId} className="rounded-lg bg-background p-3 text-sm">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="font-semibold text-heading">{i + 1}. {r.producerName}</span>
            <span className="text-xs text-body/60">Score {r.rankScore.toFixed(1)} · {Math.round(r.confidence * 100)}% confidence</span>
          </div>
          <p className="mt-1 text-xs text-body/70">{r.reasoning}</p>
        </li>
      ))}
    </ol>
  );
}

function QualityPredictionView({ data }) {
  return (
    <div className="rounded-lg bg-background p-3 text-sm">
      <div className="flex items-center justify-between">
        <span className="font-semibold text-heading">{Number(data.predictedQualityScore).toFixed(1)} / 100</span>
        <Badge tone={QUALITY_TONE[data.qualityTier] || 'neutral'}>{data.qualityTier}</Badge>
      </div>
      <p className="mt-2 text-xs text-body/70">{data.reasoning}</p>
    </div>
  );
}

function PriceForecastView({ data }) {
  return (
    <div className="rounded-lg bg-background p-3 text-sm">
      <div className="mb-2 flex items-center justify-between">
        <span className="font-semibold text-heading">Trend</span>
        <Badge tone={TREND_TONE[data.trend] || 'neutral'}>{data.trend}</Badge>
      </div>
      {data.forecastedPrices?.length > 0 && (
        <ul className="space-y-1">
          {data.forecastedPrices.map((p) => (
            <li key={p.periodStart} className="flex justify-between text-xs text-body/70">
              <span>{new Date(p.periodStart).toLocaleDateString(undefined, { year: 'numeric', month: 'short' })}</span>
              <span className="font-medium text-heading">৳ {Number(p.averagePrice).toLocaleString('en-BD')}</span>
            </li>
          ))}
        </ul>
      )}
      <p className="mt-2 text-xs text-body/70">{data.recommendation}</p>
    </div>
  );
}

function DeliveryPredictionView({ data }) {
  return (
    <div className="rounded-lg bg-background p-3 text-sm">
      <div className="flex items-center justify-between">
        <span className="font-semibold text-heading">{data.predictedDeliveryDays} day(s)</span>
        <Badge tone={LEVEL_TONE[data.confidenceLevel] || 'neutral'}>{data.confidenceLevel} confidence</Badge>
      </div>
      <p className="mt-2 text-xs text-body/70">{data.reasoning}</p>
    </div>
  );
}

function RiskAssessmentView({ data }) {
  return (
    <div className="rounded-lg bg-background p-3 text-sm">
      <div className="flex items-center justify-between">
        <span className="font-semibold text-heading">Risk score {Number(data.riskScore).toFixed(1)} / 100</span>
        <Badge tone={RISK_TONE[data.riskLevel] || 'neutral'}>{data.riskLevel} risk</Badge>
      </div>
      {data.riskFactors?.length > 0 && (
        <ul className="mt-2 list-disc space-y-1 pl-5 text-xs text-body/70">
          {data.riskFactors.map((f) => <li key={f}>{f}</li>)}
        </ul>
      )}
      <p className="mt-2 text-xs text-body/70">{data.recommendation}</p>
    </div>
  );
}

export default function AiIntelligence() {
  const tools = useAiIntelligenceTools();
  const categoriesQuery = useCategories();
  const [categoryId, setCategoryId] = useState('');
  const [producerId, setProducerId] = useState('');
  const [quantity, setQuantity] = useState('');
  // The API rejects quantities below 1; block them here instead of accepting -27.
  const quantityInvalid = quantity !== '' && (!Number.isInteger(Number(quantity)) || Number(quantity) < 1);

  return (
    <div>
      <PageHeader title="AI Intelligence" description="AI-assisted supplier ranking, quality, pricing and risk tools." action={<Badge tone="primary">AI Powered</Badge>} />

      <div className="grid gap-6 lg:grid-cols-2">
        <ToolCard title="Supplier Ranking" mutation={tools.rankSuppliers} renderResult={(data) => <SupplierRankingView data={data} />}>
          <select aria-label="Category Id" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm">
            <option value="">Any category</option>
            {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <Button variant="primary" onClick={() => tools.rankSuppliers.mutate({ categoryId: categoryId || undefined, maxResults: 10 })} disabled={tools.rankSuppliers.isPending}>
            Rank Suppliers
          </Button>
        </ToolCard>

        <ToolCard title="Quality Prediction" mutation={tools.predictQuality} renderResult={(data) => <QualityPredictionView data={data} />}>
          <ProducerSelect id="ai-producer-quality" label="Producer" value={producerId} onChange={setProducerId} />
          <Button variant="primary" onClick={() => tools.predictQuality.mutate(producerId)} disabled={!producerId || tools.predictQuality.isPending}>
            Predict Quality
          </Button>
        </ToolCard>

        <ToolCard title="Price Forecast" mutation={tools.forecastPrice} renderResult={(data) => <PriceForecastView data={data} />}>
          <select aria-label="Category Id" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm">
            <option value="">Select category</option>
            {(categoriesQuery.data || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          <Button variant="primary" onClick={() => tools.forecastPrice.mutate({ categoryId, horizonMonths: 3 })} disabled={!categoryId || tools.forecastPrice.isPending}>
            Forecast Price
          </Button>
        </ToolCard>

        <ToolCard title="Delivery Prediction" mutation={tools.predictDelivery} renderResult={(data) => <DeliveryPredictionView data={data} />}>
          <ProducerSelect id="ai-producer-delivery" label="Producer" value={producerId} onChange={setProducerId} />
          <input aria-label="Quantity" type="number" min="1" step="1" placeholder="Quantity (optional, whole units)" value={quantity} onChange={(e) => setQuantity(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          {quantityInvalid && <p role="alert" className="text-xs text-error">Quantity must be a whole number of 1 or more.</p>}
          <Button variant="primary" onClick={() => tools.predictDelivery.mutate({ producerId, quantity: quantity ? Number(quantity) : undefined })} disabled={!producerId || quantityInvalid || tools.predictDelivery.isPending}>
            Predict Delivery
          </Button>
        </ToolCard>

        <ToolCard title="Risk Assessment" mutation={tools.assessRisk} renderResult={(data) => <RiskAssessmentView data={data} />}>
          <ProducerSelect id="ai-producer-risk" label="Producer" value={producerId} onChange={setProducerId} />
          <Button variant="primary" onClick={() => tools.assessRisk.mutate(producerId)} disabled={!producerId || tools.assessRisk.isPending}>
            Assess Risk
          </Button>
        </ToolCard>
      </div>
    </div>
  );
}
