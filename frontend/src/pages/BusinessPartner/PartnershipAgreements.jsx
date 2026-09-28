import { useState } from 'react';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import PartnershipAgreementCard from '../../components/business/PartnershipAgreementCard';
import { useMyPartnershipAgreements, usePartnershipAgreement } from '../../hooks/useProducerPartnershipAgreements';

const STATUS_TONE = {
  Pending: 'neutral', AwaitingProducerConfirmation: 'secondary', AwaitingBPConfirmation: 'secondary',
  Active: 'success', Suspended: 'secondary', Expired: 'neutral', Cancelled: 'neutral', Completed: 'primary',
};

export default function PartnershipAgreements() {
  const [selectedId, setSelectedId] = useState(null);
  const listQuery = useMyPartnershipAgreements({ pageSize: 50 });
  const detailQuery = usePartnershipAgreement(selectedId);
  const agreements = listQuery.data?.items || [];

  return (
    <div>
      <PageHeader title="Partnership Agreements" description="Producer partnerships you've won at auction — confirm the terms and manage the active partnership." />

      <AsyncState isLoading={listQuery.isLoading} isError={listQuery.isError} error={listQuery.error}>
        <div className="mb-8 space-y-2">
          {agreements.map((a) => (
            <button
              key={a.id}
              type="button"
              onClick={() => setSelectedId(a.id)}
              className={`block w-full rounded-xl border p-4 text-left transition ${selectedId === a.id ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'}`}
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="text-sm font-semibold text-heading">{a.producerName}</p>
                <Badge tone={STATUS_TONE[a.status] || 'neutral'}>{a.status}</Badge>
              </div>
              <p className="mt-1 text-xs text-body/60">Winning bid ৳{Number(a.winningBidAmount ?? 0).toLocaleString('en-BD')}</p>
            </button>
          ))}
          {agreements.length === 0 && <TravelEmptyState title="No partnerships yet" description="Win an auction lot to start a partnership with that producer." />}
        </div>
      </AsyncState>

      {selectedId && (
        <AsyncState isLoading={detailQuery.isLoading} isError={detailQuery.isError} error={detailQuery.error}>
          {detailQuery.data && <PartnershipAgreementCard agreement={detailQuery.data} viewerRole="BusinessPartner" />}
        </AsyncState>
      )}
    </div>
  );
}
