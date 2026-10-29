import { Button } from '../ui';
import { useLogisticsPartnerRoutes } from '../../hooks/useLogisticsDirectory';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

// Shipping an item: producers never deliver themselves, and never choose the logistics partner --
// the customer already chose one (by delivery area) when they checked out. Producers just see who
// it is, optionally pick one of that partner's planned routes, and give weight / notes. The partner
// gets it as a shipment, the customer sees the tracking number, and only the partner can mark it delivered.
export default function LogisticsHandoffControls({ partnerId, partnerName, form, onChange, onShip, shipping }) {
  const routesQuery = useLogisticsPartnerRoutes(partnerId);
  const routes = routesQuery.data || [];
  const weightOk = form.weightKg === undefined || form.weightKg === '' || Number(form.weightKg) > 0;

  if (!partnerId) {
    return <p className="text-sm text-error">No logistics partner is assigned to this order. Contact support.</p>;
  }

  return (
    <div className="flex w-full flex-wrap items-center gap-2">
      <span className={`${inputClass} bg-background/60 text-body/80`}>
        Hand over to: <span className="font-medium text-heading">{partnerName || 'Assigned logistics partner'}</span>
      </span>
      <select
        aria-label="Delivery route"
        value={form.deliveryRouteId || ''}
        onChange={(e) => onChange({ deliveryRouteId: e.target.value })}
        className={inputClass}
      >
        <option value="">{routesQuery.isLoading ? 'Loading routes…' : routes.length ? 'Any route (partner decides)' : 'No routes published yet'}</option>
        {routes.map((r) => (
          <option key={r.routeId} value={r.routeId}>
            {r.name} — {[r.from, r.to].filter(Boolean).join(' → ') || r.originDistrictName || r.routeCode}
            {r.originDistrictName ? ` · ${r.originDistrictName}` : ''}{r.notes ? ` · via ${r.notes}` : ''}
            {r.scheduledDate ? ` · ${new Date(r.scheduledDate).toLocaleDateString()}` : ''}
          </option>
        ))}
      </select>
      <input aria-label="Parcel weight (kg)" type="number" min="0" step="any" placeholder="Weight (kg)" value={form.weightKg ?? ''} onChange={(e) => onChange({ weightKg: e.target.value })} className={`${inputClass} w-28`} />
      <input aria-label="Note for the partner" placeholder="Note for the partner (optional)" maxLength={300} value={form.notes || ''} onChange={(e) => onChange({ notes: e.target.value })} className={`${inputClass} min-w-[12rem] flex-1`} />
      <Button
        variant="primary"
        onClick={() => onShip({
          deliveryRouteId: form.deliveryRouteId || undefined,
          weightKg: form.weightKg ? Number(form.weightKg) : undefined,
          notes: form.notes?.trim() || undefined,
        })}
        disabled={shipping || !weightOk}
      >
        {shipping ? 'Handing over…' : 'Hand over to logistics'}
      </Button>
      <p className="basis-full text-xs text-body/60">The partner receives this order as a shipment and contacts you to arrange pickup. You and the customer are notified at every parcel update. The order counts as completed, and revenue is recorded, only after the partner marks it delivered.</p>
    </div>
  );
}
