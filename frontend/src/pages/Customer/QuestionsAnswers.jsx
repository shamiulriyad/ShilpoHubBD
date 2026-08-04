import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button } from '../../components/ui';
import { qaThreads } from '../../data/mockData';

export default function QuestionsAnswers() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Community', path: routePaths.customerCommunity },
          { label: 'Questions & Answers' },
        ]}
        title="Questions & Answers"
        description="Get answers straight from verified producers about materials, technique and authenticity."
        action={
          <div className="flex items-center gap-3">
            <Link to={routePaths.customerForum} className="text-sm font-medium text-link hover:underline">
              Discussion Forum →
            </Link>
            <Button variant="primary">Ask a Question</Button>
          </div>
        }
      />

      <div className="space-y-5">
        {qaThreads.map((qa) => (
          <div key={qa.id} className="rounded-xl border border-border bg-surface p-5">
            <div className="mb-2 flex flex-wrap items-center gap-2">
              <Badge tone="primary">{qa.craft}</Badge>
              <span className="text-xs text-body/50">
                Asked by {qa.askedBy} · {qa.time}
              </span>
            </div>
            <p className="text-sm font-semibold text-heading">{qa.question}</p>

            <div className="mt-4 space-y-3 border-t border-border pt-4">
              {qa.answers.map((answer) => (
                <div key={answer.id} className="flex gap-3">
                  <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                    {answer.author.slice(0, 1)}
                  </span>
                  <div>
                    <p className="text-sm">
                      <span className="font-medium text-heading">{answer.author}</span>
                      {answer.isProducer && <Badge tone="success" className="ml-2">Verified Producer</Badge>}
                    </p>
                    <p className="mt-1 text-sm text-body/70">{answer.body}</p>
                    <p className="mt-1 text-xs text-body/50">▲ {answer.votes} helpful</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
