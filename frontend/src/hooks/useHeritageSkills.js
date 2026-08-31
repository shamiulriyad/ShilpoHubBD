import { useQuery } from '@tanstack/react-query';
import { heritageSkillsService } from '../services/heritageSkillsService';

export function useHeritageSkills() {
  return useQuery({ queryKey: ['heritage-skills'], queryFn: () => heritageSkillsService.list() });
}
