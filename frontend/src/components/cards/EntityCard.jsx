import { Link } from 'react-router-dom';

export default function EntityCard({ title, subtitle, meta, to, image }) {
  const className =
    'group flex flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-[0_8px_22px_rgba(45,44,36,0.05)] transition duration-300 hover:-translate-y-1 hover:border-primary/25 hover:shadow-[0_16px_32px_rgba(45,44,36,0.10)]';

  const inner = (
    <>
      <div className="relative aspect-[4/3] overflow-hidden bg-primary-soft">
        {image ? (
          <>
            <img src={image} alt="" className="h-full w-full object-cover transition duration-700 group-hover:scale-105" />
            <span className="absolute inset-0 bg-gradient-to-t from-title/25 via-transparent to-transparent" />
          </>
        ) : (
          <div className="flex h-full items-center justify-center text-2xl">
            <span className="absolute -right-4 -top-5 h-20 w-20 rounded-full bg-secondary/25" />
            <span className="relative text-title/70">✦</span>
          </div>
        )}
      </div>
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