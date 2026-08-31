import { useQuery } from '@tanstack/react-query';
import { catalogService } from '../../services/catalogService';

const STALE = 5 * 60 * 1000;

export function useDistricts() {
  return useQuery({
    queryKey: ['districts'],
    queryFn: () => catalogService.getDistricts(),
    staleTime: STALE,
  });
}

export function useVillages() {
  return useQuery({
    queryKey: ['villages'],
    queryFn: () => catalogService.getVillages(),
    staleTime: STALE,
  });
}

export function useVillage(id) {
  return useQuery({
    queryKey: ['village', id],
    queryFn: () => catalogService.getVillage(id),
    enabled: Boolean(id),
    staleTime: STALE,
  });
}

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => catalogService.getCategories(),
    staleTime: STALE,
  });
}

export function useCategory(id) {
  return useQuery({
    queryKey: ['category', id],
    queryFn: () => catalogService.getCategory(id),
    enabled: Boolean(id),
    staleTime: STALE,
  });
}

export function useCraftStoryByCategory(categoryId) {
  return useQuery({
    queryKey: ['craft-story', 'category', categoryId],
    queryFn: () => catalogService.getCraftStoryByCategory(categoryId),
    enabled: Boolean(categoryId),
    staleTime: STALE,
    retry: false, // a category may legitimately have no story yet
  });
}

export function useProducerStory(producerId) {
  return useQuery({
    queryKey: ['producer-story', producerId],
    queryFn: () => catalogService.getProducerStory(producerId),
    enabled: Boolean(producerId),
    staleTime: STALE,
    retry: false,
  });
}

export function useProducts(params = {}) {
  return useQuery({
    queryKey: ['products', params],
    queryFn: () => catalogService.getProducts(params),
    staleTime: STALE,
    keepPreviousData: true,
  });
}

export function useFeaturedProducts(count = 12) {
  return useQuery({
    queryKey: ['products', 'featured', count],
    queryFn: () => catalogService.getFeaturedProducts(count),
    staleTime: STALE,
  });
}
