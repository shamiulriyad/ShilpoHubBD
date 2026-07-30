export default function FestivalCard({ festival }) {
  const date = new Date(festival.date);
  const day = Number.isNaN(date.getTime()) ? '--' : date.getDate();
  const month = Number.isNaN(date.getTime()) ? '' : date.toLocaleString('default', { month: 'short' });

  return (
    <div className="flex gap-4 rounded-xl border border-border bg-surface p-4 transition hover:shadow-md">
      <div className="flex h-14 w-14 shrink-0 flex-col items-center justify-center rounded-lg bg-primary/10 text-primary">
        <span className="text-lg font-semibold leading-none">{day}</span>
        <span className="text-[10px] font-medium uppercase">{month}</span>
      </div>
      <div>
        <h3 className="text-sm font-semibold text-heading">{festival.name}</h3>
        <p className="mt-1 text-xs text-body/60">{festival.district}</p>
      </div>
    </div>
  );
}
