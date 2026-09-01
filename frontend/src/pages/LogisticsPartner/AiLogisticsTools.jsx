import { useState } from 'react';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useShipments } from '../../hooks/useShipments';
import { useDeliveryRoutes } from '../../hooks/useDeliveryRoutes';
import { useDistricts } from '../../hooks/useDistricts';
import {
  useDeliveryPredictions, useRouteOptimizationRuns, useDemandForecasts, useWarehouseAllocations, useAiLogisticsMutations,
} from '../../hooks/useAiLogistics';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const tabs = [
  { key: 'predictions', label: 'Delivery Predictions' },
  { key: 'routes', label: 'Route Optimization' },
  { key: 'demand', label: 'Demand Forecast' },
  { key: 'warehouse', label: 'Warehouse Allocation' },
];

const confidenceTone = { High: 'success', Medium: 'secondary', Low: 'neutral' };

function PredictionsTab() {
  const shipmentsQuery = useShipments({ pageSize: 100 });
  const { data } = useDeliveryPredictions({ pageSize: 20 });
  const { predictDelivery } = useAiLogisticsMutations();
  const [shipmentId, setShipmentId] = useState('');

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-end gap-2">
        <select value={shipmentId} onChange={(e) => setShipmentId(e.target.value)} className={inputClass}>
          <option value="">Select shipment…</option>
          {(shipmentsQuery.data?.items || []).map((s) => <option key={s.id} value={s.id}>{s.trackingNumber}</option>)}
        </select>
        <Button variant="primary" disabled={!shipmentId || predictDelivery.isPending} onClick={() => predictDelivery.mutate({ shipmentId })}>
          {predictDelivery.isPending ? 'Predicting…' : 'Predict Delivery'}
        </Button>
      </div>
      <div className="space-y-2">
        {(data?.items || []).map((p) => (
          <div key={p.id} className="rounded-lg border border-border bg-surface p-3 text-sm">
            <div className="flex items-center justify-between">
              <span className="font-medium text-heading">{p.shipmentTrackingNumber}</span>
              <Badge tone={confidenceTone[p.riskLevel] || 'neutral'}>{p.riskLevel} risk</Badge>
            </div>
            <p className="text-xs text-body/60">
              On-time probability {(p.onTimeProbability * 100).toFixed(0)}%
              {p.predictedDeliveryAt && ` · ETA ${new Date(p.predictedDeliveryAt).toLocaleString()}`}
              {' · '}confidence {p.confidence}
            </p>
          </div>
        ))}
        {(data?.items || []).length === 0 && <p className="text-sm text-body/60">No predictions generated yet.</p>}
      </div>
    </div>
  );
}

function RouteOptimizationTab() {
  const routesQuery = useDeliveryRoutes({ pageSize: 100 });
  const { data } = useRouteOptimizationRuns({ pageSize: 20 });
  const { optimizeRoute, applyRouteOptimization } = useAiLogisticsMutations();
  const [routeId, setRouteId] = useState('');
  const [objective, setObjective] = useState('proximity');

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-end gap-2">
        <select value={routeId} onChange={(e) => setRouteId(e.target.value)} className={inputClass}>
          <option value="">Select route…</option>
          {(routesQuery.data?.items || []).map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
        <select value={objective} onChange={(e) => setObjective(e.target.value)} className={inputClass}>
          {['proximity', 'balanced', 'capacity', 'coldchain', 'cost'].map((o) => <option key={o} value={o}>{o}</option>)}
        </select>
        <Button variant="primary" disabled={!routeId || optimizeRoute.isPending} onClick={() => optimizeRoute.mutate({ deliveryRouteId: routeId, objective })}>
          {optimizeRoute.isPending ? 'Optimizing…' : 'Run Optimization'}
        </Button>
      </div>
      <div className="space-y-2">
        {(data?.items || []).map((run) => (
          <div key={run.id} className="rounded-lg border border-border bg-surface p-3 text-sm">
            <div className="flex items-center justify-between">
              <span className="font-medium text-heading">{run.deliveryRouteCode} · {run.objective}</span>
              <div className="flex items-center gap-2">
                <Badge tone={confidenceTone[run.confidence] || 'neutral'}>{run.confidence}</Badge>
                {run.status !== 'Applied' && (
                  <button type="button" onClick={() => applyRouteOptimization.mutate(run.id)} className="text-xs text-primary hover:underline">Apply</button>
                )}
              </div>
            </div>
            <p className="text-xs text-body/60">
              {run.distanceSavingKm != null ? `Saves ${run.distanceSavingKm.toFixed?.(1) ?? run.distanceSavingKm} km` : 'No saving estimate'} · {run.status}
            </p>
          </div>
        ))}
        {(data?.items || []).length === 0 && <p className="text-sm text-body/60">No optimization runs yet.</p>}
      </div>
    </div>
  );
}

