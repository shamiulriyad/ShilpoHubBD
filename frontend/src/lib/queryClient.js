import { MutationCache, QueryClient } from '@tanstack/react-query';
import { MUTATION_ERROR_EVENT, MUTATION_SUCCESS_EVENT } from '../components/ui/GlobalFeedback';
import { getApiErrorMessage } from '../utils/apiError';

function shouldRetry(failureCount, error) {
  if (failureCount >= 1) return false;

  const status = error?.response?.status;
  if (!status) return true;
  return status === 408 || status === 429 || status >= 500;
}

// After any successful action, refresh whatever is on screen so lists, counts and statuses never
// show stale data (the page used to keep the old value until the 30 second cache expired).
const mutationCache = new MutationCache({
  onSuccess: (_data, _variables, _context, mutation) => {
    if (mutation.options.meta?.skipRefresh) return;
    queryClient.invalidateQueries({ refetchType: 'active' });
    // Every action answers back (Shneiderman: informative feedback); chatty ones opt out with meta.silent.
    if (!mutation.options.meta?.silent && typeof window !== 'undefined') window.dispatchEvent(new CustomEvent(MUTATION_SUCCESS_EVENT));
  },
  onError: (error, _variables, _context, mutation) => {
    if (mutation.options.meta?.suppressGlobalError) return;
    if (typeof window === 'undefined') return;

    window.dispatchEvent(
      new CustomEvent(MUTATION_ERROR_EVENT, {
        detail: { message: getApiErrorMessage(error, 'The action could not be completed. Please try again.') },
      }),
    );
  },
});

export const queryClient = new QueryClient({
  mutationCache,
  defaultOptions: {
    queries: {
      staleTime: 10_000,
      refetchOnWindowFocus: true,
      refetchOnReconnect: true,
      retry: shouldRetry,
    },
    mutations: {
      retry: false,
    },
  },
});
