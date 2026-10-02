import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { questionsService } from '../services/questionsService';
import { mergeDemoPage } from '../data/producerDemoData';

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
    mutationFn: ({ body, imageUrl }) => questionsService.ask(productId, body, imageUrl),
    onSuccess: invalidate,
  });

  const answer = useMutation({
    mutationFn: ({ id, body }) => questionsService.answer(id, body),
    onSuccess: invalidate,
  });

  return { ask, answer };
}

export function useProducerQuestions(params = {}) {
  return useQuery({
    queryKey: ['questions', 'mine', params],
    queryFn: async () => {
      const result = mergeDemoPage(await questionsService.listMine(params), 'questions');
      if (params.unansweredOnly) result.items = result.items.filter((item) => !item.answers?.length);
      result.totalCount = result.items.length;
      return result;
    },
  });
}

export function useAnswerQuestion() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, body }) => questionsService.answer(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['questions'] }),
  });
}
