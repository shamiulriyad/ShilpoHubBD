import { useCallback, useEffect, useState } from 'react';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { performingArtsAdminService, performingArtsService } from '../services/performingArtsService';
import { LANGUAGES } from '../utils/performingArtsAdapters';

const STALE = 5 * 60 * 1000;

export function usePerformingArts(params) {
  return useQuery({
    queryKey: ['performing-arts', 'list', params],
    queryFn: ({ signal }) => performingArtsService.list(params, signal),
    placeholderData: keepPreviousData,
    staleTime: STALE,
  });
}

export function useFeaturedPerformingArts(count = 6) {
  return useQuery({ queryKey: ['performing-arts', 'featured', count], queryFn: () => performingArtsService.featured(count), staleTime: STALE });
}

export function usePerformingArtFacets() {
  return useQuery({ queryKey: ['performing-arts', 'facets'], queryFn: () => performingArtsService.facets(), staleTime: STALE });
}

export function usePerformingArt(slug) {
  return useQuery({
    queryKey: ['performing-arts', 'detail', slug],
    queryFn: () => performingArtsService.getBySlug(slug),
    enabled: Boolean(slug),
    retry: (count, error) => error?.response?.status !== 404 && count < 2,
    staleTime: STALE,
  });
}

export function useAdminPerformingArts(params) {
  return useQuery({
    queryKey: ['performing-arts', 'admin', params],
    queryFn: () => performingArtsAdminService.list(params),
    placeholderData: keepPreviousData,
  });
}

export function usePerformingArtAdminMutations() {
  const client = useQueryClient();
  const invalidate = () => client.invalidateQueries({ queryKey: ['performing-arts'] });
  return {
    save: useMutation({ mutationFn: ({ id, payload }) => performingArtsAdminService.save(id, payload), onSuccess: invalidate }),
    transition: useMutation({ mutationFn: ({ id, action, note }) => performingArtsAdminService.transition(id, action, note), onSuccess: invalidate }),
    remove: useMutation({ mutationFn: (id) => performingArtsAdminService.remove(id), onSuccess: invalidate }),
  };
}

const LANGUAGE_KEY = 'shilpohub.contentLanguage';

/** English/Bangla content toggle, remembered across visits. */
export function useContentLanguage() {
  const [lang, setLangState] = useState(() => {
    try {
      const saved = window.localStorage.getItem(LANGUAGE_KEY);
      return LANGUAGES.includes(saved) ? saved : 'en';
    } catch {
      return 'en';
    }
  });
  useEffect(() => {
    try { window.localStorage.setItem(LANGUAGE_KEY, lang); } catch { /* storage unavailable: keep in memory */ }
  }, [lang]);
  const setLang = useCallback((next) => setLangState(LANGUAGES.includes(next) ? next : 'en'), []);
  return [lang, setLang];
}
