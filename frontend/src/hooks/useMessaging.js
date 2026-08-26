import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { messagingService } from '../services/messagingService';

export function useConversations(params = {}) {
  return useQuery({
    queryKey: ['conversations', params],
    queryFn: () => messagingService.listConversations(params),
  });
}

export function useConversation(id) {
  return useQuery({
    queryKey: ['conversations', id],
    queryFn: () => messagingService.getConversation(id),
    enabled: Boolean(id),
  });
}

export function useMessagingMutations() {
  const queryClient = useQueryClient();

  const sendMessage = useMutation({
    mutationFn: ({ id, body }) => messagingService.sendMessage(id, body),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['conversations', id] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
  });

  const startConversation = useMutation({
    mutationFn: ({ recipientId, body }) => messagingService.startConversation(recipientId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations'] }),
  });

  const markAsRead = useMutation({
    mutationFn: (id) => messagingService.markAsRead(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations'] }),
  });

  return { sendMessage, startConversation, markAsRead };
}
