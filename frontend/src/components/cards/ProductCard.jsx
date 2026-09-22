import CardMedia from '../media/CardMedia';
import OptionalCardLink from './OptionalCardLink';

export default function ProductCard({ product, to }) {
  return (
    <OptionalCardLink
      to={to}
      className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface transition duration-200 hover:border-primary/40 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-4"
    >
      <CardMedia src={product.image || product.primaryImageUrl} name={product.name} category={product.category} />
      <div className="flex flex-1 flex-col p-5">
        {product.category && <p className="text-[10px] font-semibold uppercase tracking-[.14em] text-primary">{product.category}</p>}
        <h3 className="mt-2 text-base font-semibold leading-6 text-heading group-hover:text-primary">{product.name}</h3>
        {(product.producer || product.district) && (
          <p className="mt-2 text-xs leading-5 text-body/75">
            {[product.producer, product.district].filter(Boolean).join(' · ')}
          </p>
        )}
        <div className="mt-auto pt-5"><div className="flex items-center justify-between gap-3 border-t border-border pt-4">
          {product.price != null && <p className="text-lg font-semibold tabular-nums text-heading">৳ {Number(product.price).toLocaleString('en-BD')}</p>}
          {to && <span className="text-xs font-medium text-primary">View details <span aria-hidden="true">↗</span></span>}
        </div></div>
      </div>
    </OptionalCardLink>
  );
}
