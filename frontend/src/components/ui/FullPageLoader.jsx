export default function FullPageLoader({ label = 'Loading…' }) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4" role="status" aria-live="polite">
      <div className="flex items-center gap-3 rounded-xl border border-border bg-surface px-5 py-4 text-sm text-body/70 shadow-sm">
        <span
          aria-hidden="true"
          className="h-5 w-5 animate-spin rounded-full border-2 border-primary/25 border-t-primary"
        />
        <span>{label}</span>
      </div>
    </div>
  );
}
