import { useEffect, useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import ProducerBusinessProfilePanel from '../../components/business/ProducerBusinessProfilePanel';
import {
  useProducerPartnershipAuctions, useProducerPartnershipAuction, useProducerPartnershipAuctionLots,
  useProducerPartnershipAuctionLot, useMyParticipantStatus, useMyAuctionBids, useProducerPartnershipParticipation,
} from '../../hooks/useProducerPartnershipAuctions';

const STATUS_TONE = { Draft: 'neutral', Scheduled: 'secondary', RegistrationOpen: 'secondary', Live: 'success', Ended: 'primary', Settled: 'primary', Cancelled: 'neutral' };
const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;
const dt = (value) => (value ? new Date(value).toLocaleString() : '—');

function useCountdown(target) {
  const [now, setNow] = useState(Date.now());
  useEffect(() => {
    if (!target) return undefined;
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, [target]);
  if (!target) return null;
  const diff = new Date(target).getTime() - now;
  if (diff <= 0) return 'Ended';
  const h = Math.floor(diff / 3600000);
  const m = Math.floor((diff % 3600000) / 60000);
  const s = Math.floor((diff % 60000) / 1000);
  return `${h}h ${m}m ${s}s`;
}

export default function PartnershipAuctions() {
  const [selectedId, setSelectedId] = useState(null);
  const listQuery = useProducerPartnershipAuctions({ pageSize: 50 });
  const auctions = listQuery.data?.items || [];

  return (
    <div>
      <PageHeader title="Partnership Auctions" description="Apply to participate, bid on producer lots, and track your active bids." />

      <AsyncState isLoading={listQuery.isLoading} isError={listQuery.isError} error={listQuery.error}>
        <div className="mb-8 grid gap-3 sm:grid-cols-2">
          {auctions.map((a) => (
            <button
              key={a.id}
              type="button"
              onClick={() => setSelectedId(a.id)}
              className={`rounded-xl border p-4 text-left transition ${selectedId === a.id ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'}`}
            >
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold text-heading">{a.name}</p>
                <Badge tone={STATUS_TONE[a.status] || 'neutral'}>{a.status}</Badge>
              </div>
              <p className="mt-1 text-xs text-body/60">{a.auctionYear} · {a.lotCount} producer lot(s)</p>
            </button>
          ))}
          {auctions.length === 0 && <TravelEmptyState title="No auctions yet" description="Check back once the admin schedules the next Producer Partnership Auction." />}
        </div>
      </AsyncState>

      {selectedId && <AuctionParticipation auctionId={selectedId} />}
    </div>
  );
}

function AuctionParticipation({ auctionId }) {
  const auctionQuery = useProducerPartnershipAuction(auctionId);
  const participantQuery = useMyParticipantStatus(auctionId);
  const { apply } = useProducerPartnershipParticipation(auctionId);
  const auction = auctionQuery.data;
  const participant = participantQuery.data;
  const countdown = useCountdown(auction?.status === 'Live' ? auction.biddingClosesAt : null);

  return (
    <div className="space-y-6 border-t border-border pt-6">
      <AsyncState isLoading={auctionQuery.isLoading} isError={auctionQuery.isError} error={auctionQuery.error}>
        {auction && (
          <>
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p className="text-lg font-semibold text-heading">{auction.name}</p>
                <p className="text-sm text-body/70">{auction.description}</p>
              </div>
              <div className="text-right">
                <Badge tone={STATUS_TONE[auction.status] || 'neutral'}>{auction.status}</Badge>
                {countdown && <p className="mt-1 text-xs text-body/60">Bidding ends in {countdown}</p>}
              </div>
            </div>

            {auction.status === 'RegistrationOpen' && !participant && (
              <div className="rounded-xl border border-border bg-surface p-4">
                <p className="mb-2 text-sm text-body/70">Registration is open. Apply to be considered eligible to bid once the auction goes live.</p>
                <MutationFeedback mutation={apply} successMessage="Application submitted — waiting for admin approval." />
                <Button variant="primary" disabled={apply.isPending} onClick={() => apply.mutate()}>Apply to participate</Button>
              </div>
            )}

            {participant && (
              <p className="text-sm text-body/70">
                Your participation status: <Badge tone={participant.status === 'Approved' ? 'success' : participant.status === 'Rejected' ? 'neutral' : 'secondary'}>{participant.status}</Badge>
                {participant.status === 'Rejected' && participant.decisionNotes && <span className="ml-2 text-xs text-body/50">({participant.decisionNotes})</span>}
              </p>
            )}

            {participant?.status === 'Approved' && (auction.status === 'Live' || auction.status === 'Ended') && (
              <LotsAndBidding auctionId={auctionId} auctionEnded={auction.status === 'Ended'} />
            )}
          </>
        )}
      </AsyncState>
    </div>
  );
}

function LotsAndBidding({ auctionId, auctionEnded }) {
  const lotsQuery = useProducerPartnershipAuctionLots(auctionId);
  const myBidsQuery = useMyAuctionBids(auctionId);
  const [selectedLotId, setSelectedLotId] = useState(null);
  const lots = lotsQuery.data || [];
  const myBids = myBidsQuery.data || [];

  return (
    <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
      <div>
        <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-body/50">Producer lots</h3>
        <AsyncState isLoading={lotsQuery.isLoading} isError={lotsQuery.isError} error={lotsQuery.error}>
          <div className="space-y-2">
            {lots.map((lot) => (
              <button
                key={lot.id}
                type="button"
                onClick={() => setSelectedLotId(lot.id)}
                className={`block w-full rounded-xl border p-4 text-left transition ${selectedLotId === lot.id ? 'border-primary bg-primary/5' : 'border-border bg-surface hover:shadow-md'}`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="text-sm font-semibold text-heading">{lot.producerName}</p>
                  <Badge tone={lot.status === 'Awarded' ? 'success' : lot.status === 'Unsold' ? 'neutral' : 'secondary'}>{lot.status}</Badge>
                </div>
                <p className="mt-1 text-xs text-body/60">
                  Current highest {lot.currentHighestBid != null ? money(lot.currentHighestBid) : `starts at ${money(lot.startingBid)}`} · {lot.bidCount} bid(s)
                  {lot.isCurrentUserHighestBidder && lot.status === 'Open' && <span className="ml-1 font-medium text-success">— you're winning</span>}
                  {lot.status === 'Awarded' && (lot.winnerId ? (lot.winnerName ? `— won by ${lot.winnerName}` : '— you won this lot') : '')}
                </p>
              </button>
            ))}
            {lots.length === 0 && <p className="text-sm text-body/60">No producer lots in this auction.</p>}
          </div>
        </AsyncState>

        {myBids.length > 0 && (
          <div className="mt-6">
            <h3 className="mb-2 text-sm font-semibold uppercase tracking-wide text-body/50">Your bids</h3>
            <ul className="space-y-1 text-sm text-body/70">
              {myBids.map((b) => (
                <li key={b.id} className="flex justify-between rounded-lg border border-border bg-surface px-3 py-2">
                  <span>{b.producerName}</span>
                  <span>{money(b.amount)} {b.isCurrentHighest ? <Badge tone="success">Highest</Badge> : null}</span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      <div>{selectedLotId && <LotDetail auctionId={auctionId} lotId={selectedLotId} auctionEnded={auctionEnded} />}</div>
    </div>
  );
}

function LotDetail({ auctionId, lotId, auctionEnded }) {
  const lotQuery = useProducerPartnershipAuctionLot(auctionId, lotId);
  const { placeBid } = useProducerPartnershipParticipation(auctionId);
  const [amount, setAmount] = useState('');
  const lot = lotQuery.data;

  if (lotQuery.isLoading) return <p className="text-sm text-body/60">Loading lot…</p>;
  if (!lot) return null;

  const handleBid = (e) => {
    e.preventDefault();
    placeBid.mutate({ lotId, amount: Number(amount) }, { onSuccess: () => setAmount('') });
  };

  return (
    <div className="space-y-4">
      <ProducerBusinessProfilePanel profile={lot.producerProfile} />
      <div className="rounded-xl border border-border bg-surface p-4">
        <p className="text-xs text-body/50">Current highest bid</p>
        <p className="text-xl font-semibold text-heading">{lot.currentHighestBid != null ? money(lot.currentHighestBid) : money(lot.startingBid)}</p>
        <p className="text-xs text-body/60">{lot.bidCount} bid(s) · minimum next bid {money(lot.minimumNextBid)}</p>
        {!auctionEnded && lot.status === 'Open' && (
          <form onSubmit={handleBid} className="mt-3 flex items-center gap-2">
            <MutationFeedback mutation={placeBid} />
            <input
              type="number" min={lot.minimumNextBid} step="0.01" required
              placeholder={`≥ ${lot.minimumNextBid}`} value={amount} onChange={(e) => setAmount(e.target.value)}
              className="w-32 rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
            <Button type="submit" variant="primary" disabled={placeBid.isPending}>{placeBid.isPending ? 'Placing…' : 'Place bid'}</Button>
          </form>
        )}
        {lot.status === 'Awarded' && (
          <p className="mt-2 text-sm">
            {lot.winnerId ? (lot.winnerName ? `Won by ${lot.winnerName} at ${money(lot.winningAmount)}.` : `You won this lot at ${money(lot.winningAmount)}!`) : 'Lot awarded.'}
          </p>
        )}
        {lot.status === 'Unsold' && <p className="mt-2 text-sm text-body/60">This lot received no eligible bids.</p>}
      </div>
    </div>
  );
}
