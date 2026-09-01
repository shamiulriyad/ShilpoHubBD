import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { liveEventsService } from '../services/liveEventsService';

export function useLiveEvents(params = {}) {
  return useQuery({
    queryKey: ['live-events', params],
    queryFn: () => liveEventsService.list(params),
    placeholderData: (previousData) => previousData,
  });
}

export function useLiveEvent(id) {
  return useQuery({
    queryKey: ['live-events', id],
    queryFn: () => liveEventsService.getById(id),
    enabled: Boolean(id),
  });
}

export function useMyLiveEventMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['live-events'] });

  return {
    create: useMutation({ mutationFn: (payload) => liveEventsService.create(payload), onSuccess: invalidate }),
    start: useMutation({ mutationFn: (id) => liveEventsService.start(id), onSuccess: invalidate }),
    end: useMutation({ mutationFn: (id) => liveEventsService.end(id), onSuccess: invalidate }),
  };
}

export function useLiveEventInteractions(id) {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['live-events', id] });

  const addComment = useMutation({
    mutationFn: (body) => liveEventsService.addComment(id, body),
    onSuccess: invalidate,
  });
  const addReaction = useMutation({
    mutationFn: (type) => liveEventsService.addReaction(id, type),
    onSuccess: invalidate,
  });
  const buyDuringLive = useMutation({
    mutationFn: (payload) => liveEventsService.buyDuringLive(id, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  });

  return { addComment, addReaction, buyDuringLive };
}
