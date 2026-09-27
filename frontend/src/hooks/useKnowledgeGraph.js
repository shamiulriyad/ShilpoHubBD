import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { knowledgeGraphService } from '../services/knowledgeGraphService';

export function useKnowledgeNodes(params = {}) {
  return useQuery({ queryKey: ['knowledge-nodes', params], queryFn: () => knowledgeGraphService.listNodes(params) });
}

export function useKnowledgeNeighbors(id) {
  return useQuery({ queryKey: ['knowledge-nodes', id, 'neighbors'], queryFn: () => knowledgeGraphService.getNeighbors(id), enabled: Boolean(id) });
}

export function useKnowledgeNetwork(network, params = {}) {
  return useQuery({ queryKey: ['knowledge-network', network, params], queryFn: () => knowledgeGraphService.getNetwork(network, params), enabled: Boolean(network) });
}

export function useKnowledgePath(params) {
  return useQuery({
    queryKey: ['knowledge-path', params],
    queryFn: () => knowledgeGraphService.findPath(params),
    enabled: Boolean(params?.sourceNodeId && params?.targetNodeId),
  });
}

export function useKnowledgeStats() {
  return useQuery({ queryKey: ['knowledge-stats'], queryFn: knowledgeGraphService.getStats });
}

export function useKnowledgeRules() {
  return useQuery({ queryKey: ['knowledge-rules'], queryFn: knowledgeGraphService.getRules });
}

export function useKnowledgeEntities(params) {
  return useQuery({
    queryKey: ['knowledge-entities', params],
    queryFn: () => knowledgeGraphService.searchEntities(params),
    enabled: Boolean(params?.nodeType),
  });
}

export function useKnowledgeGraphMutations() {
  const queryClient = useQueryClient();
  const invalidateGraph = () => {
    queryClient.invalidateQueries({ queryKey: ['knowledge-nodes'] });
    queryClient.invalidateQueries({ queryKey: ['knowledge-network'] });
    queryClient.invalidateQueries({ queryKey: ['knowledge-stats'] });
  };

  return {
    createNode: useMutation({ mutationFn: (payload) => knowledgeGraphService.createNode(payload), onSuccess: invalidateGraph }),
    importNode: useMutation({ mutationFn: (payload) => knowledgeGraphService.importNode(payload), onSuccess: invalidateGraph }),
    updateNode: useMutation({ mutationFn: ({ id, payload }) => knowledgeGraphService.updateNode(id, payload), onSuccess: invalidateGraph }),
    removeNode: useMutation({ mutationFn: (id) => knowledgeGraphService.removeNode(id), onSuccess: invalidateGraph }),
    createRelationship: useMutation({
      mutationFn: (payload) => knowledgeGraphService.createRelationship(payload),
      onSuccess: invalidateGraph,
    }),
    removeRelationship: useMutation({
      mutationFn: (id) => knowledgeGraphService.removeRelationship(id),
      onSuccess: invalidateGraph,
    }),
  };
}
