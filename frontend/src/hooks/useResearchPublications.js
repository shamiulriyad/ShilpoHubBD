import { useQuery } from '@tanstack/react-query';
import { researchPublicationsService } from '../services/researchPublicationsService';

// GET /api/research/publications is [Authorize] — pass enabled=false for anonymous visitors.
export function useResearchPublications(params = {}, enabled = true) {
  return useQuery({
    queryKey: ['research-publications', params],
    queryFn: () => researchPublicationsService.browse(params),
    enabled,
    placeholderData: (previousData) => previousData,
  });
}

export function useResearchPublication(id) {
  return useQuery({
    queryKey: ['research-publications', id],
    queryFn: () => researchPublicationsService.getById(id),
    enabled: Boolean(id),
  });
}
