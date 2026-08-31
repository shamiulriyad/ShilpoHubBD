import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState, SectionHeader } from '../../components/ui';
import { useInvestmentOpportunities, useMyInvestmentProposals, useInvestmentOpportunityMutations } from '../../hooks/useInvestmentOpportunities';

const oppTone = { Open: 'secondary', FullyFunded: 'success', Closed: 'neutral', Cancelled: 'neutral' };
const proposalTone = { Submitted: 'secondary', Approved: 'success', Rejected: 'neutral', Active: 'primary', Completed: 'success', Cancelled: 'neutral' };

export default function InvestmentMarketplace() {
  const opportunitiesQuery = useInvestmentOpportunities({ status: 'Open', pageSize: 20 });
  const proposalsQuery = useMyInvestmentProposals();
  const { submitProposal } = useInvestmentOpportunityMutations();
  const [form, setForm] = useState({});
  const [proposingId, setProposingId] = useState(null);

  const opportunities = opportunitiesQuery.data?.items || [];

  const handleSubmit = (id) => {
    submitProposal.mutate(
      { id, payload: { investmentAmount: Number(form.investmentAmount), proposalMessage: form.proposalMessage } },
      { onSuccess: () => setProposingId(null) },
    );
  };

  return (
    <div>
      <PageHeader title="Investment Marketplace" description="Invest in growth projects posted by heritage producers." />

      <AsyncState isLoading={opportunitiesQuery.isLoading} isError={opportunitiesQuery.isError} error={opportunitiesQuery.error}>
        <div className="grid gap-4 sm:grid-cols-2">
          {opportunities.map((opp) => (
            <div key={opp.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold text-heading">{opp.title}</p>
                <Badge tone={oppTone[opp.status] || 'neutral'}>{opp.status}</Badge>
              </div>
              <p className="mt-1 text-xs text-body/60">By {opp.producerName}</p>
              <p className="mt-2 text-sm text-body/70">{opp.projectDescription}</p>
              <p className="mt-2 text-sm font-semibold text-primary">৳ {opp.fundingSecured.toLocaleString()} / ৳ {opp.fundingRequirement.toLocaleString()}</p>

              {proposingId === opp.id ? (
                <div className="mt-3 space-y-2">
                  <input type="number" placeholder="Investment amount (৳)" onChange={(e) => setForm((p) => ({ ...p, investmentAmount: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
                  <textarea rows={2} placeholder="Message (optional)" onChange={(e) => setForm((p) => ({ ...p, proposalMessage: e.target.value }))} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
                  <Button variant="primary" onClick={() => handleSubmit(opp.id)} disabled={submitProposal.isPending}>Submit Proposal</Button>
                </div>
              ) : (
                <Button variant="secondary" className="mt-3" onClick={() => setProposingId(opp.id)}>Propose Investment</Button>
              )}
            </div>
          ))}
          {opportunities.length === 0 && <p className="col-span-full text-sm text-body/60">No open investment opportunities right now.</p>}
        </div>
      </AsyncState>

      <SectionHeader eyebrow="Mine" title="My Proposals" />
      <AsyncState isLoading={proposalsQuery.isLoading} isError={proposalsQuery.isError} error={proposalsQuery.error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {(proposalsQuery.data || []).map((p) => (
            <div key={p.id} className="flex items-center justify-between p-3 text-sm">
              <span>{p.opportunityTitle}</span>
              <div className="flex items-center gap-3">
                <span className="text-body/60">৳ {p.investmentAmount.toLocaleString()}</span>
                <Badge tone={proposalTone[p.status] || 'neutral'}>{p.status}</Badge>
              </div>
            </div>
          ))}
          {(proposalsQuery.data || []).length === 0 && <p className="p-3 text-sm text-body/60">No proposals submitted yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
