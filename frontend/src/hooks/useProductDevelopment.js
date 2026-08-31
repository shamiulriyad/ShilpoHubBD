import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { productDevelopmentService } from '../services/productDevelopmentService';

export function useMyDevelopmentProjects(params = {}) {
  return useQuery({ queryKey: ['product-development', 'mine', params], queryFn: () => productDevelopmentService.mine(params) });
}

export function useReceivedDevelopmentProjects(params = {}) {
  return useQuery({ queryKey: ['product-development', 'received', params], queryFn: () => productDevelopmentService.received(params) });
}

export function useDevelopmentProject(id) {
  return useQuery({ queryKey: ['product-development', id], queryFn: () => productDevelopmentService.getById(id), enabled: Boolean(id) });
}

export function useProductDevelopmentMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['product-development'] });

  const create = useMutation({ mutationFn: (payload) => productDevelopmentService.create(payload), onSuccess: invalidate });
  const respond = useMutation({ mutationFn: ({ id, accept }) => productDevelopmentService.respond(id, accept), onSuccess: invalidate });
  const addComment = useMutation({ mutationFn: ({ id, content }) => productDevelopmentService.addComment(id, content), onSuccess: invalidate });
  const addMilestone = useMutation({
    mutationFn: ({ id, payload }) => productDevelopmentService.addMilestone(id, payload),
    onSuccess: invalidate,
  });
  const updateMilestoneStatus = useMutation({
    mutationFn: ({ id, milestoneId, status }) => productDevelopmentService.updateMilestoneStatus(id, milestoneId, status),
    onSuccess: invalidate,
  });
  const submitPrototype = useMutation({
    mutationFn: ({ id, payload }) => productDevelopmentService.submitPrototype(id, payload),
    onSuccess: invalidate,
  });
  const decidePrototype = useMutation({
    mutationFn: ({ id, prototypeVersionId, payload }) => productDevelopmentService.decidePrototype(id, prototypeVersionId, payload),
    onSuccess: invalidate,
  });
  const convertToProduct = useMutation({
    mutationFn: ({ id, payload }) => productDevelopmentService.convertToProduct(id, payload),
    onSuccess: invalidate,
  });
  const cancel = useMutation({ mutationFn: (id) => productDevelopmentService.cancel(id), onSuccess: invalidate });

  return { create, respond, addComment, addMilestone, updateMilestoneStatus, submitPrototype, decidePrototype, convertToProduct, cancel };
}
