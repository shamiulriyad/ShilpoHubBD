import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useQuizMutations, useMyQuizAttempts } from '../../hooks/useQuizzes';

export default function QuizDetails() {
  const { quizId } = useParams();
  const { startAttempt, submitAttempt } = useQuizMutations();
  const attemptsQuery = useMyQuizAttempts(quizId);
  const [attempt, setAttempt] = useState(null);
  const [answers, setAnswers] = useState({});
  const [result, setResult] = useState(null);

  const handleStart = () => {
    startAttempt.mutate(quizId, { onSuccess: (data) => setAttempt(data) });
  };

  const handleSubmit = () => {
    const payload = Object.entries(answers).map(([questionId, selectedOptionId]) => ({ questionId, selectedOptionId }));
    submitAttempt.mutate({ attemptId: attempt.id, answers: payload }, { onSuccess: (data) => setResult(data) });
  };

  if (result) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
        <PageHeader title={result.quizTitle} description="Quiz Result" />
        <div className="rounded-xl border border-border bg-surface p-6 text-center">
          <Badge tone={result.isPassed ? 'success' : 'neutral'}>{result.isPassed ? 'Passed' : 'Not Passed'}</Badge>
          <p className="mt-3 text-3xl font-semibold text-primary">{result.percentageScore?.toFixed(0)}%</p>
          <p className="mt-1 text-sm text-body/60">{result.score} / {result.maxScore} points</p>
        </div>
      </div>
    );
  }

  if (attempt) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
        <PageHeader title={attempt.quizTitle} description={attempt.timeLimitMinutes ? `Time limit: ${attempt.timeLimitMinutes} minutes` : undefined} />
        <div className="space-y-6">
          {attempt.questions.map((question, i) => (
            <div key={question.id} className="rounded-xl border border-border bg-surface p-5">
              <p className="text-sm font-semibold text-heading">
                {i + 1}. {question.body} <span className="text-xs font-normal text-body/50">({question.points} pts)</span>
              </p>
              <div className="mt-3 space-y-2">
                {question.options.map((option) => (
                  <label key={option.id} className="flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm">
                    <input
                      type="radio"
                      name={question.id}
                      checked={answers[question.id] === option.id}
                      onChange={() => setAnswers((prev) => ({ ...prev, [question.id]: option.id }))}
                    />
                    {option.text}
                  </label>
                ))}
              </div>
            </div>
          ))}
          <Button variant="primary" className="w-full" onClick={handleSubmit} disabled={submitAttempt.isPending}>
            {submitAttempt.isPending ? 'Submitting…' : 'Submit Quiz'}
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
      <PageHeader title="Quiz" description="Start your attempt when you're ready." />
      <AsyncState isLoading={attemptsQuery.isLoading} isError={attemptsQuery.isError} error={attemptsQuery.error}>
        {(attemptsQuery.data || []).length > 0 && (
          <div className="mb-6 space-y-2">
            <p className="text-sm font-semibold text-heading">Previous Attempts</p>
            {attemptsQuery.data.map((a) => (
              <div key={a.id} className="flex items-center justify-between rounded-lg border border-border bg-surface p-3 text-sm">
                <span>Attempt {a.attemptNumber}</span>
                <Badge tone={a.isPassed ? 'success' : 'neutral'}>
                  {a.percentageScore != null ? `${a.percentageScore.toFixed(0)}%` : a.status}
                </Badge>
              </div>
            ))}
          </div>
        )}
      </AsyncState>
      <Button variant="primary" onClick={handleStart} disabled={startAttempt.isPending}>
        {startAttempt.isPending ? 'Starting…' : 'Start Quiz'}
      </Button>
    </div>
  );
}
