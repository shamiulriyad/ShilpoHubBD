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
    mutationFn: ({ id, body, imageUrl }) => messagingService.sendMessage(id, body, imageUrl),
    meta: { silent: true },
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['conversations', id] });
      queryClient.invalidateQueries({ queryKey: ['conversations'] });
    },
  });

  const startConversation = useMutation({
    mutationFn: ({ recipientId, body, imageUrl }) => messagingService.startConversation(recipientId, body, imageUrl),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations'] }),
  });

  const markAsRead = useMutation({
    mutationFn: (id) => messagingService.markAsRead(id),
    meta: { silent: true },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['conversations'] }),
  });

  return { sendMessage, startConversation, markAsRead };
}

export function useChatImageUpload() {
  return useMutation({ mutationFn: (file) => messagingService.uploadImage(file) });
}
