import Badge from '../ui/Badge';
import OptionalCardLink from './OptionalCardLink';
import CardMedia from '../media/CardMedia';

export default function ProductCard({ product, to }) {
  return (
    <OptionalCardLink
      to={to}
      className="group flex flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-[0_8px_22px_rgba(45,44,36,0.05)] transition duration-300 hover:-translate-y-1 hover:border-primary/25 hover:shadow-[0_16px_32px_rgba(45,44,36,0.10)]"
    >
      <CardMedia src={product.image || product.primaryImageUrl} name={product.name} category={product.category} />
      <div className="flex flex-1 flex-col gap-2 p-4">
        {product.category && <div className="self-start"><Badge tone="secondary">{product.category}</Badge></div>}
        <h3 className="text-sm font-semibold leading-5 text-heading group-hover:text-primary">{product.name}</h3>
        {(product.producer || product.district) && (
          <p className="text-xs text-body/60">
            {[product.producer, product.district].filter(Boolean).join(' · ')}
          </p>
        )}
        {product.price != null && (
          <p className="mt-auto text-sm font-semibold text-primary">৳ {Number(product.price).toLocaleString()}</p>
        )}
      </div>
    </OptionalCardLink>
  );
}
