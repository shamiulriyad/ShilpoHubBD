import { Link } from 'react-router-dom';
import Badge from '../ui/Badge';

export default function RecommendationCard({ product, reason, matchScore, to }) {
  return (
    <Link
      to={to || '#'}
      className="group flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
    >
      <div className="flex aspect-[4/3] items-center justify-center bg-background text-xs text-body/40">
        Product Image
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <div className="flex items-center justify-between gap-2">
          <Badge tone="secondary">{product.category}</Badge>
          {matchScore != null && <Badge tone="success">{matchScore}% Match</Badge>}
        </div>
        <h3 className="text-sm font-semibold text-heading group-hover:text-primary">{product.name}</h3>
        <p className="text-xs text-body/60">
          {product.producer} · {product.district}
        </p>
        {reason && <p className="text-xs italic text-body/50">{reason}</p>}
        <p className="mt-auto text-sm font-semibold text-primary">৳ {product.price?.toLocaleString()}</p>
      </div>
    </Link>
  );
}
