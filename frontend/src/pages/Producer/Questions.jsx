import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { useProducerQuestions, useAnswerQuestion } from '../../hooks/useQuestions';
import { resolveUploadUrl } from '../../components/messaging/ImageAttachButton';

function QuestionCard({ question }) {
  const [reply, setReply] = useState('');
  const answer = useAnswerQuestion();
  const answered = question.answers.length > 0;

  const submit = (event) => {
    event.preventDefault();
    if (!reply.trim()) return;
    answer.mutate({ id: question.id, body: reply.trim() }, { onSuccess: () => setReply('') });
  };

  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <div className="mb-1 flex flex-wrap items-center justify-between gap-2">
        <p className="text-xs text-body/60">
          {question.productName || 'Product'} · asked by {question.askerName} · {new Date(question.createdAt).toLocaleDateString()}
        </p>
        <Badge tone={answered ? 'success' : 'secondary'}>{answered ? 'Answered' : 'Needs answer'}</Badge>
      </div>
      <p className="text-sm font-semibold text-heading">{question.body}</p>
      {question.imageUrl && (
        <a href={resolveUploadUrl(question.imageUrl)} target="_blank" rel="noreferrer">
          <img src={resolveUploadUrl(question.imageUrl)} alt="Attached by the customer" className="mt-2 max-h-48 rounded-lg object-cover" loading="lazy" />
        </a>
      )}

      {answered && (
        <div className="mt-3 space-y-2 border-t border-border pt-3">
          {question.answers.map((a) => (
            <p key={a.id} className="text-sm text-body/80">
              <span className="font-medium text-heading">{a.authorName}:</span> {a.body}
            </p>
          ))}
        </div>
      )}

      <form onSubmit={submit} className="mt-3 flex flex-col gap-2 sm:flex-row">
        <textarea
          aria-label="Your answer"
          rows={2}
          required
          maxLength={2000}
          placeholder={answered ? 'Add another reply…' : 'Write your answer…'}
          value={reply}
          onChange={(e) => setReply(e.target.value)}
          className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <Button type="submit" variant="primary" disabled={answer.isPending || !reply.trim()}>
          {answer.isPending ? 'Sending…' : 'Reply'}
        </Button>
      </form>
      <MutationFeedback mutation={answer} successMessage="Reply sent. The customer has been notified." />
    </div>
  );
}

export default function ProducerQuestions() {
  const [unansweredOnly, setUnansweredOnly] = useState(true);
  const { data, isLoading, isError, error } = useProducerQuestions({ unansweredOnly, pageSize: 50 });
  const questions = data?.items || [];

  return (
    <div>
      <PageHeader
        title="Customer Questions"
        description="Questions customers asked about your products. Answer them here — the customer is notified."
        action={
          <label className="flex items-center gap-2 text-sm text-body/70">
            <input type="checkbox" checked={unansweredOnly} onChange={(e) => setUnansweredOnly(e.target.checked)} />
            Only unanswered
          </label>
        }
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-3">
          {questions.map((q) => <QuestionCard key={q.id} question={q} />)}
          {questions.length === 0 && (
            <p className="text-sm text-body/60">{unansweredOnly ? 'No unanswered questions. Nice work!' : 'No customer questions yet.'}</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
