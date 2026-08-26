import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { discussionsService } from '../services/discussionsService';

export function useDiscussions(params = {}) {
  return useQuery({
    queryKey: ['discussions', params],
    queryFn: () => discussionsService.list(params),
  });
}

export function useDiscussion(id) {
  return useQuery({
    queryKey: ['discussions', id],
    queryFn: () => discussionsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useDiscussionMutations() {
  const queryClient = useQueryClient();

  const create = useMutation({
    mutationFn: (payload) => discussionsService.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['discussions'] }),
  });

  const reply = useMutation({
    mutationFn: ({ id, body }) => discussionsService.reply(id, body),
    onSuccess: (_, { id }) => queryClient.invalidateQueries({ queryKey: ['discussions', id] }),
  });

  return { create, reply };
}
