import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { quizzesService } from '../services/quizzesService';

export function useCourseQuizzes(courseId) {
  return useQuery({
    queryKey: ['quizzes', 'course', courseId],
    queryFn: () => quizzesService.listForCourse(courseId),
    enabled: Boolean(courseId),
  });
}

export function useQuizAttempt(attemptId) {
  return useQuery({
    queryKey: ['quizzes', 'attempts', attemptId],
    queryFn: () => quizzesService.getAttempt(attemptId),
    enabled: Boolean(attemptId),
  });
}

export function useMyQuizAttempts(quizId) {
  return useQuery({
    queryKey: ['quizzes', quizId, 'attempts', 'mine'],
    queryFn: () => quizzesService.myAttempts(quizId),
    enabled: Boolean(quizId),
  });
}

export function useQuizMutations() {
  const queryClient = useQueryClient();

  const startAttempt = useMutation({ mutationFn: (quizId) => quizzesService.startAttempt(quizId) });
  const submitAttempt = useMutation({
    mutationFn: ({ attemptId, answers }) => quizzesService.submitAttempt(attemptId, answers),
    onSuccess: (_, { attemptId }) => {
      queryClient.invalidateQueries({ queryKey: ['quizzes', 'attempts', attemptId] });
    },
  });

  return { startAttempt, submitAttempt };
}
