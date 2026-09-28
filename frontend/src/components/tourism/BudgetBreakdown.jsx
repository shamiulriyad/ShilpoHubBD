// Renders a BudgetPlanResult (backend/src/.../DTOs/AITourism/BudgetPlanResult.cs) exactly as the
// backend computed it -- no client-side math. Verified line items (real TouristService prices, real
// TransportOption fares) are always kept visually separate from the assumed per-diem estimate, and
// any cost the backend could not verify is listed honestly instead of being folded into a total.
export default function BudgetBreakdown({ budget, statedBudget }) {
  const hasLineItems = budget.lineItems?.length > 0;
  const comparisonTotal = budget.estimatedTotal || budget.totalEstimatedCost;

  return (
    <div className="rounded-xl border border-border bg-surface p-5">
      {hasLineItems ? (
        <ul className="space-y-2">
          {budget.lineItems.map((item, i) => (
            <li key={i} className="flex justify-between text-sm text-body/70">
              <span>{item.label} <span className="text-body/40">({item.category})</span></span>
              <span className="font-medium text-heading">৳ {Number(item.amount).toLocaleString('en-BD')}</span>
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-sm text-body/60">No verified costs yet — nothing priced could be confirmed for this selection.</p>
      )}
      <div className="mt-4 flex items-center justify-between border-t border-border pt-4">
        <span className="text-sm font-semibold text-heading">Known / verified total</span>
        <span className="text-lg font-semibold text-primary">৳ {Number(budget.totalEstimatedCost).toLocaleString('en-BD')}</span>
      </div>
      <p className="mt-1 text-xs text-body/50">৳ {Number(budget.perPersonCost).toLocaleString('en-BD')} per person</p>
      {statedBudget && Number(statedBudget) < comparisonTotal && (
        <p className="mt-3 text-sm text-amber-700">This is above your stated budget of ৳ {Number(statedBudget).toLocaleString('en-BD')}.</p>
      )}
      {budget.estimatedItems?.length > 0 && (
        <div className="mt-4 rounded-lg border border-border p-3">
          <p className="text-xs font-semibold text-heading">Rough estimate for the unverified parts</p>
          <ul className="mt-2 space-y-1">
            {budget.estimatedItems.map((item, i) => (
              <li key={i} className="flex justify-between text-xs text-body/70">
                <span>{item.label}</span>
                <span className="font-medium text-heading">৳ {Number(item.amount).toLocaleString('en-BD')}</span>
              </li>
            ))}
          </ul>
          <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
            <span className="text-sm font-semibold text-heading">Estimated total (with assumptions)</span>
            <span className="text-lg font-semibold text-primary">৳ {Number(budget.estimatedTotal).toLocaleString('en-BD')}</span>
          </div>
          <p className="mt-1 text-xs text-body/50">৳ {Number(budget.estimatedPerPerson).toLocaleString('en-BD')} per person · {budget.estimateNote}</p>
        </div>
      )}
      {budget.unverifiedCosts?.length > 0 && (
        <div className="mt-4 rounded-lg bg-amber-50 p-3">
          <p className="text-xs font-semibold text-amber-800">Unverified / excluded from the total</p>
          <ul className="mt-1 list-disc pl-5 text-xs text-amber-800/80">
            {budget.unverifiedCosts.map((note, i) => (
              <li key={i}>{note}</li>
            ))}
          </ul>
        </div>
      )}
      {budget.notes && <p className="mt-3 text-xs text-body/50">{budget.notes}</p>}
    </div>
  );
}
