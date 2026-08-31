import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { enrollmentsService } from '../services/enrollmentsService';

export function useMyEnrollments(enabled = true) {
  return useQuery({ queryKey: ['enrollments', 'mine'], queryFn: () => enrollmentsService.mine(), enabled });
}

export function useEnrollment(id) {
  return useQuery({ queryKey: ['enrollments', id], queryFn: () => enrollmentsService.getById(id), enabled: Boolean(id) });
}

export function useEnrollmentMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['enrollments'] });

  const enroll = useMutation({ mutationFn: (courseId) => enrollmentsService.enroll(courseId), onSuccess: invalidate });
  const markProgress = useMutation({
    mutationFn: ({ id, lessonId, isCompleted }) => enrollmentsService.markProgress(id, lessonId, isCompleted),
    onSuccess: invalidate,
  });
  const complete = useMutation({ mutationFn: (id) => enrollmentsService.complete(id), onSuccess: invalidate });

  return { enroll, markProgress, complete };
}
