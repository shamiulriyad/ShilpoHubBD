import { useState } from 'react';
import { Pagination, PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useContract, useReceivedContracts, useContractMutations } from '../../hooks/useContracts';

import MutationFeedback from '../../components/ui/MutationFeedback';

import { confirmAction } from '../../lib/confirm';
const statusTone = { PendingApproval: 'secondary', Active: 'success', Rejected: 'neutral', Terminated: 'neutral', Expired: 'neutral' };

export default function Contracts() {
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useReceivedContracts({ page, pageSize: 10 });
  const { accept, reject, terminate } = useContractMutations();
  const [expandedId, setExpandedId] = useState(null);
  const detailQuery = useContract(expandedId);

  const contracts = data?.items || [];

  return (
    <div>
      <PageHeader title="Contracts Received" description="Supply contracts from business partners." />
<div className="mb-4 space-y-2"><MutationFeedback mutation={accept} successMessage="Changes saved." /><MutationFeedback mutation={reject} successMessage="Changes saved." /><MutationFeedback mutation={terminate} successMessage="Changes saved." /></div>
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {contracts.map((summary) => {
            const contract = expandedId === summary.id && detailQuery.data ? detailQuery.data : summary;
            return (
            <div key={contract.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{contract.title}</p>
                  <p className="text-xs text-body/60">
                    {contract.referenceNumber} · ৳ {contract.contractValue.toLocaleString()}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[contract.status] || 'neutral'}>{contract.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === contract.id ? null : contract.id)}>
                    {expandedId === contract.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>

              {expandedId === contract.id && (
                <AsyncState isLoading={detailQuery.isLoading} isError={detailQuery.isError} error={detailQuery.error}>
                <div className="mt-4 space-y-4 border-t border-border pt-4">
                  <p className="text-sm text-body/70">{contract.terms}</p>
                  <div className="divide-y divide-border rounded-lg border border-border">
                    {(contract.items || []).map((item) => (
                      <div key={item.id} className="flex items-center justify-between p-3 text-sm">
                        <span>{item.productName} × {item.quantity}</span>
                        <span className="font-medium">৳ {item.lineTotal.toLocaleString()}</span>
                      </div>
                    ))}
                  </div>
                  <StatusTimeline events={contract.statusHistory} />
                  {contract.status === 'PendingApproval' && (
                    <div className="flex gap-2">
                      <Button variant="primary" onClick={() => accept.mutate(contract.id)} disabled={accept.isPending}>
                        Accept
                      </Button>
                      <Button variant="secondary" onClick={async () => { if (await confirmAction('Reject this? The other person will be told.', { confirmLabel: 'Yes, reject' })) reject.mutate({ id: contract.id, notes: undefined }); }} disabled={reject.isPending}>
                        Reject
                      </Button>
                    </div>
                  )}
                  {contract.status === 'Active' && (
                    <Button variant="secondary" onClick={async () => { if (await confirmAction('Terminate this? This ends it for both sides.', { confirmLabel: 'Yes, terminate' })) terminate.mutate(contract.id); }} disabled={terminate.isPending}>
                      Terminate
                    </Button>
                  )}
                </div>
                </AsyncState>
              )}
            </div>
          ); })}
          {contracts.length === 0 && <p className="text-sm text-body/60">No contracts received yet.</p>}
        </div>
        {data?.totalPages > 1 && <div className="mt-6"><Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} /></div>}
      </AsyncState>
    </div>
  );
}
