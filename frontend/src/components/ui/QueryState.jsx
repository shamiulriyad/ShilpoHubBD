/**
 * Renders the right UI for a react-query result: loading, error (with retry),
 * empty, or the children once data has arrived.
 *
 * Usage:
 *   <QueryState query={query} emptyLabel="No villages yet.">
 *     {(data) => data.map(...)}
 *   </QueryState>
 */
export default function QueryState({
  query,
  children,
  loadingLabel = 'Loading…',
  errorLabel = 'Something went wrong while loading this content.',
  emptyLabel = 'Nothing to show here yet.',
  isEmpty,
  skeleton = null,
}) {
  const { isLoading, isPending, isError, error, data, refetch, isFetching } = query;

  if (isLoading || isPending) {
    return (
      skeleton || (
        <div className="flex items-center justify-center rounded-xl border border-border bg-surface px-4 py-16 text-sm text-body/50">
          {loadingLabel}
        </div>
      )
    );
  }

  if (isError) {
    return (
      <div className="flex flex-col items-center gap-3 rounded-xl border border-border bg-surface px-4 py-16 text-center">
        <p className="text-sm text-body/70">{errorLabel}</p>
        {error?.message && <p className="text-xs text-body/40">{error.message}</p>}
        <button
          type="button"
          onClick={() => refetch()}
          disabled={isFetching}
          className="rounded-lg border border-border px-3 py-1.5 text-xs font-medium text-heading transition hover:bg-background disabled:opacity-50"
        >
          {isFetching ? 'Retrying…' : 'Try again'}
        </button>
      </div>
    );
  }

  const empty =
    typeof isEmpty === 'function'
      ? isEmpty(data)
      : data == null || (Array.isArray(data) && data.length === 0);

  if (empty) {
    return (
      <div className="flex items-center justify-center rounded-xl border border-dashed border-border bg-surface px-4 py-16 text-sm text-body/50">
        {emptyLabel}
      </div>
    );
  }

  return typeof children === 'function' ? children(data) : children;
}
