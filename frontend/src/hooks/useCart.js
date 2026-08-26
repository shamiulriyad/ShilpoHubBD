import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { cartService } from '../services/cartService';

export function useCart(enabled = true) {
  return useQuery({
    queryKey: ['cart'],
    queryFn: () => cartService.list(),
    enabled,
  });
}

export function useCartSummary(enabled = true) {
  return useQuery({
    queryKey: ['cart', 'summary'],
    queryFn: () => cartService.summary(),
    enabled,
  });
}

export function useCartMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['cart'] });
  };

  const add = useMutation({
    mutationFn: ({ productId, productVariantId, quantity = 1 }) =>
      cartService.add({ productId, productVariantId, quantity }),
    onSuccess: invalidate,
  });

  const updateQuantity = useMutation({
    mutationFn: ({ itemId, quantity }) => cartService.updateQuantity(itemId, quantity),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (itemId) => cartService.remove(itemId),
    onSuccess: invalidate,
  });

  const clear = useMutation({
    mutationFn: () => cartService.clear(),
    onSuccess: invalidate,
  });

  return { add, updateQuantity, remove, clear };
}
