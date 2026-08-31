import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productsService } from '../services/productsService';

export function useProducts(params = {}) {
  return useQuery({
    queryKey: ['products', params],
    queryFn: () => productsService.list(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useFeaturedProducts(count = 8) {
  return useQuery({
    queryKey: ['products', 'featured', count],
    queryFn: () => productsService.featured(count),
  });
}

export function useTrendingProducts(count = 8) {
  return useQuery({
    queryKey: ['products', 'trending', count],
    queryFn: () => productsService.trending(count),
  });
}

export function useProduct(id) {
  return useQuery({
    queryKey: ['products', id],
    queryFn: () => productsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useMyProducts(enabled = true) {
  return useQuery({ queryKey: ['products', 'mine'], queryFn: () => productsService.mine(), enabled });
}

export function useProductAdminMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['products'] });

  const setFeatured = useMutation({
    mutationFn: ({ id, isFeatured }) => productsService.setFeatured(id, isFeatured),
    onSuccess: invalidate,
  });
  const setHandmadeVerification = useMutation({
    mutationFn: ({ id, payload }) => productsService.setHandmadeVerification(id, payload),
    onSuccess: invalidate,
  });

  return { setFeatured, setHandmadeVerification };
}
