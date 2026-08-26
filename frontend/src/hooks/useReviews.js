import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { reviewsService } from '../services/reviewsService';

export function useProductReviews(productId, params = {}) {
  return useQuery({
    queryKey: ['reviews', 'product', productId, params],
    queryFn: () => reviewsService.listForProduct(productId, params),
    enabled: Boolean(productId),
  });
}

export function useReviewMutations(productId) {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['reviews', 'product', productId] });
  };

  const create = useMutation({
    mutationFn: (payload) => reviewsService.create(payload),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (id) => reviewsService.remove(id),
    onSuccess: invalidate,
  });

  return { create, remove };
}
