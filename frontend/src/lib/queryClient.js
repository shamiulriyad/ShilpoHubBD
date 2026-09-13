import { MutationCache, QueryClient } from '@tanstack/react-query';
import { MUTATION_ERROR_EVENT } from '../components/ui/GlobalFeedback';
import { getApiErrorMessage } from '../utils/apiError';

function shouldRetry(failureCount, error) {
  if (failureCount >= 1) return false;

  const status = error?.response?.status;
  if (!status) return true;
  return status === 408 || status === 429 || status >= 500;
}

const mutationCache = new MutationCache({
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
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      retry: shouldRetry,
    },
    mutations: {
      retry: false,
    },
  },
});
