import { Link } from 'react-router-dom';

export default function VillageCard({ village, to }) {
  return (
    <Link
      to={to || '#'}
      className="group flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
    >
      <div className="flex aspect-[4/3] items-center justify-center bg-background text-xs text-body/40">
        Village Image
      </div>
      <div className="flex flex-1 flex-col gap-1.5 p-4">
        <h3 className="text-sm font-semibold text-heading group-hover:text-primary">{village.name}</h3>
        <p className="text-xs text-body/60">{village.craft}</p>
        <p className="text-xs text-body/50">{village.district}</p>
      </div>
    </Link>
  );
}
