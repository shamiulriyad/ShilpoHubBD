import { Badge } from '../ui';

const money = (v) => `৳ ${Number(v ?? 0).toLocaleString('en-BD')}`;

const INSPECTION_LABEL = { NotRequired: 'Not yet', Pending: 'Waiting for admin', Approved: 'Approved', Rejected: 'Rejected' };
const INSPECTION_TONE = { NotRequired: 'neutral', Pending: 'secondary', Approved: 'success', Rejected: 'neutral' };

// Advance (at least 50%) and admin-inspection summary shared by the partner, producer and admin views.
export default function ProcurementAdvanceInfo({ req }) {
  const paid = req.advancePaidAt && !req.advanceRefundedAt;
  return (
    <dl className="grid grid-cols-2 gap-3 text-xs sm:grid-cols-4">
      <div><dt className="text-body/60">Total</dt><dd className="font-semibold text-heading">{money(req.itemsTotal)}</dd></div>
      <div><dt className="text-body/60">Advance needed (50%)</dt><dd className="font-semibold text-heading">{money(req.requiredAdvance)}</dd></div>
      <div>
        <dt className="text-body/60">Advance</dt>
        <dd className="font-semibold text-heading">
          {req.advanceRefundedAt ? `${money(req.advanceAmount)} (refunded)` : paid ? `${money(req.advanceAmount)} paid` : 'Not paid'}
        </dd>
      </div>
      <div>
        <dt className="text-body/60">Admin inspection</dt>
        <dd><Badge tone={INSPECTION_TONE[req.inspectionStatus] || 'neutral'}>{INSPECTION_LABEL[req.inspectionStatus] || req.inspectionStatus}</Badge></dd>
      </div>
      {req.inspectionNotes && <p className="col-span-full text-body/70">Admin note: “{req.inspectionNotes}”</p>}
    </dl>
  );
}
