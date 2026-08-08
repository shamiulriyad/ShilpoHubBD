import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { AuctionCard } from '../../components/cards';
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
          <AuctionCard
            key={auction.id}
            auction={auction}
            to={routePaths.customerAuctionDetails.replace(':auctionId', auction.id)}
          />
        ))}
      </div>
    </div>
  );
}
