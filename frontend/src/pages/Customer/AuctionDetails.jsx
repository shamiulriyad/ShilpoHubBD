import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader } from '../../components/ui';
import { auctions } from '../../data/mockData';

export default function AuctionDetails() {
  const { auctionId } = useParams();
  const auction = auctions.find((a) => a.id === auctionId) || auctions[0];
  const related = auctions.filter((a) => a.id !== auction.id).slice(0, 3);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Auctions', path: routePaths.customerAuctions },
          { label: auction.name },
        ]}
        title={auction.name}
      />

      <div className="grid gap-10 lg:grid-cols-2">
        <div className="flex aspect-square items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
          Item Image
        </div>

        <div>
          <Badge tone="primary">{auction.category}</Badge>
          <p className="mt-3 text-sm text-body/70">{auction.description}</p>

          <Link
            to={routePaths.customerProducerProfile.replace(':producerId', auction.producerId)}
            className="mt-4 block text-sm text-link hover:underline"
          >
            Offered by {auction.producer} · {auction.district}
          </Link>

          <div className="mt-6 space-y-3 rounded-xl border border-border bg-surface p-5">
            <div className="flex items-center justify-between">
              <span className="text-sm text-body/70">Current Bid</span>
              <span className="text-2xl font-semibold text-primary">৳ {auction.currentBid.toLocaleString()}</span>
            </div>
            <div className="flex items-center justify-between text-sm text-body/70">
              <span>{auction.bidsCount} bids</span>
              <span>Closes in {auction.closesIn}</span>
            </div>
            <div className="flex gap-2">
              <input
                placeholder={`৳ ${(auction.currentBid + 200).toLocaleString()} or more`}
                className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <Button variant="primary">Place Bid</Button>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-10">
        <SectionHeader eyebrow="Activity" title="Bid History" />
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {auction.bidHistory.map((bid) => (
            <div key={bid.id} className="flex items-center justify-between p-4 text-sm">
              <span className="font-medium text-heading">{bid.bidder}</span>
              <span className="text-body/60">{bid.time}</span>
              <span className="font-semibold text-primary">৳ {bid.amount.toLocaleString()}</span>
            </div>
          ))}
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
                <p className="text-sm font-semibold text-heading">{item.name}</p>
                <p className="mt-1 text-xs text-body/60">Closes in {item.closesIn}</p>
                <p className="mt-2 text-sm font-semibold text-primary">৳ {item.currentBid.toLocaleString()}</p>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
