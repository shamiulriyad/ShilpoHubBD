export default function AsyncState({ isLoading, isError, error, loadingText = 'Loading…', children }) {
  if (isLoading) {
    return <p className="py-10 text-center text-sm text-body/60">{loadingText}</p>;
  }

  if (isError) {
    const message = error?.response?.data?.title || error?.message || 'Something went wrong. Please try again.';
    return (
      <p className="rounded-md border border-red-200 bg-red-50 px-3.5 py-2.5 text-sm text-red-600">{message}</p>
    );
  }

  return children;
}
