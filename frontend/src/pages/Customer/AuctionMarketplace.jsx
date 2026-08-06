import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { auctions } from '../../data/mockData';

export default function AuctionMarketplace() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Auctions' }]}
        title="Auctions"
        description="Bid on rare and limited-edition heritage pieces, direct from the maker."
      />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {auctions.map((auction) => (
          <Link
            key={auction.id}
            to={routePaths.customerAuctionDetails.replace(':auctionId', auction.id)}
            className="group flex flex-col rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
          >
            <div className="mb-3 flex aspect-square items-center justify-center rounded-lg bg-background text-xs text-body/40">
              Item Image
            </div>
            <Badge tone="primary" className="mb-2 self-start">
              Closes in {auction.closesIn}
            </Badge>
            <p className="text-sm font-semibold text-heading group-hover:text-primary">{auction.name}</p>
            <p className="mt-1 text-xs text-body/60">
              {auction.producer} · {auction.bidsCount} bids
            </p>
            <p className="mt-2 text-lg font-semibold text-primary">৳ {auction.currentBid.toLocaleString()}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
