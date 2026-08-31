import { Link } from 'react-router-dom';

export default function EntityCard({ title, subtitle, meta, to }) {
  const className =
    'group flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md';

  const inner = (
    <>
      <div className="flex aspect-[4/3] items-center justify-center bg-background text-xs text-body/40">
        Image
      </div>
      <div className="flex flex-1 flex-col gap-1 p-4">
        <h3 className="text-sm font-semibold text-heading group-hover:text-primary">{title}</h3>
        {subtitle && <p className="text-xs text-body/60">{subtitle}</p>}
        {meta && <p className="text-xs text-body/50">{meta}</p>}
      </div>
    </>
  );

<<<<<<< HEAD
  return to ? (
=======
  if (!to) {
    return <div className={className}>{inner}</div>;
  }

  return (
>>>>>>> origin/main
    <Link to={to} className={className}>
      {inner}
    </Link>
  ) : (
    <div className={className}>{inner}</div>
  );
}
