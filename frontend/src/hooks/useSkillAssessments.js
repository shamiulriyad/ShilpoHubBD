import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { skillAssessmentsService } from '../services/skillAssessmentsService';

export function useSkillAssessmentHistory() {
  return useQuery({ queryKey: ['skill-assessments', 'history'], queryFn: () => skillAssessmentsService.history() });
}

export function useRunSkillAssessment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (heritageSkillId) => skillAssessmentsService.run(heritageSkillId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['skill-assessments', 'history'] }),
  });
}
