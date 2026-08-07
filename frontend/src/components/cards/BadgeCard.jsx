export default function BadgeCard({ badge }) {
  return (
    <div
      className={`flex flex-col items-center gap-2 rounded-xl border p-5 text-center ${
        badge.earned ? 'border-border bg-surface' : 'border-dashed border-border bg-surface opacity-50 grayscale'
      }`}
    >
      <span className="text-3xl">{badge.icon}</span>
      <p className="text-sm font-semibold text-heading">{badge.name}</p>
      <p className="text-xs text-body/60">{badge.description}</p>
      {!badge.earned && <p className="text-xs font-medium text-body/40">Locked</p>}
    </div>
  );
}
