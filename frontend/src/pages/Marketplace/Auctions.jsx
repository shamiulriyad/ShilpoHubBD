import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useAuctions } from '../../hooks/useAuctions';

function formatTimeRemaining(seconds) {
  if (seconds <= 0) return 'Closed';
  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);
  return `${days}d ${hours}h`;
}

export default function Auctions() {
  const { data, isLoading, isError, error } = useAuctions({ status: 'Active' });
  const auctions = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Auctions' },
        ]}
        title="Auctions"
        description="Bid on rare and limited-edition heritage pieces."
      />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {auctions.map((auction) => (
            <Link
              key={auction.id}
              to={routePaths.customerAuctionDetails.replace(':auctionId', auction.id)}
              className="flex flex-col rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
            >
              <div className="mb-3 flex aspect-square items-center justify-center rounded-lg bg-background text-xs text-body/40">
                {auction.productImageUrl ? (
                  <img src={auction.productImageUrl} alt={auction.title} className="h-full w-full rounded-lg object-cover" />
                ) : (
                  'Item Image'
                )}
              </div>
              <Badge tone="primary" className="mb-2 self-start">
                Closes in {formatTimeRemaining(auction.timeRemainingSeconds)}
              </Badge>
              <p className="text-sm font-semibold text-heading">{auction.title}</p>
              <p className="mt-1 text-xs text-body/60">{auction.bidCount} bids</p>
              <p className="mt-2 text-lg font-semibold text-primary">৳ {auction.currentPrice.toLocaleString()}</p>
            </Link>
          ))}
          {auctions.length === 0 && <p className="col-span-full text-sm text-body/60">No active auctions right now.</p>}
        </AsyncState>
      </div>
    </div>
  );
}
