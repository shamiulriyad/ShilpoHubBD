import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, QnASection } from '../../components/ui';
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
          <QnASection key={qa.id} qa={qa} />
        ))}
      </div>
    </div>
  );
}
