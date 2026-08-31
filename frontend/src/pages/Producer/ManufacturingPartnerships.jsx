import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, MilestoneList } from '../../components/ui';
import { useReceivedPartnerships, usePartnershipMutations } from '../../hooks/useManufacturingPartnerships';

const statusTone = { Requested: 'secondary', Accepted: 'primary', Rejected: 'neutral', InProgress: 'primary', Completed: 'success', Cancelled: 'neutral' };

export default function ManufacturingPartnerships() {
  const { data, isLoading, isError, error } = useReceivedPartnerships({ pageSize: 50 });
  const { respond, updateMilestoneStatus } = usePartnershipMutations();
  const [expandedId, setExpandedId] = useState(null);

  const partnerships = data?.items || [];

  return (
    <div>
      <PageHeader title="Manufacturing Partnerships" description="Manufacturing requests from business partners." />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {partnerships.map((partnership) => (
            <div key={partnership.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{partnership.title}</p>
                  <p className="text-xs text-body/60">Qty {partnership.quantity} · {partnership.progressPercentage}% complete</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={statusTone[partnership.status] || 'neutral'}>{partnership.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === partnership.id ? null : partnership.id)}>
                    {expandedId === partnership.id ? 'Hide' : 'Details'}
                  </Button>
                </div>
              </div>

              {expandedId === partnership.id && (
                <div className="mt-4 space-y-4 border-t border-border pt-4">
                  <p className="text-sm text-body/70">{partnership.productRequirements}</p>
                  <p className="text-sm text-body/70">{partnership.manufacturingSpecifications}</p>
                  <MilestoneList
                    milestones={partnership.milestones}
                    onAdvance={(m) => updateMilestoneStatus.mutate({ id: partnership.id, milestoneId: m.id, status: 'Completed' })}
                  />
                  {partnership.status === 'Requested' && (
                    <div className="flex gap-2">
                      <Button variant="primary" onClick={() => respond.mutate({ id: partnership.id, payload: { accept: true } })}>
                        Accept
                      </Button>
                      <Button variant="secondary" onClick={() => respond.mutate({ id: partnership.id, payload: { accept: false } })}>
                        Decline
                      </Button>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
          {partnerships.length === 0 && <p className="text-sm text-body/60">No manufacturing partnership requests yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
