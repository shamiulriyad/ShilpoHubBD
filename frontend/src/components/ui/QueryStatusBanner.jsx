import { getApiErrorMessage } from '../../utils/apiError';

export default function QueryStatusBanner({ queries = [], loadingLabel = 'Loading dashboard data…' }) {
  const loading = queries.some((query) => query?.isLoading || query?.isPending);
  const failed = queries.find((query) => query?.isError);

  if (failed) {
    return (
      <div role="alert" className="mb-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
        {getApiErrorMessage(failed.error, 'Some dashboard information could not be loaded.')}
      </div>
    );
  }

  if (loading) {
    return (
      <div role="status" className="mb-5 flex items-center gap-2 rounded-lg border border-border bg-surface px-4 py-3 text-sm text-body/60">
        <span aria-hidden="true" className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-primary/25 border-t-primary" />
        {loadingLabel}
      </div>
    );
  }

  return null;
}