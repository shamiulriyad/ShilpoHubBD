import { Link } from 'react-router-dom';
import CardMedia from '../media/CardMedia';

export default function EntityCard({ title, subtitle, meta, to, image, imageUrl, kind = 'category' }) {
  const className =
    'group flex flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-[0_8px_22px_rgba(45,44,36,0.05)] transition duration-300 hover:-translate-y-1 hover:border-primary/25 hover:shadow-[0_16px_32px_rgba(45,44,36,0.10)]';

  const inner = (
    <>
      <CardMedia src={image || imageUrl} name={title} kind={kind} />
      <div className="flex flex-1 flex-col gap-1 p-4">
        <h3 className="text-sm font-semibold text-heading group-hover:text-primary">{title}</h3>
        {subtitle && <p className="text-xs text-body/60">{subtitle}</p>}
        {meta && <p className="text-xs text-body/50">{meta}</p>}
      </div>
    </>
  );

  return to ? (
    <Link to={to} className={className}>
      {inner}
    </Link>
  ) : (
    <div className={className}>{inner}</div>
  );
}
