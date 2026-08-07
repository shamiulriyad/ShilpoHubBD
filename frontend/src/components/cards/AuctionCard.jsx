import { Link } from 'react-router-dom';
import Badge from '../ui/Badge';

export default function AuctionCard({ auction, to }) {
  return (
    <Link
      to={to || '#'}
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
  );
}
