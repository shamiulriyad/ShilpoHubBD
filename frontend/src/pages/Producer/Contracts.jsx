import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, StatusTimeline } from '../../components/ui';
import { useReceivedContracts, useContractMutations } from '../../hooks/useContracts';

const statusTone = { PendingApproval: 'secondary', Active: 'success', Rejected: 'neutral', Terminated: 'neutral', Expired: 'neutral' };

export default function Contracts() {
  const { data, isLoading, isError, error } = useReceivedContracts({ pageSize: 50 });
  const { accept, reject, terminate } = useContractMutations();
  const [expandedId, setExpandedId] = useState(null);

  const contracts = data?.items || [];

  return (
    <div>
      <PageHeader title="Contracts Received" description="Supply contracts from business partners." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {contracts.map((contract) => (
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
                <div className="mt-4 space-y-4 border-t border-border pt-4">
                  <p className="text-sm text-body/70">{contract.terms}</p>
                  <div className="divide-y divide-border rounded-lg border border-border">
                    {contract.items.map((item) => (
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
                      <Button variant="secondary" onClick={() => reject.mutate({ id: contract.id, notes: undefined })} disabled={reject.isPending}>
                        Reject
                      </Button>
                    </div>
                  )}
                  {contract.status === 'Active' && (
                    <Button variant="secondary" onClick={() => terminate.mutate(contract.id)} disabled={terminate.isPending}>
                      Terminate
                    </Button>
                  )}
                </div>
              )}
            </div>
          ))}
          {contracts.length === 0 && <p className="text-sm text-body/60">No contracts received yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
