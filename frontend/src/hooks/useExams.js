import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { examsService } from '../services/examsService';

export function useCourseExams(courseId) {
  return useQuery({
    queryKey: ['exams', 'course', courseId],
    queryFn: () => examsService.listForCourse(courseId),
    enabled: Boolean(courseId),
  });
}

export function useExamAttempt(attemptId) {
  return useQuery({
    queryKey: ['exams', 'attempts', attemptId],
    queryFn: () => examsService.getAttempt(attemptId),
    enabled: Boolean(attemptId),
  });
}

export function useMyExamAttempts(examId) {
  return useQuery({
    queryKey: ['exams', examId, 'attempts', 'mine'],
    queryFn: () => examsService.myAttempts(examId),
    enabled: Boolean(examId),
  });
}

export function useExamMutations() {
  const queryClient = useQueryClient();

  const startAttempt = useMutation({ mutationFn: (examId) => examsService.startAttempt(examId) });
  const submitAttempt = useMutation({
    mutationFn: ({ attemptId, answers }) => examsService.submitAttempt(attemptId, answers),
    onSuccess: (_, { attemptId }) => {
      queryClient.invalidateQueries({ queryKey: ['exams', 'attempts', attemptId] });
    },
  });

  return { startAttempt, submitAttempt };
}
