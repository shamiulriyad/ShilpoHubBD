import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { ProducerSelect } from '../../components/forms/EntityPickers';
import {
  useProducerPartnershipAuctions, useProducerPartnershipAuction, useProducerPartnershipAuctionLots,
  useProducerPartnershipAuctionParticipants, useBidHistory, useProducerPartnershipAuctionMutations,
} from '../../hooks/useProducerPartnershipAuctions';

const STATUS_TONE = { Draft: 'neutral', Scheduled: 'secondary', RegistrationOpen: 'secondary', Live: 'success', Ended: 'primary', Settled: 'primary', Cancelled: 'neutral' };
const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;
const dt = (value) => (value ? new Date(value).toLocaleString() : '—');

const NEXT_ACTION = {
  Draft: { label: 'Schedule', key: 'schedule' },
  Scheduled: { label: 'Open registration', key: 'openRegistration' },
  RegistrationOpen: { label: 'Go live', key: 'goLive' },
  Live: { label: 'End auction', key: 'end' },
};

export default function ProducerPartnershipAuctions() {
  const [selectedId, setSelectedId] = useState(null);
  const [showCreate, setShowCreate] = useState(false);
  const listQuery = useProducerPartnershipAuctions({ pageSize: 50 });
  const auctions = listQuery.data?.items || [];

  return (
    <div>
      <PageHeader
        title="Producer Partnership Auctions"
        description="Configure the annual auction, enter producers as lots, approve Business Partner participants and run the auction through its lifecycle."
        action={<Button variant="primary" onClick={() => setShowCreate((s) => !s)}>{showCreate ? 'Close' : 'New auction'}</Button>}
      />

      {showCreate && <CreateAuctionForm onCreated={(id) => { setShowCreate(false); setSelectedId(id); }} />}

      <AsyncState isLoading={listQuery.isLoading} isError={listQuery.isError} error={listQuery.error}>
        <div className="mb-8 space-y-2">
          {auctions.map((a) => (
            <button
              key={a.id}
              type="button"
              onClick={() => setSelectedId(a.id)}
              className={`block w-full rounded-xl border p-4 text-left transition ${selectedId === a.id ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'}`}
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <p className="text-sm font-semibold text-heading">{a.name} <span className="font-normal text-body/60">({a.auctionYear})</span></p>
                <Badge tone={STATUS_TONE[a.status] || 'neutral'}>{a.status}</Badge>
              </div>
              <p className="mt-1 text-xs text-body/60">{a.lotCount} lot(s) · bidding {dt(a.biddingOpensAt)} → {dt(a.biddingClosesAt)}</p>
            </button>
          ))}
          {auctions.length === 0 && <TravelEmptyState title="No auctions yet" description="Create the first Producer Partnership Auction to get started." />}
        </div>
      </AsyncState>

      {selectedId && <AuctionDetail auctionId={selectedId} />}
    </div>
  );
}

