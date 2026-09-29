import { useMemo, useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { useMyPartnershipAgreements } from '../../hooks/useProducerPartnershipAgreements';
import { useProducerPartnershipSettlements, useProducerPartnershipSettlementMutations } from '../../hooks/useProducerPartnershipSettlements';

const STATUS_TONE = { Draft: 'neutral', PendingApproval: 'secondary', Approved: 'success', Rejected: 'neutral' };
const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;
const dt = (value) => (value ? new Date(value).toLocaleDateString() : '—');

export default function ProducerPartnershipSettlements() {
  const [statusFilter, setStatusFilter] = useState('');
  const [selectedAgreementId, setSelectedAgreementId] = useState('');
  const [periodStart, setPeriodStart] = useState('');
  const [periodEnd, setPeriodEnd] = useState('');
  const [expandedId, setExpandedId] = useState(null);

  const agreementsQuery = useMyPartnershipAgreements({ pageSize: 100 });
  const activeAgreements = (agreementsQuery.data?.items || []).filter((a) => a.status === 'Active');

  const listQuery = useProducerPartnershipSettlements({ status: statusFilter || undefined, pageSize: 50 });
  const { generate, submitForApproval, approve, reject } = useProducerPartnershipSettlementMutations();
  const settlements = listQuery.data?.items || [];

  const handleGenerate = (e) => {
    e.preventDefault();
    generate.mutate({ agreementId: selectedAgreementId, periodStart, periodEnd });
  };

  return (
    <div>
      <PageHeader
        title="Partnership Settlement Dashboard"
        description="Calculate each partnership's revenue split for a period, review, and approve. There is no payout integration — approval confirms the calculation only, not that money moved."
      />

      <form onSubmit={handleGenerate} className="mb-8 space-y-3 rounded-xl border border-border bg-surface p-5">
        <p className="text-xs font-semibold uppercase tracking-wide text-body/50">Generate a settlement</p>
        <div className="grid gap-3 sm:grid-cols-4">
          <label className="block text-sm sm:col-span-2"><span className="mb-1 block text-xs text-body/60">Active partnership</span>
            <select required value={selectedAgreementId} onChange={(e) => setSelectedAgreementId(e.target.value)} className="w-full rounded-md border border-border bg-background px-2 py-1.5 text-sm">
              <option value="">Choose…</option>
              {activeAgreements.map((a) => <option key={a.id} value={a.id}>{a.producerName} × {a.businessPartnerName}</option>)}
            </select>
          </label>
          <label className="block text-sm"><span className="mb-1 block text-xs text-body/60">Period start</span>
            <input required type="date" value={periodStart} onChange={(e) => setPeriodStart(e.target.value)} className="w-full rounded-md border border-border bg-background px-2 py-1.5 text-sm" /></label>
          <label className="block text-sm"><span className="mb-1 block text-xs text-body/60">Period end</span>
            <input required type="date" value={periodEnd} onChange={(e) => setPeriodEnd(e.target.value)} className="w-full rounded-md border border-border bg-background px-2 py-1.5 text-sm" /></label>
        </div>
        <MutationFeedback mutation={generate} successMessage="Settlement calculated as Draft." />
        <Button type="submit" variant="primary" disabled={generate.isPending}>{generate.isPending ? 'Calculating…' : 'Calculate settlement'}</Button>
        {activeAgreements.length === 0 && !agreementsQuery.isLoading && (
          <p className="text-xs text-body/60">No active partnerships yet — a settlement can only be generated for one that's Active.</p>
        )}
      </form>

      <div className="mb-4 flex items-center gap-2">
        <label className="text-sm text-body/70">Filter by status</label>
        <select aria-label="Settlement status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="rounded-md border border-border bg-background px-2 py-1.5 text-sm">
          <option value="">All</option>
          <option value="Draft">Draft</option>
          <option value="PendingApproval">Pending approval</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
      </div>

      <MutationFeedback mutation={submitForApproval} successMessage="Submitted for approval." />
      <MutationFeedback mutation={approve} successMessage="Settlement approved (calculation confirmed — no payout is implied)." />
      <MutationFeedback mutation={reject} successMessage="Settlement rejected." />

      <AsyncState isLoading={listQuery.isLoading} isError={listQuery.isError} error={listQuery.error}>
        <div className="space-y-2">
          {settlements.map((s) => (
            <SettlementRow
              key={s.id} settlement={s} expanded={expandedId === s.id}
              onToggle={() => setExpandedId(expandedId === s.id ? null : s.id)}
              onSubmit={() => submitForApproval.mutate(s.id)}
              onApprove={() => approve.mutate(s.id)}
              onReject={(reason) => reject.mutate({ id: s.id, reason })}
              busy={submitForApproval.isPending || approve.isPending || reject.isPending}
            />
          ))}
          {settlements.length === 0 && <TravelEmptyState title="No settlements yet" description="Generate one for an active partnership above." />}
        </div>
      </AsyncState>
    </div>
  );
}

function SettlementRow({ settlement: s, expanded, onToggle, onSubmit, onApprove, onReject, busy }) {
  const [rejectReason, setRejectReason] = useState('');
  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <button type="button" onClick={onToggle} className="flex w-full flex-wrap items-center justify-between gap-2 text-left">
        <div>
          <p className="text-sm font-semibold text-heading">{s.producerName} × {s.businessPartnerName}</p>
          <p className="text-xs text-body/60">{dt(s.periodStart)} – {dt(s.periodEnd)} · {s.orderCount} order(s)</p>
        </div>
        <div className="flex items-center gap-2">
          {s.belowMinimumThreshold && <Badge tone="secondary">Below minimum</Badge>}
          <Badge tone={STATUS_TONE[s.status] || 'neutral'}>{s.status}</Badge>
        </div>
      </button>

      {expanded && (
        <div className="mt-3 space-y-3 border-t border-border pt-3">
          <div className="grid grid-cols-2 gap-2 text-sm sm:grid-cols-4">
            <Field label="Gross revenue" value={money(s.grossRevenue)} />
            <Field label="Refund deductions" value={money(s.refundDeductions)} />
            <Field label="Platform fee" value={`${s.platformFeePercentageApplied}% = ${money(s.platformFeeAmount)}`} />
            <Field label="Net partnership revenue" value={money(s.netPartnershipRevenue)} />
            <Field label="Producer share" value={`${s.producerSharePercentageApplied}% = ${money(s.producerShareAmount)}`} />
            <Field label="Business Partner share" value={`${s.businessPartnerSharePercentageApplied}% = ${money(s.businessPartnerShareAmount)}`} />
            <Field label="Calculated" value={new Date(s.calculatedAt).toLocaleString()} />
            {s.approvedAt && <Field label="Approved by" value={`${s.approvedByName} · ${new Date(s.approvedAt).toLocaleString()}`} />}
          </div>
          {s.rejectionReason && <p className="text-xs text-body/60">Rejected: {s.rejectionReason}</p>}
          <p className="text-xs text-body/50">No payout integration exists — approval confirms the calculation only.</p>

          {s.status === 'Draft' && (
            <div className="flex gap-2">
              <Button variant="primary" disabled={busy} onClick={onSubmit}>Submit for approval</Button>
              <Button variant="secondary" disabled={busy} onClick={() => onReject(rejectReason || undefined)}>Reject</Button>
            </div>
          )}
          {s.status === 'PendingApproval' && (
            <div className="flex flex-wrap items-center gap-2">
              <Button variant="primary" disabled={busy} onClick={onApprove}>Approve</Button>
              <input aria-label="Rejection reason" placeholder="Rejection reason (optional)" value={rejectReason} onChange={(e) => setRejectReason(e.target.value)} className="flex-1 rounded-md border border-border bg-background px-2 py-1.5 text-sm" />
              <Button variant="secondary" disabled={busy} onClick={() => onReject(rejectReason || undefined)}>Reject</Button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

function Field({ label, value }) {
  return (
    <div>
      <p className="text-xs text-body/50">{label}</p>
      <p className="font-medium text-heading">{value}</p>
    </div>
  );
}
