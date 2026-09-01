import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { knowledgeGraphService } from '../services/knowledgeGraphService';

export function useKnowledgeNodes(params = {}) {
  return useQuery({ queryKey: ['knowledge-nodes', params], queryFn: () => knowledgeGraphService.listNodes(params) });
}

export function useKnowledgeNeighbors(id) {
  return useQuery({ queryKey: ['knowledge-nodes', id, 'neighbors'], queryFn: () => knowledgeGraphService.getNeighbors(id), enabled: Boolean(id) });
}

export function useKnowledgePath(params) {
  return useQuery({
    queryKey: ['knowledge-path', params],
    queryFn: () => knowledgeGraphService.findPath(params),
    enabled: Boolean(params?.sourceNodeId && params?.targetNodeId),
  });
}

export function useKnowledgeGraphMutations() {
  const queryClient = useQueryClient();
  const invalidateNodes = () => queryClient.invalidateQueries({ queryKey: ['knowledge-nodes'] });

  return {
    createNode: useMutation({ mutationFn: (payload) => knowledgeGraphService.createNode(payload), onSuccess: invalidateNodes }),
    removeNode: useMutation({ mutationFn: (id) => knowledgeGraphService.removeNode(id), onSuccess: invalidateNodes }),
    createRelationship: useMutation({
      mutationFn: (payload) => knowledgeGraphService.createRelationship(payload),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['knowledge-nodes'] }),
    }),
    removeRelationship: useMutation({
      mutationFn: (id) => knowledgeGraphService.removeRelationship(id),
      onSuccess: () => queryClient.invalidateQueries({ queryKey: ['knowledge-nodes'] }),
    }),
  };
}
