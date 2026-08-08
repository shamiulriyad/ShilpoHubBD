export default function MaterialTraceabilityCard({ step, index }) {
  return (
    <div className="flex gap-4 rounded-xl border border-border bg-surface p-4">
      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
        {index + 1}
      </span>
      <div>
        <p className="text-sm font-semibold text-heading">{step.stage}</p>
        <p className="text-xs text-body/50">{step.location}</p>
        <p className="mt-1 text-sm text-body/70">{step.detail}</p>
      </div>
    </div>
  );
}
