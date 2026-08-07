export default function TimelineViewer({ items = [] }) {
  return (
    <ol className="space-y-8">
      {items.map((item, index) => (
        <li key={index} className="relative flex gap-4">
          <div className="flex flex-col items-center">
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
              {item.marker ?? index + 1}
            </span>
            {index < items.length - 1 && <span className="mt-1 w-px flex-1 bg-border" />}
          </div>
          <div className="max-w-3xl pb-2">
            <h3 className="text-sm font-semibold text-heading">{item.title}</h3>
            <p className="mt-1 text-sm leading-relaxed text-body/80">{item.description}</p>
          </div>
        </li>
      ))}
    </ol>
  );
}
