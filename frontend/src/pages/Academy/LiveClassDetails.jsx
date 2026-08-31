import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { VideoPlayer } from '../../components/media';
import { useLiveClass, useLiveClassMutations, useMyRegisteredLiveClasses } from '../../hooks/useLiveClasses';
import { useAuth } from '../../hooks/useAuth';

export default function LiveClassDetails() {
  const { liveClassId } = useParams();
  const { isAuthenticated, user } = useAuth();
  const classQuery = useLiveClass(liveClassId);
  const registeredQuery = useMyRegisteredLiveClasses();
  const { register, join, askQuestion } = useLiveClassMutations(liveClassId);
  const [question, setQuestion] = useState('');

  const liveClass = classQuery.data;
  const isRegistered = (registeredQuery.data || []).some((c) => c.id === liveClassId);
  const isInstructor = liveClass && user?.id === liveClass.instructorUserId;

  const handleAsk = (event) => {
    event.preventDefault();
    askQuestion.mutate(question, { onSuccess: () => setQuestion('') });
  };

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={classQuery.isLoading} isError={classQuery.isError} error={classQuery.error}>
        {liveClass && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Academy', path: routePaths.academy },
                { label: 'Live Classes', path: routePaths.academyLiveClasses },
                { label: liveClass.title },
              ]}
              title={liveClass.title}
              description={`By ${liveClass.instructorName}`}
              action={<Badge tone={liveClass.status === 'Live' ? 'success' : 'secondary'}>{liveClass.status}</Badge>}
            />

            <VideoPlayer title={liveClass.title} live={liveClass.status === 'Live'} className="mb-6" />

            <p className="mb-6 text-sm text-body/70">{liveClass.description}</p>

            {isAuthenticated && !isInstructor && (
              <div className="mb-6 flex flex-wrap gap-3">
                {!isRegistered ? (
                  <Button variant="primary" onClick={() => register.mutate()} disabled={register.isPending}>
                    {register.isPending ? 'Registering…' : 'Register'}
                  </Button>
                ) : liveClass.status === 'Live' ? (
                  <Button variant="primary" onClick={() => join.mutate()}>
                    Join Now
                  </Button>
                ) : (
                  <Badge tone="success">Registered</Badge>
                )}
              </div>
            )}
            {!isAuthenticated && (
              <p className="mb-6 text-xs text-body/50">
                <Link to={routePaths.login} className="text-link hover:underline">Log in</Link> to register for this class.
              </p>
            )}

            <p className="mb-3 text-sm font-semibold text-heading">Q&A</p>
            {isAuthenticated && liveClass.status === 'Live' && (isRegistered || isInstructor) && (
              <form onSubmit={handleAsk} className="mb-4 flex gap-2">
                <input
                  required
                  placeholder="Ask a question…"
                  value={question}
                  onChange={(event) => setQuestion(event.target.value)}
                  className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
                />
                <Button type="submit" variant="primary" disabled={askQuestion.isPending}>
                  Ask
                </Button>
              </form>
            )}
            <div className="space-y-3">
              {liveClass.questions.map((q) => (
                <div key={q.id} className="rounded-lg border border-border bg-surface p-3">
                  <p className="text-sm">
                    <span className="font-medium text-heading">{q.userName}: </span>
                    {q.body}
                  </p>
                  {q.isAnswered && <p className="mt-1 text-sm text-body/70">↳ {q.answerBody}</p>}
                </div>
              ))}
              {liveClass.questions.length === 0 && <p className="text-sm text-body/60">No questions yet.</p>}
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
