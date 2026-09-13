import { getApiErrorMessage } from '../../utils/apiError';

export default function AsyncState({ isLoading, isError, error, loadingText = 'Loading…', children }) {
  if (isLoading) {
    return (
      <div className="flex items-center justify-center gap-2 py-10 text-center text-sm text-body/60" role="status">
        <span aria-hidden="true" className="h-4 w-4 animate-spin rounded-full border-2 border-primary/25 border-t-primary" />
        <span>{loadingText}</span>
      </div>
    );
  }

  if (isError) {
    return (
      <p role="alert" className="rounded-md border border-red-200 bg-red-50 px-3.5 py-2.5 text-sm text-red-700">
        {getApiErrorMessage(error)}
      </p>
    );
  }

  return children;
}