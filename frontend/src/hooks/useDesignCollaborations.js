import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { designCollaborationsService } from '../services/designCollaborationsService';

export function useMyDesignCollaborations(params = {}) {
  return useQuery({ queryKey: ['design-collaborations', 'mine', params], queryFn: () => designCollaborationsService.mine(params) });
}

export function useReceivedDesignCollaborations(params = {}) {
  return useQuery({ queryKey: ['design-collaborations', 'received', params], queryFn: () => designCollaborationsService.received(params) });
}

export function useDesignCollaboration(id) {
  return useQuery({ queryKey: ['design-collaborations', id], queryFn: () => designCollaborationsService.getById(id), enabled: Boolean(id) });
}

export function useDesignCollaborationMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['design-collaborations'] });

  const create = useMutation({ mutationFn: (payload) => designCollaborationsService.create(payload), onSuccess: invalidate });
  const respond = useMutation({ mutationFn: ({ id, accept }) => designCollaborationsService.respond(id, accept), onSuccess: invalidate });
  const addComment = useMutation({ mutationFn: ({ id, content }) => designCollaborationsService.addComment(id, content), onSuccess: invalidate });
  const addFile = useMutation({ mutationFn: ({ id, payload }) => designCollaborationsService.addFile(id, payload), onSuccess: invalidate });
  const submitRevision = useMutation({
    mutationFn: ({ id, payload }) => designCollaborationsService.submitRevision(id, payload),
    onSuccess: invalidate,
  });
  const decideRevision = useMutation({
    mutationFn: ({ id, revisionId, payload }) => designCollaborationsService.decideRevision(id, revisionId, payload),
    onSuccess: invalidate,
  });
  const complete = useMutation({ mutationFn: (id) => designCollaborationsService.complete(id), onSuccess: invalidate });
  const cancel = useMutation({ mutationFn: (id) => designCollaborationsService.cancel(id), onSuccess: invalidate });

  return { create, respond, addComment, addFile, submitRevision, decideRevision, complete, cancel };
}
