import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { wishlistService } from '../services/wishlistService';

export function useWishlist(enabled = true) {
  return useQuery({
    queryKey: ['wishlist'],
    queryFn: () => wishlistService.list(),
    enabled,
  });
}

export function useWishlistMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['wishlist'] });
  };

  const add = useMutation({
    mutationFn: (productId) => wishlistService.add(productId),
    onSuccess: invalidate,
  });

  const remove = useMutation({
    mutationFn: (productId) => wishlistService.remove(productId),
    onSuccess: invalidate,
  });

  const moveToCart = useMutation({
    mutationFn: ({ productId, ...payload }) => wishlistService.moveToCart(productId, payload),
    onSuccess: () => {
      invalidate();
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    },
  });

  return { add, remove, moveToCart };
}
