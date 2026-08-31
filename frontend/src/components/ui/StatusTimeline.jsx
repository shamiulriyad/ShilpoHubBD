export default function StatusTimeline({ events = [] }) {
  if (events.length === 0) return <p className="text-sm text-body/60">No status history yet.</p>;

  return (
    <ol className="space-y-2">
      {events.map((event, i) => (
        <li key={i} className="flex items-center justify-between rounded-lg border border-border bg-surface px-3 py-2 text-sm">
          <div>
            <span className="font-medium text-heading">{event.status}</span>
            {event.note && <span className="ml-2 text-xs text-body/60">{event.note}</span>}
          </div>
          <span className="shrink-0 text-xs text-body/50">{new Date(event.createdAt).toLocaleString()}</span>
        </li>
      ))}
    </ol>
  );
}