function CreateAuctionForm({ onCreated }) {
  const { create } = useProducerPartnershipAuctionMutations();
  const [form, setForm] = useState({
    name: '', auctionYear: new Date().getFullYear(), description: '',
    minimumStartingBid: 1000, minimumBidIncrement: 100, maxProducersPerBusinessPartner: '',
    biddingOpensAt: '', biddingClosesAt: '', registrationOpensAt: '', registrationClosesAt: '',
  });
  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const handleSubmit = (e) => {
    e.preventDefault();
    create.mutate({
      name: form.name,
      auctionYear: Number(form.auctionYear),
      description: form.description,
      minimumStartingBid: Number(form.minimumStartingBid),
      minimumBidIncrement: Number(form.minimumBidIncrement),
      maxProducersPerBusinessPartner: form.maxProducersPerBusinessPartner ? Number(form.maxProducersPerBusinessPartner) : null,
      biddingOpensAt: form.biddingOpensAt || null,
      biddingClosesAt: form.biddingClosesAt || null,
      registrationOpensAt: form.registrationOpensAt || null,
      registrationClosesAt: form.registrationClosesAt || null,
    }, { onSuccess: (result) => onCreated(result.id) });
  };

  return (
    <form onSubmit={handleSubmit} className="mb-8 space-y-4 rounded-xl border border-border bg-surface p-5">
      <MutationFeedback mutation={create} successMessage="Auction created as a Draft." />
      <div className="grid gap-4 sm:grid-cols-2">
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Name</span>
          <input required value={form.name} onChange={set('name')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Auction year</span>
          <input required type="number" value={form.auctionYear} onChange={set('auctionYear')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
      </div>
      <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Description</span>
        <textarea required value={form.description} onChange={set('description')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" rows={2} /></label>
      <div className="grid gap-4 sm:grid-cols-3">
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Minimum starting bid</span>
          <input required type="number" min="0" value={form.minimumStartingBid} onChange={set('minimumStartingBid')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Minimum bid increment</span>
          <input required type="number" min="1" value={form.minimumBidIncrement} onChange={set('minimumBidIncrement')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Max producers a BP can win</span>
          <input type="number" min="1" placeholder="Unlimited" value={form.maxProducersPerBusinessPartner} onChange={set('maxProducersPerBusinessPartner')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Registration opens</span>
          <input type="datetime-local" value={form.registrationOpensAt} onChange={set('registrationOpensAt')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Registration closes</span>
          <input type="datetime-local" value={form.registrationClosesAt} onChange={set('registrationClosesAt')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Bidding starts</span>
          <input type="datetime-local" value={form.biddingOpensAt} onChange={set('biddingOpensAt')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
        <label className="block text-sm"><span className="mb-1 block font-medium text-heading">Bidding ends</span>
          <input type="datetime-local" value={form.biddingClosesAt} onChange={set('biddingClosesAt')} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" /></label>
      </div>
      <Button type="submit" variant="primary" disabled={create.isPending}>{create.isPending ? 'Creating…' : 'Create auction'}</Button>
    </form>
  );
}

function AuctionDetail({ auctionId }) {
  const auctionQuery = useProducerPartnershipAuction(auctionId);
  const lotsQuery = useProducerPartnershipAuctionLots(auctionId);
  const participantsQuery = useProducerPartnershipAuctionParticipants(auctionId);
  const { schedule, openRegistration, goLive, end, cancel, addLot, removeLot, decideParticipant } = useProducerPartnershipAuctionMutations(auctionId);
  const [newProducerId, setNewProducerId] = useState('');
  const [expandedLotId, setExpandedLotId] = useState(null);

  const auction = auctionQuery.data;
  const lots = lotsQuery.data || [];
  const participants = participantsQuery.data || [];
  const nextAction = auction && NEXT_ACTION[auction.status];
  const mutationByKey = { schedule, openRegistration, goLive, end };

  return (
    <div className="space-y-6 border-t border-border pt-6">
      <AsyncState isLoading={auctionQuery.isLoading} isError={auctionQuery.isError} error={auctionQuery.error}>
        {auction && (
          <>
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p className="text-lg font-semibold text-heading">{auction.name}</p>
                <p className="text-xs text-body/60">Status: <Badge tone={STATUS_TONE[auction.status] || 'neutral'}>{auction.status}</Badge></p>
              </div>
              <div className="flex flex-wrap gap-2">
                {nextAction && (
                  <Button variant="primary" disabled={mutationByKey[nextAction.key].isPending} onClick={() => mutationByKey[nextAction.key].mutate(auctionId)}>
                    {nextAction.label}
                  </Button>
                )}
                {auction.status !== 'Ended' && auction.status !== 'Cancelled' && auction.status !== 'Settled' && (
                  <Button variant="secondary" disabled={cancel.isPending} onClick={() => cancel.mutate(auctionId)}>Cancel auction</Button>
                )}
              </div>
            </div>
            <MutationFeedback mutation={schedule} successMessage="Auction scheduled." />
            <MutationFeedback mutation={openRegistration} successMessage="Registration opened." />
            <MutationFeedback mutation={goLive} successMessage="Auction is now live." />
            <MutationFeedback mutation={end} successMessage="Auction ended and winners assigned." />
            <MutationFeedback mutation={cancel} successMessage="Auction cancelled." />

            <section>
              <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-body/50">Producer lots</h3>
              {(auction.status === 'Draft' || auction.status === 'Scheduled' || auction.status === 'RegistrationOpen') && (
                <div className="mb-3 flex flex-wrap items-end gap-2">
                  <div className="w-64"><ProducerSelect id="add-lot-producer" label="Add a producer" value={newProducerId} onChange={setNewProducerId} /></div>
                  <Button
                    variant="primary"
                    disabled={!newProducerId || addLot.isPending}
                    onClick={() => addLot.mutate(newProducerId, { onSuccess: () => setNewProducerId('') })}
                  >
                    Add lot
                  </Button>
                </div>
              )}
              <MutationFeedback mutation={addLot} />
              <MutationFeedback mutation={removeLot} successMessage="Lot removed." />
              <div className="space-y-2">
                {lots.map((lot) => (
                  <div key={lot.id} className="rounded-lg border border-border bg-surface p-3">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <div>
                        <p className="text-sm font-medium text-heading">{lot.producerName}</p>
                        <p className="text-xs text-body/60">
                          Starting {money(lot.startingBid)} · Highest {lot.currentHighestBid != null ? money(lot.currentHighestBid) : '—'} ({lot.bidCount} bid{lot.bidCount === 1 ? '' : 's'})
                          {lot.status === 'Awarded' && lot.winnerName && <> · Won by {lot.winnerName} at {money(lot.winningAmount)}</>}
                        </p>
                      </div>
                      <div className="flex items-center gap-2">
                        <Badge tone={lot.status === 'Awarded' ? 'success' : lot.status === 'Unsold' ? 'neutral' : 'secondary'}>{lot.status}</Badge>
                        {lot.status === 'Pending' && (
                          <Button variant="secondary" disabled={removeLot.isPending} onClick={() => removeLot.mutate(lot.id)}>Remove</Button>
                        )}
                        {lot.bidCount > 0 && (
                          <Button variant="secondary" onClick={() => setExpandedLotId(expandedLotId === lot.id ? null : lot.id)}>
                            {expandedLotId === lot.id ? 'Hide bids' : 'View bids'}
                          </Button>
                        )}
                      </div>
                    </div>
                    {expandedLotId === lot.id && <BidHistory auctionId={auctionId} lotId={lot.id} />}
                  </div>
                ))}
                {lots.length === 0 && <p className="text-sm text-body/60">No producers entered yet.</p>}
              </div>
            </section>

            <section>
              <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-body/50">Business Partner participants</h3>
              <MutationFeedback mutation={decideParticipant} successMessage="Decision recorded." />
              <div className="space-y-2">
                {participants.map((p) => (
                  <div key={p.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-surface p-3">
                    <div>
                      <p className="text-sm font-medium text-heading">{p.businessPartnerName}</p>
                      <p className="text-xs text-body/60">Applied {dt(p.appliedAt)}{p.decidedAt && ` · Decided ${dt(p.decidedAt)}`}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge tone={p.status === 'Approved' ? 'success' : p.status === 'Rejected' ? 'neutral' : 'secondary'}>{p.status}</Badge>
                      {p.status === 'Applied' && (
                        <>
                          <Button variant="primary" disabled={decideParticipant.isPending} onClick={() => decideParticipant.mutate({ participantId: p.id, approve: true })}>Approve</Button>
                          <Button variant="secondary" disabled={decideParticipant.isPending} onClick={() => decideParticipant.mutate({ participantId: p.id, approve: false })}>Reject</Button>
                        </>
                      )}
                    </div>
                  </div>
                ))}
                {participants.length === 0 && <p className="text-sm text-body/60">No Business Partner has applied yet.</p>}
              </div>
            </section>
          </>
        )}
      </AsyncState>
    </div>
  );
}

function BidHistory({ auctionId, lotId }) {
  const { data, isLoading } = useBidHistory(auctionId, lotId);
  if (isLoading) return <p className="mt-2 text-xs text-body/60">Loading bid history…</p>;
  return (
    <ul className="mt-2 space-y-1 border-t border-border pt-2 text-xs text-body/70">
      {(data || []).map((b) => (
        <li key={b.id} className="flex justify-between"><span>{b.businessPartnerName}</span><span>{money(b.amount)} · {dt(b.placedAt)}</span></li>
      ))}
    </ul>
  );
}
