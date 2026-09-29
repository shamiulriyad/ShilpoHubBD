import { useState } from 'react';
import { Badge, Button } from '../ui';
import MutationFeedback from '../ui/MutationFeedback';
import { usePartnershipAgreementMutations } from '../../hooks/useProducerPartnershipAgreements';
import { useSettlementsForAgreement } from '../../hooks/useProducerPartnershipSettlements';

const STATUS_TONE = {
  Pending: 'neutral', AwaitingProducerConfirmation: 'secondary', AwaitingBPConfirmation: 'secondary',
  Active: 'success', Suspended: 'secondary', Expired: 'neutral', Cancelled: 'neutral', Completed: 'primary',
};
const money = (value) => (value == null ? '—' : `৳${Number(value).toLocaleString('en-BD')}`);
const pct = (value) => (value == null ? '—' : `${value}%`);
const dt = (value) => (value ? new Date(value).toLocaleDateString() : '—');

// Shown to Admin, the Producer and the Business Partner alike — each sees the same facts, but only
// the actions relevant to their role and to the agreement's current stage in the workflow.
export default function PartnershipAgreementCard({ agreement, viewerRole }) {
  const m = usePartnershipAgreementMutations();
  const settlementsQuery = useSettlementsForAgreement(agreement.status === 'Active' || agreement.status === 'Suspended' || agreement.status === 'Completed' || agreement.status === 'Expired' ? agreement.id : null);
  const settlements = settlementsQuery.data || [];
  const [termsForm, setTermsForm] = useState({
    producerSharePercentage: agreement.producerSharePercentage ?? '',
    businessPartnerSharePercentage: agreement.businessPartnerSharePercentage ?? '',
    platformFeePercentage: agreement.platformFeePercentage ?? '',
    settlementFrequency: agreement.settlementFrequency ?? '',
    partnershipDurationMonths: agreement.partnershipDurationMonths ?? '',
  });
  const [cancelReason, setCancelReason] = useState('');

  const isAdmin = viewerRole === 'SuperAdmin';
  const isProducer = viewerRole === 'Producer';
  const isBusinessPartner = viewerRole === 'BusinessPartner';
  const canActOnLifecycle = isAdmin || isProducer || isBusinessPartner;

  const submitTerms = (e) => {
    e.preventDefault();
    m.updateTerms.mutate({
      id: agreement.id,
      payload: {
        producerSharePercentage: termsForm.producerSharePercentage === '' ? null : Number(termsForm.producerSharePercentage),
        businessPartnerSharePercentage: termsForm.businessPartnerSharePercentage === '' ? null : Number(termsForm.businessPartnerSharePercentage),
        platformFeePercentage: termsForm.platformFeePercentage === '' ? null : Number(termsForm.platformFeePercentage),
        settlementFrequency: termsForm.settlementFrequency || null,
        partnershipDurationMonths: termsForm.partnershipDurationMonths === '' ? null : Number(termsForm.partnershipDurationMonths),
      },
    });
  };

  return (
    <div className="space-y-4 rounded-xl border border-border bg-surface p-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-heading">{agreement.producerName} × {agreement.businessPartnerName}</p>
          <p className="text-xs text-body/60">{agreement.auctionName ? `From auction: ${agreement.auctionName}` : 'Manually created'}</p>
        </div>
        <Badge tone={STATUS_TONE[agreement.status] || 'neutral'}>{agreement.status}</Badge>
      </div>

      <div className="grid grid-cols-2 gap-3 text-sm sm:grid-cols-3">
        <Field label="Winning bid amount" value={money(agreement.winningBidAmount)} />
        <Field label="Producer share" value={pct(agreement.producerSharePercentage)} />
        <Field label="Business Partner share" value={pct(agreement.businessPartnerSharePercentage)} />
        <Field label="Platform fee" value={pct(agreement.platformFeePercentage)} />
        <Field label="Settlement frequency" value={agreement.settlementFrequency || '—'} />
        <Field label="Duration" value={agreement.partnershipDurationMonths ? `${agreement.partnershipDurationMonths} months` : '—'} />
        <Field label="Start date" value={dt(agreement.startDate)} />
        <Field label="End date" value={dt(agreement.endDate)} />
      </div>

      {agreement.agreementTerms && (
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-body/50">Responsibilities / terms</p>
          <p className="text-sm text-body/70">{agreement.agreementTerms}</p>
        </div>
      )}

      <MutationFeedback mutation={m.updateTerms} successMessage="Terms saved." />
      {isAdmin && agreement.status === 'Pending' && (
        <form onSubmit={submitTerms} className="space-y-3 border-t border-border pt-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-body/50">Set agreement terms</p>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
            <NumberField label="Producer %" value={termsForm.producerSharePercentage} onChange={(v) => setTermsForm((f) => ({ ...f, producerSharePercentage: v }))} />
            <NumberField label="BP %" value={termsForm.businessPartnerSharePercentage} onChange={(v) => setTermsForm((f) => ({ ...f, businessPartnerSharePercentage: v }))} />
            <NumberField label="Platform fee %" value={termsForm.platformFeePercentage} onChange={(v) => setTermsForm((f) => ({ ...f, platformFeePercentage: v }))} />
            <label className="block text-sm"><span className="mb-1 block text-xs text-body/60">Settlement frequency</span>
              <select value={termsForm.settlementFrequency} onChange={(e) => setTermsForm((f) => ({ ...f, settlementFrequency: e.target.value }))} className="w-full rounded-md border border-border bg-background px-2 py-1.5 text-sm">
                <option value="">Choose…</option>
                <option value="Monthly">Monthly</option>
                <option value="Quarterly">Quarterly</option>
                <option value="Biannual">Biannual</option>
                <option value="Custom">Custom</option>
              </select>
            </label>
            <NumberField label="Duration (months)" value={termsForm.partnershipDurationMonths} onChange={(v) => setTermsForm((f) => ({ ...f, partnershipDurationMonths: v }))} />
          </div>
          <div className="flex gap-2">
            <Button type="submit" variant="secondary" disabled={m.updateTerms.isPending}>Save terms</Button>
            <Button
              type="button" variant="primary" disabled={m.submitForConfirmation.isPending}
              onClick={() => m.submitForConfirmation.mutate(agreement.id)}
            >
              Submit for confirmation
            </Button>
          </div>
          <MutationFeedback mutation={m.submitForConfirmation} successMessage="Sent to the producer for confirmation." />
        </form>
      )}

      {agreement.status === 'AwaitingProducerConfirmation' && isProducer && (
        <div className="border-t border-border pt-4">
          <MutationFeedback mutation={m.confirm} successMessage="Confirmed — now awaiting the Business Partner." />
          <Button variant="primary" disabled={m.confirm.isPending} onClick={() => m.confirm.mutate(agreement.id)}>Confirm partnership</Button>
        </div>
      )}
      {agreement.status === 'AwaitingProducerConfirmation' && !isProducer && (
        <p className="border-t border-border pt-4 text-sm text-body/60">Waiting for the producer to confirm.</p>
      )}

      {agreement.status === 'AwaitingBPConfirmation' && isBusinessPartner && (
        <div className="border-t border-border pt-4">
          <MutationFeedback mutation={m.confirm} successMessage="Confirmed — partnership is now active." />
          <Button variant="primary" disabled={m.confirm.isPending} onClick={() => m.confirm.mutate(agreement.id)}>Confirm partnership</Button>
        </div>
      )}
      {agreement.status === 'AwaitingBPConfirmation' && !isBusinessPartner && (
        <p className="border-t border-border pt-4 text-sm text-body/60">Producer confirmed — waiting for the Business Partner.</p>
      )}

      {agreement.status === 'Active' && (
        <div className="flex flex-wrap gap-2 border-t border-border pt-4">
          <MutationFeedback mutation={m.suspend} successMessage="Partnership suspended." />
          <MutationFeedback mutation={m.complete} successMessage="Partnership completed." />
          {isAdmin && <Button variant="secondary" disabled={m.suspend.isPending} onClick={() => m.suspend.mutate({ id: agreement.id, reason: undefined })}>Suspend</Button>}
          {canActOnLifecycle && <Button variant="secondary" disabled={m.complete.isPending} onClick={() => m.complete.mutate(agreement.id)}>Mark completed</Button>}
        </div>
      )}

      {agreement.status === 'Suspended' && isAdmin && (
        <div className="border-t border-border pt-4">
          <MutationFeedback mutation={m.resume} successMessage="Partnership resumed." />
          <Button variant="primary" disabled={m.resume.isPending} onClick={() => m.resume.mutate(agreement.id)}>Resume</Button>
        </div>
      )}

      {canActOnLifecycle && !['Cancelled', 'Completed', 'Expired'].includes(agreement.status) && (
        <div className="flex items-center gap-2 border-t border-border pt-4">
          <MutationFeedback mutation={m.cancel} successMessage="Partnership cancelled." />
          <input
            aria-label="Cancellation reason"
            placeholder="Reason (optional)" value={cancelReason} onChange={(e) => setCancelReason(e.target.value)}
            className="flex-1 rounded-md border border-border bg-background px-2 py-1.5 text-sm"
          />
          <Button variant="secondary" disabled={m.cancel.isPending} onClick={() => m.cancel.mutate({ id: agreement.id, reason: cancelReason || undefined })}>Cancel</Button>
        </div>
      )}

      {settlements.length > 0 && (
        <div className="border-t border-border pt-4">
          <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Settlements</p>
          <p className="mb-2 text-xs text-body/50">No payout integration exists — these are calculations, not confirmation that money moved.</p>
          <ul className="space-y-1 text-xs text-body/70">
            {settlements.map((s) => (
              <li key={s.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2">
                <span>{new Date(s.periodStart).toLocaleDateString()} – {new Date(s.periodEnd).toLocaleDateString()}</span>
                <span>{isProducer ? money(s.producerShareAmount) : isBusinessPartner ? money(s.businessPartnerShareAmount) : money(s.netPartnershipRevenue)}</span>
                <Badge tone={s.status === 'Approved' ? 'success' : s.status === 'Rejected' ? 'neutral' : 'secondary'}>{s.status}</Badge>
              </li>
            ))}
          </ul>
        </div>
      )}

      {agreement.statusHistory?.length > 0 && (
        <div className="border-t border-border pt-4">
          <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">History</p>
          <ul className="space-y-1 text-xs text-body/60">
            {agreement.statusHistory.map((h, i) => (
              <li key={i}>{new Date(h.createdAt).toLocaleString()} — {h.status}{h.note ? `: ${h.note}` : ''}</li>
            ))}
          </ul>
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

function NumberField({ label, value, onChange }) {
  return (
    <label className="block text-sm">
      <span className="mb-1 block text-xs text-body/60">{label}</span>
      <input type="number" min="0" max="100" step="0.01" value={value} onChange={(e) => onChange(e.target.value)} className="w-full rounded-md border border-border bg-background px-2 py-1.5 text-sm" />
    </label>
  );
}
