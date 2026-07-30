export default function SectionHeader({ eyebrow, title, description, action }) {
  return (
    <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        {eyebrow && (
          <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-primary">{eyebrow}</p>
        )}
        <h2 className="text-xl font-semibold text-heading sm:text-2xl">{title}</h2>
        {description && <p className="mt-1 max-w-2xl text-sm text-body/80">{description}</p>}
      </div>
      {action && <div className="shrink-0">{action}</div>}
    </div>
  );
}
