import { Link } from 'react-router-dom';
import Badge from '../ui/Badge';
import WishlistButton from '../ui/WishlistButton';

export default function ProductCard({ product, to }) {
  return (
    <Link
      to={to || '#'}
      className="group flex flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-[0_8px_22px_rgba(45,44,36,0.05)] transition duration-300 hover:-translate-y-1 hover:border-primary/25 hover:shadow-[0_16px_32px_rgba(45,44,36,0.10)]"
    >
      <div className="relative flex aspect-[4/3] items-center justify-center overflow-hidden bg-primary-soft text-3xl">
        <span className="absolute -bottom-8 -left-7 h-28 w-28 rounded-full bg-secondary/30 transition duration-500 group-hover:scale-125" />
        <span className="relative">✦</span>
        <WishlistButton className="absolute right-2 top-2 bg-surface" />
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <Badge tone="secondary">{product.category}</Badge>
        <h3 className="text-sm font-semibold leading-5 text-heading group-hover:text-primary">{product.name}</h3>
        <p className="text-xs text-body/60">
          {product.producer} · {product.district}
        </p>
        <p className="mt-auto text-sm font-semibold text-primary">৳ {product.price?.toLocaleString()}</p>
      </div>
    </Link>
  );
}
