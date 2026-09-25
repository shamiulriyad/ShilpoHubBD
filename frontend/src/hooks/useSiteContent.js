import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { craftHeritageService, siteContentService } from '../services/siteContentService';
import { toCraftHeritage } from '../data/craftHeritage';

const STALE = 5 * 60 * 1000;

// All admin-managed site copy in one request, grouped by `group` (about-stat, footer-explore, ...).
export function useSiteContent() {
  const query = useQuery({ queryKey: ['site-content'], queryFn: () => siteContentService.list(), staleTime: STALE });
  const groups = useMemo(() => {
    const byGroup = {};
    for (const item of Array.isArray(query.data) ? query.data : []) (byGroup[item.group] ||= []).push(item);
    return byGroup;
  }, [query.data]);
  return { ...query, groups, group: (name) => groups[name] || [] };
}

export function useCraftHeritage() {
  const query = useQuery({ queryKey: ['craft-heritage'], queryFn: () => craftHeritageService.list(), staleTime: STALE });
  const records = useMemo(() => (Array.isArray(query.data) ? query.data : []).map(toCraftHeritage), [query.data]);
  return { ...query, records };
}
