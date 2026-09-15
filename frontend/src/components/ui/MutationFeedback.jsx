import { getApiErrorMessage } from '../../utils/apiError';

export default function MutationFeedback({ mutation, successMessage }) {
  if (mutation.isError) return <p role="alert" className="rounded-lg border border-error/20 bg-error/5 p-3 text-sm text-error">{getApiErrorMessage(mutation.error)}</p>;
  if (mutation.isSuccess && successMessage) return <p role="status" className="rounded-lg border border-success/20 bg-success/5 p-3 text-sm text-success">{successMessage}</p>;
  return null;
}
