import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { assignmentsService } from '../services/assignmentsService';

export function useCourseAssignments(courseId) {
  return useQuery({
    queryKey: ['assignments', 'course', courseId],
    queryFn: () => assignmentsService.listForCourse(courseId),
    enabled: Boolean(courseId),
  });
}

export function useMySubmission(assignmentId) {
  return useQuery({
    queryKey: ['assignments', assignmentId, 'my-submission'],
    queryFn: () => assignmentsService.mySubmission(assignmentId),
    enabled: Boolean(assignmentId),
    retry: false,
  });
}

export function useSubmitAssignment(assignmentId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload) => assignmentsService.submit(assignmentId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['assignments', assignmentId] }),
  });
}