function DemandForecastTab() {
  const { data } = useDemandForecasts({ pageSize: 20 });
  const { forecastDemand } = useAiLogisticsMutations();
  const [scope, setScope] = useState('Network');
  const [metric, setMetric] = useState('shipments');
  const [horizonDays, setHorizonDays] = useState(14);

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-end gap-2">
        <select value={scope} onChange={(e) => setScope(e.target.value)} className={inputClass}>
          {['Network', 'District', 'Warehouse'].map((s) => <option key={s} value={s}>{s}</option>)}
        </select>
        <input placeholder="Metric (e.g. shipments)" value={metric} onChange={(e) => setMetric(e.target.value)} className={inputClass} />
        <input type="number" min="1" value={horizonDays} onChange={(e) => setHorizonDays(e.target.value)} className={`${inputClass} w-24`} />
        <Button variant="primary" disabled={forecastDemand.isPending} onClick={() => forecastDemand.mutate({ scope, metric, horizonDays: Number(horizonDays) })}>
          {forecastDemand.isPending ? 'Forecasting…' : 'Run Forecast'}
        </Button>
      </div>
      <div className="space-y-2">
        {(data?.items || []).map((f) => (
          <div key={f.id} className="rounded-lg border border-border bg-surface p-3 text-sm">
            <div className="flex items-center justify-between">
              <span className="font-medium text-heading">{f.scopeLabel} · {f.metric}</span>
              <Badge tone={confidenceTone[f.confidence] || 'neutral'}>{f.confidence}</Badge>
            </div>
            <p className="text-xs text-body/60">{f.horizonDays}-day predicted total: {f.predictedTotal.toFixed?.(1) ?? f.predictedTotal}</p>
          </div>
        ))}
        {(data?.items || []).length === 0 && <p className="text-sm text-body/60">No forecasts generated yet.</p>}
      </div>
    </div>
  );
}

function WarehouseAllocationTab() {
  const districtsQuery = useDistricts();
  const { data } = useWarehouseAllocations({ pageSize: 20 });
  const { recommendWarehouse } = useAiLogisticsMutations();
  const [form, setForm] = useState({ objective: 'balanced', sku: '', quantity: '', destinationDistrictId: '', requireColdChain: false });

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-end gap-2">
        <select value={form.objective} onChange={(e) => setForm((p) => ({ ...p, objective: e.target.value }))} className={inputClass}>
          {['balanced', 'proximity', 'capacity', 'coldchain', 'cost'].map((o) => <option key={o} value={o}>{o}</option>)}
        </select>
        <input placeholder="SKU (optional)" value={form.sku} onChange={(e) => setForm((p) => ({ ...p, sku: e.target.value }))} className={inputClass} />
        <input type="number" min="0" placeholder="Quantity" value={form.quantity} onChange={(e) => setForm((p) => ({ ...p, quantity: e.target.value }))} className={`${inputClass} w-24`} />
        <select value={form.destinationDistrictId} onChange={(e) => setForm((p) => ({ ...p, destinationDistrictId: e.target.value }))} className={inputClass}>
          <option value="">Destination district</option>
          {(districtsQuery.data || []).map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
        </select>
        <label className="flex items-center gap-2 text-sm text-body/70">
          <input type="checkbox" checked={form.requireColdChain} onChange={(e) => setForm((p) => ({ ...p, requireColdChain: e.target.checked }))} /> Cold chain
        </label>
        <Button
          variant="primary"
          disabled={recommendWarehouse.isPending}
          onClick={() => recommendWarehouse.mutate({ ...form, quantity: form.quantity === '' ? null : Number(form.quantity), destinationDistrictId: form.destinationDistrictId || null })}
        >
          {recommendWarehouse.isPending ? 'Recommending…' : 'Recommend Warehouse'}
        </Button>
      </div>
      <div className="space-y-2">
        {(data?.items || []).map((rec) => (
          <div key={rec.id} className="rounded-lg border border-border bg-surface p-3 text-sm">
            <div className="flex items-center justify-between">
              <span className="font-medium text-heading">{rec.recommendedWarehouseCode || '—'} {rec.sku ? `for ${rec.sku}` : ''}</span>
              <Badge tone={confidenceTone[rec.confidence] || 'neutral'}>{rec.confidence}</Badge>
            </div>
            <p className="text-xs text-body/60">{rec.objective} · {rec.optionCount} option(s) considered</p>
          </div>
        ))}
        {(data?.items || []).length === 0 && <p className="text-sm text-body/60">No recommendations generated yet.</p>}
      </div>
    </div>
  );
}

export default function AiLogisticsTools() {
  const [tab, setTab] = useState('predictions');

  return (
    <div>
      <PageHeader title="AI Logistics Tools" description="Delivery predictions, route optimization, demand forecasting and warehouse allocation recommendations." />

      <div className="mb-4 flex flex-wrap gap-2 border-b border-border">
        {tabs.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={`border-b-2 px-3 py-2 text-sm font-medium transition ${tab === t.key ? 'border-primary text-primary' : 'border-transparent text-body/60 hover:text-body'}`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'predictions' && <PredictionsTab />}
      {tab === 'routes' && <RouteOptimizationTab />}
      {tab === 'demand' && <DemandForecastTab />}
      {tab === 'warehouse' && <WarehouseAllocationTab />}
    </div>
  );
}
