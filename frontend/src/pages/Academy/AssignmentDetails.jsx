import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { PageHeader, Badge, Button } from '../../components/ui';
import { useMySubmission, useSubmitAssignment } from '../../hooks/useAssignments';

export default function AssignmentDetails() {
  const { assignmentId } = useParams();
  const submissionQuery = useMySubmission(assignmentId);
  const submit = useSubmitAssignment(assignmentId);
  const [form, setForm] = useState({ submissionText: '', attachmentUrl: '' });

  const submission = submissionQuery.data;

  const handleSubmit = (event) => {
    event.preventDefault();
    submit.mutate({ submissionText: form.submissionText, attachmentUrl: form.attachmentUrl || undefined });
  };

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
      <PageHeader title="Assignment" description="Submit your work for grading." />

      {submissionQuery.isLoading ? (
        <p className="py-10 text-center text-sm text-body/60">Loading…</p>
      ) : (
        submission ? (
          <div className="rounded-xl border border-border bg-surface p-6">
            <div className="flex items-center justify-between">
              <p className="text-sm font-semibold text-heading">{submission.assignmentTitle}</p>
              <Badge tone={submission.status === 'Graded' ? 'success' : 'secondary'}>{submission.status}</Badge>
            </div>
            <p className="mt-3 text-sm text-body/70">{submission.submissionText}</p>
            {submission.attachmentUrl && (
              <a href={submission.attachmentUrl} target="_blank" rel="noreferrer" className="mt-2 inline-block text-sm text-link hover:underline">
                View attachment →
              </a>
            )}
            {submission.status === 'Graded' && (
              <div className="mt-4 border-t border-border pt-4">
                <p className="text-2xl font-semibold text-primary">
                  {submission.score} / {submission.maxScore}
                </p>
                {submission.feedback && <p className="mt-2 text-sm text-body/70">{submission.feedback}</p>}
              </div>
            )}
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4 rounded-xl border border-border bg-surface p-6">
            <textarea
              required
              rows={6}
              placeholder="Write your submission…"
              value={form.submissionText}
              onChange={(event) => setForm((prev) => ({ ...prev, submissionText: event.target.value }))}
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
            <input
              placeholder="Attachment URL (optional)"
              value={form.attachmentUrl}
              onChange={(event) => setForm((prev) => ({ ...prev, attachmentUrl: event.target.value }))}
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
            <Button type="submit" variant="primary" disabled={submit.isPending}>
              {submit.isPending ? 'Submitting…' : 'Submit Assignment'}
            </Button>
          </form>
        )
      )}
    </div>
  );
}
