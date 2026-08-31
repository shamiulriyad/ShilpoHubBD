import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { academyMemberProfilesService } from '../services/academyMemberProfilesService';

export function useMyAcademyProfile() {
  return useQuery({
    queryKey: ['academy-profile', 'me'],
    queryFn: () => academyMemberProfilesService.me(),
    retry: false,
  });
}

export function useMyLearningHistory() {
  return useQuery({ queryKey: ['academy-profile', 'learning-history'], queryFn: () => academyMemberProfilesService.learningHistory() });
}

export function useAcademyProfileMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['academy-profile'] });

  const create = useMutation({ mutationFn: (payload) => academyMemberProfilesService.create(payload), onSuccess: invalidate });
  const update = useMutation({ mutationFn: (payload) => academyMemberProfilesService.updateMe(payload), onSuccess: invalidate });
  const addSkill = useMutation({
    mutationFn: ({ heritageSkillId, level }) => academyMemberProfilesService.addSkill(heritageSkillId, level),
    onSuccess: invalidate,
  });
  const removeSkill = useMutation({
    mutationFn: (heritageSkillId) => academyMemberProfilesService.removeSkill(heritageSkillId),
    onSuccess: invalidate,
  });

  return { create, update, addSkill, removeSkill };
}
