import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { liveClassesService } from '../services/liveClassesService';

export function useLiveClasses(params = {}) {
  return useQuery({ queryKey: ['live-classes', params], queryFn: () => liveClassesService.list(params) });
}

export function useLiveClass(id) {
  return useQuery({ queryKey: ['live-classes', id], queryFn: () => liveClassesService.getById(id), enabled: Boolean(id) });
}

export function useMyRegisteredLiveClasses() {
  return useQuery({ queryKey: ['live-classes', 'registered'], queryFn: () => liveClassesService.registered() });
}

export function useLiveClassMutations(id) {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['live-classes', id] });
    queryClient.invalidateQueries({ queryKey: ['live-classes', 'registered'] });
  };

  const register = useMutation({ mutationFn: () => liveClassesService.register(id), onSuccess: invalidate });
  const join = useMutation({ mutationFn: () => liveClassesService.join(id) });
  const leave = useMutation({ mutationFn: () => liveClassesService.leave(id) });
  const askQuestion = useMutation({
    mutationFn: (body) => liveClassesService.askQuestion(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live-classes', id] }),
  });

  return { register, join, leave, askQuestion };
}
