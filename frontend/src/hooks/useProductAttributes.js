import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { productAttributesService } from '../services/productAttributesService';

export function useProductAttributes(productId) {
  return useQuery({ queryKey: ['product-attributes', productId], queryFn: () => productAttributesService.get(productId), enabled: Boolean(productId) });
}

export function useAttributeSuggestion(productId) {
  return useQuery({ queryKey: ['product-attribute-suggestion', productId], queryFn: () => productAttributesService.suggestion(productId), enabled: Boolean(productId) });
}

export function useProductLookups() {
  const types = useQuery({ queryKey: ['product-types'], queryFn: productAttributesService.productTypes, staleTime: 5 * 60_000 });
  const materials = useQuery({ queryKey: ['materials'], queryFn: productAttributesService.materials, staleTime: 5 * 60_000 });
  return { types, materials };
}

export function useProductAttributeMutations(productId) {
  const queryClient = useQueryClient();
  const attributesKey = ['product-attributes', productId];
  const suggestionKey = ['product-attribute-suggestion', productId];
  // The server's answer is written straight into the cache, so the page never flashes stale (pre-save) data
  // while the background refetch is still in flight.
  const setAttributes = (data) => queryClient.setQueryData(attributesKey, data);
  const setSuggestion = (data) => queryClient.setQueryData(suggestionKey, data);
  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: attributesKey });
    queryClient.invalidateQueries({ queryKey: suggestionKey });
  };
  return {
    save: useMutation({ mutationFn: (payload) => productAttributesService.save(productId, payload), onSuccess: (data) => { setAttributes(data); refresh(); } }),
    generate: useMutation({ mutationFn: () => productAttributesService.generate(productId), onSuccess: (data) => { setSuggestion(data); } }),
    confirm: useMutation({
      mutationFn: ({ suggestionId, attributes }) => productAttributesService.confirm(productId, suggestionId, attributes),
      onSuccess: (data) => { setAttributes(data); setSuggestion(null); refresh(); },
    }),
    dismiss: useMutation({ mutationFn: (suggestionId) => productAttributesService.dismiss(productId, suggestionId), onSuccess: () => { setSuggestion(null); refresh(); } }),
  };
}
