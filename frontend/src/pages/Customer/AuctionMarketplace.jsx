import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { AuctionCard } from '../../components/cards';
import { useAuctions } from '../../hooks/useAuctions';

function formatTimeRemaining(seconds) {
  if (seconds <= 0) return 'Closed';
  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);
  return `${days}d ${hours}h`;
}

function toAuctionCardItem(dto) {
  return {
    id: dto.id,
    name: dto.title,
    producer: dto.producerName,
    closesIn: formatTimeRemaining(dto.timeRemainingSeconds),
    bidsCount: dto.bidCount,
    currentBid: dto.currentPrice,
  };
}

export default function AuctionMarketplace() {
  const { data, isLoading, isError, error } = useAuctions({ status: 'Active', pageSize: 20 });
  const auctions = data?.items || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Auctions' }]}
        title="Auctions"
        description="Bid on rare and limited-edition heritage pieces, direct from the maker."
      />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {auctions.map((auction) => (
            <AuctionCard
              key={auction.id}
              auction={toAuctionCardItem(auction)}
              to={routePaths.customerAuctionDetails.replace(':auctionId', auction.id)}
            />
          ))}
          {auctions.length === 0 && <p className="col-span-full text-sm text-body/60">No active auctions right now.</p>}
        </AsyncState>
      </div>
    </div>
  );
}
