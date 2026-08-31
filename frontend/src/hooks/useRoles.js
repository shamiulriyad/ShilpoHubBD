import { useMutation } from '@tanstack/react-query';
import { rolesService } from '../services/rolesService';

export function useRoleMutations() {
  const assign = useMutation({ mutationFn: ({ userId, role }) => rolesService.assign(userId, role) });
  const remove = useMutation({ mutationFn: ({ userId, role }) => rolesService.remove(userId, role) });
  return { assign, remove };
}
