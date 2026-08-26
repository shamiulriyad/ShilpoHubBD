import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, SectionHeader, BidForm, AsyncState } from '../../components/ui';
import { useAuction, useAuctions, usePlaceBid } from '../../hooks/useAuctions';

function formatTimeRemaining(seconds) {
  if (seconds <= 0) return 'Closed';
  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);
  return `${days}d ${hours}h`;
}

export default function AuctionDetails() {
  const { auctionId } = useParams();
  const auctionQuery = useAuction(auctionId);
  const relatedQuery = useAuctions({ status: 'Active', pageSize: 4 });
  const placeBid = usePlaceBid(auctionId);
  const auction = auctionQuery.data;
  const related = (relatedQuery.data?.items || []).filter((a) => a.id !== auctionId).slice(0, 3);

  const bidError = placeBid.error?.response?.data?.title || placeBid.error?.message;

  return (
    <div>
      <AsyncState isLoading={auctionQuery.isLoading} isError={auctionQuery.isError} error={auctionQuery.error}>
        {auction && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Dashboard', path: routePaths.customer },
                { label: 'Auctions', path: routePaths.customerAuctions },
                { label: auction.title },
              ]}
              title={auction.title}
            />

            <div className="grid gap-10 lg:grid-cols-2">
              <div className="flex aspect-square items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
                {auction.productImageUrl ? (
                  <img src={auction.productImageUrl} alt={auction.title} className="h-full w-full rounded-2xl object-cover" />
                ) : (
                  'Item Image'
                )}
              </div>

              <div>
                <Badge tone="primary">{auction.status}</Badge>
                <p className="mt-3 text-sm text-body/70">{auction.description}</p>

                <Link
                  to={routePaths.customerProducerProfile.replace(':producerId', auction.producerId)}
                  className="mt-4 block text-sm text-link hover:underline"
                >
                  Offered by {auction.producerName}
                </Link>

                <div className="mt-6 space-y-3 rounded-xl border border-border bg-surface p-5">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-body/70">Current Bid</span>
                    <span className="text-2xl font-semibold text-primary">৳ {auction.currentPrice.toLocaleString()}</span>
                  </div>
                  <div className="flex items-center justify-between text-sm text-body/70">
                    <span>{auction.bidCount} bids</span>
                    <span>Closes in {formatTimeRemaining(auction.timeRemainingSeconds)}</span>
                  </div>
                  {auction.status === 'Active' ? (
                    <>
                      <BidForm
                        currentBid={auction.currentPrice}
                        step={auction.minBidIncrement}
                        onSubmit={(amount) => placeBid.mutate(amount)}
                      />
                      {bidError && <p className="text-sm text-red-600">{bidError}</p>}
                    </>
                  ) : (
                    <p className="text-sm text-body/60">
                      This auction has ended{auction.winnerName ? ` — won by ${auction.winnerName}` : ''}.
                    </p>
                  )}
                </div>
              </div>
            </div>

            <div className="mt-10">
              <SectionHeader eyebrow="Activity" title="Bid History" />
              <div className="divide-y divide-border rounded-xl border border-border bg-surface">
                {auction.bids.map((bid) => (
                  <div key={bid.id} className="flex items-center justify-between p-4 text-sm">
                    <span className="font-medium text-heading">{bid.bidderName}</span>
                    <span className="text-body/60">{new Date(bid.createdAt).toLocaleString()}</span>
                    <span className="font-semibold text-primary">৳ {bid.amount.toLocaleString()}</span>
                  </div>
                ))}
                {auction.bids.length === 0 && (
                  <p className="p-4 text-center text-sm text-body/60">No bids yet — be the first.</p>
                )}
              </div>
            </div>

            {related.length > 0 && (
              <div className="mt-12">
                <SectionHeader eyebrow="More" title="Other Auctions" />
                <div className="grid gap-4 sm:grid-cols-3">
                  {related.map((item) => (
                    <Link
                      key={item.id}
                      to={routePaths.customerAuctionDetails.replace(':auctionId', item.id)}
                      className="rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
                    >
                      <p className="text-sm font-semibold text-heading">{item.title}</p>
                      <p className="mt-1 text-xs text-body/60">Closes in {formatTimeRemaining(item.timeRemainingSeconds)}</p>
                      <p className="mt-2 text-sm font-semibold text-primary">৳ {item.currentPrice.toLocaleString()}</p>
                    </Link>
                  ))}
                </div>
              </div>
            )}
          </>
        )}
      </AsyncState>
    </div>
  );
}
