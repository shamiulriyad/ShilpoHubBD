import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useMyInvestmentOpportunities,
  useInvestmentOpportunityProposals,
  useInvestmentOpportunityMutations,
} from '../../hooks/useInvestmentOpportunities';

const oppTone = { Open: 'secondary', FullyFunded: 'success', Closed: 'neutral', Cancelled: 'neutral' };
const proposalTone = { Submitted: 'secondary', Approved: 'success', Rejected: 'neutral', Active: 'primary', Completed: 'success', Cancelled: 'neutral' };

function ProposalsPanel({ opportunityId }) {
  const { data } = useInvestmentOpportunityProposals(opportunityId);
  const { decideProposal } = useInvestmentOpportunityMutations();

  return (
    <div className="space-y-2">
      {(data || []).map((p) => (
        <div key={p.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-background p-3 text-sm">
          <div>
            <p className="font-medium text-heading">{p.businessPartnerName}</p>
            <p className="text-xs text-body/60">৳ {p.investmentAmount.toLocaleString()}{p.proposalMessage ? ` · ${p.proposalMessage}` : ''}</p>
          </div>
          <div className="flex items-center gap-2">
            <Badge tone={proposalTone[p.status] || 'neutral'}>{p.status}</Badge>
            {p.status === 'Submitted' && (
              <>
                <Button variant="primary" onClick={() => decideProposal.mutate({ id: p.id, payload: { approve: true } })}>Approve</Button>
                <Button variant="secondary" onClick={() => decideProposal.mutate({ id: p.id, payload: { approve: false } })}>Reject</Button>
              </>
            )}
          </div>
        </div>
      ))}
      {(data || []).length === 0 && <p className="text-xs text-body/50">No proposals yet.</p>}
    </div>
  );
}

export default function InvestmentOpportunities() {
  const { data, isLoading, isError, error } = useMyInvestmentOpportunities();
  const { create, close, cancel } = useInvestmentOpportunityMutations();
  const [expandedId, setExpandedId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', projectDescription: '', fundingRequirement: '' });

  const opportunities = data || [];

  const handleCreate = (event) => {
    event.preventDefault();
    create.mutate(
      { title: form.title, projectDescription: form.projectDescription, fundingRequirement: Number(form.fundingRequirement) },
      { onSuccess: () => { setForm({ title: '', projectDescription: '', fundingRequirement: '' }); setShowForm(false); } },
    );
  };

  return (
    <div>
      <PageHeader
        title="Investment Opportunities"
        description="Raise investment from business partners for your growth projects."
        action={<Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'New Opportunity'}</Button>}
      />

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input required placeholder="Title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <textarea required rows={3} placeholder="Project description" value={form.projectDescription} onChange={(e) => setForm((p) => ({ ...p, projectDescription: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <input required type="number" placeholder="Funding requirement (৳)" value={form.fundingRequirement} onChange={(e) => setForm((p) => ({ ...p, fundingRequirement: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
          <Button type="submit" variant="primary" disabled={create.isPending}>Create</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {opportunities.map((opp) => (
            <div key={opp.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{opp.title}</p>
                  <p className="text-xs text-body/60">৳ {opp.fundingSecured.toLocaleString()} / ৳ {opp.fundingRequirement.toLocaleString()} · {opp.proposalCount} proposals</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={oppTone[opp.status] || 'neutral'}>{opp.status}</Badge>
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === opp.id ? null : opp.id)}>
                    {expandedId === opp.id ? 'Hide' : 'Proposals'}
                  </Button>
                </div>
              </div>
              {expandedId === opp.id && (
                <div className="mt-4 space-y-3 border-t border-border pt-4">
                  <ProposalsPanel opportunityId={opp.id} />
                  {opp.status === 'Open' && (
                    <div className="flex gap-2">
                      <Button variant="secondary" onClick={() => close.mutate(opp.id)}>Close</Button>
                      <Button variant="secondary" onClick={() => cancel.mutate(opp.id)}>Cancel</Button>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
          {opportunities.length === 0 && <p className="text-sm text-body/60">You haven't posted any investment opportunities yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
