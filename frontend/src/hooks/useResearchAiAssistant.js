import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { researchAiAssistantService } from '../services/researchAiAssistantService';

export function useResearchAnalyses(projectId, params = {}) {
  return useQuery({
    queryKey: ['research-ai-analyses', projectId, params],
    queryFn: () => researchAiAssistantService.listAnalyses(projectId, params),
    enabled: Boolean(projectId),
  });
}

export function useResearchAiMutations(projectId) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['research-ai-analyses', projectId] });

  return {
    runInsights: useMutation({ mutationFn: (payload) => researchAiAssistantService.runInsights(projectId, payload), onSuccess: invalidate }),
    runTrends: useMutation({ mutationFn: (payload) => researchAiAssistantService.runTrends(projectId, payload), onSuccess: invalidate }),
    runCorrelations: useMutation({ mutationFn: (payload) => researchAiAssistantService.runCorrelations(projectId, payload), onSuccess: invalidate }),
    runReport: useMutation({ mutationFn: (payload) => researchAiAssistantService.runReport(projectId, payload), onSuccess: invalidate }),
    generateCitations: useMutation({ mutationFn: (payload) => researchAiAssistantService.generateCitations(projectId, payload), onSuccess: invalidate }),
    removeAnalysis: useMutation({ mutationFn: (analysisId) => researchAiAssistantService.removeAnalysis(projectId, analysisId), onSuccess: invalidate }),
  };
}
