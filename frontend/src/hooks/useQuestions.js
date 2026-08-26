import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { questionsService } from '../services/questionsService';

export function useProductQuestions(productId, params = {}) {
  return useQuery({
    queryKey: ['questions', 'product', productId, params],
    queryFn: () => questionsService.listForProduct(productId, params),
    enabled: Boolean(productId),
  });
}

export function useQuestionMutations(productId) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['questions', 'product', productId] });

  const ask = useMutation({
    mutationFn: (body) => questionsService.ask(productId, body),
    onSuccess: invalidate,
  });

  const answer = useMutation({
    mutationFn: ({ id, body }) => questionsService.answer(id, body),
    onSuccess: invalidate,
  });

  return { ask, answer };
}
