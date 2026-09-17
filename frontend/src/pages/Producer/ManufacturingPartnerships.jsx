import { useState } from 'react';
import { Pagination, PageHeader, Badge, Button, AsyncState, MilestoneList } from '../../components/ui';
import { usePartnership, useReceivedPartnerships, usePartnershipMutations } from '../../hooks/useManufacturingPartnerships';

import MutationFeedback from '../../components/ui/MutationFeedback';

const statusTone = { Requested: 'secondary', Accepted: 'primary', Rejected: 'neutral', InProgress: 'primary', Completed: 'success', Cancelled: 'neutral' };

export default function ManufacturingPartnerships() {
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, error } = useReceivedPartnerships({ page, pageSize: 10 });
  const { respond, updateMilestoneStatus } = usePartnershipMutations();
  const [expandedId, setExpandedId] = useState(null);
  const detailQuery = usePartnership(expandedId);

  const partnerships = data?.items || [];

  return (
    <div>
      <PageHeader title="Manufacturing Partnerships" description="Manufacturing requests from business partners." />
<div className="mb-4 space-y-2"><MutationFeedback mutation={respond} successMessage="Changes saved." /><MutationFeedback mutation={updateMilestoneStatus} successMessage="Changes saved." /></div>
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {partnerships.map((summary) => {
            const partnership = expandedId === summary.id && detailQuery.data ? detailQuery.data : summary;
            return (
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
                <AsyncState isLoading={detailQuery.isLoading} isError={detailQuery.isError} error={detailQuery.error}>
                <div className="mt-4 space-y-4 border-t border-border pt-4">
                  <p className="text-sm text-body/70">{partnership.productRequirements}</p>
                  <p className="text-sm text-body/70">{partnership.manufacturingSpecifications}</p>
                  <MilestoneList
                    milestones={partnership.milestones}
                    onAdvance={['Accepted','InProgress'].includes(partnership.status) && !updateMilestoneStatus.isPending ? (m) => updateMilestoneStatus.mutate({ id: partnership.id, milestoneId: m.id, status: 'Completed' }) : undefined}
                  />
                  {partnership.status === 'Requested' && (
                    <div className="flex gap-2">
                      <Button variant="primary" disabled={respond.isPending} onClick={() => respond.mutate({ id: partnership.id, payload: { accept: true } })}>
                        Accept
                      </Button>
                      <Button variant="secondary" disabled={respond.isPending} onClick={() => respond.mutate({ id: partnership.id, payload: { accept: false } })}>
                        Decline
                      </Button>
                    </div>
                  )}
                </div>
                </AsyncState>
              )}
            </div>
          ); })}
          {partnerships.length === 0 && <p className="text-sm text-body/60">No manufacturing partnership requests yet.</p>}
        </div>
        {data?.totalPages > 1 && <div className="mt-6"><Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} /></div>}
      </AsyncState>
    </div>
  );
}
