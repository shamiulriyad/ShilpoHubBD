import { useMutation } from '@tanstack/react-query';
import { aiShoppingService } from '../services/aiShoppingService';

export function useGiftRecommendations() {
  return useMutation({ mutationFn: (payload) => aiShoppingService.giftRecommendations(payload) });
}

export function useFashionMatches() {
  return useMutation({ mutationFn: (payload) => aiShoppingService.fashionMatches(payload) });
}

export function useInteriorPreview() {
  return useMutation({ mutationFn: (payload) => aiShoppingService.interiorPreview(payload) });
}
