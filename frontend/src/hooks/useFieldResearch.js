import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fieldResearchService } from '../services/fieldResearchService';

export function useSurveys(params = {}) {
  return useQuery({ queryKey: ['surveys', 'list', params], queryFn: () => fieldResearchService.listSurveys(params) });
}

export function useSurvey(id) {
  return useQuery({ queryKey: ['surveys', id], queryFn: () => fieldResearchService.getSurvey(id), enabled: Boolean(id) });
}

export function useSurveyMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['surveys'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['surveys', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => fieldResearchService.createSurvey(payload), onSuccess: () => invalidate() }),
    updateStatus: useMutation({
      mutationFn: ({ id, payload }) => fieldResearchService.updateSurveyStatus(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({ mutationFn: (id) => fieldResearchService.removeSurvey(id), onSuccess: () => invalidate() }),
    addQuestion: useMutation({
      mutationFn: ({ id, payload }) => fieldResearchService.addQuestion(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeQuestion: useMutation({
      mutationFn: ({ id, questionId }) => fieldResearchService.removeQuestion(id, questionId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    assignFieldResearcher: useMutation({
      mutationFn: ({ id, payload }) => fieldResearchService.assignFieldResearcher(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeFieldResearcher: useMutation({
      mutationFn: ({ id, assignmentId }) => fieldResearchService.removeFieldResearcher(id, assignmentId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}

export function useSurveyResponses(surveyId, params = {}) {
  return useQuery({
    queryKey: ['survey-responses', surveyId, params],
    queryFn: () => fieldResearchService.listResponses(surveyId, params),
    enabled: Boolean(surveyId),
  });
}

export function useSurveyEvidence(surveyId, params = {}) {
  return useQuery({
    queryKey: ['survey-evidence', surveyId, params],
    queryFn: () => fieldResearchService.listEvidence(surveyId, params),
    enabled: Boolean(surveyId),
  });
}

export function useSurveyWorkItemMutations(surveyId) {
  const queryClient = useQueryClient();

  return {
    reviewResponse: useMutation({
      mutationFn: ({ responseId, payload }) => fieldResearchService.reviewResponse(surveyId, responseId, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['survey-responses', surveyId] }),
    }),
    createEvidence: useMutation({
      mutationFn: (payload) => fieldResearchService.createEvidence(surveyId, payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['survey-evidence', surveyId] }),
    }),
    removeEvidence: useMutation({
      mutationFn: (evidenceId) => fieldResearchService.removeEvidence(surveyId, evidenceId),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['survey-evidence', surveyId] }),
    }),
  };
}
