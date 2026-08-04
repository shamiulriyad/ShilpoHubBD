import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Table } from '../../components/ui';
import { forumThreads } from '../../data/mockData';

export default function DiscussionForum() {
  const rows = forumThreads.map((thread) => ({
    Thread: (
      <div>
        <p className="font-medium text-heading">{thread.title}</p>
        <p className="text-xs text-body/60">by {thread.author}</p>
      </div>
    ),
    Category: <Badge tone="secondary">{thread.category}</Badge>,
    Replies: thread.replies,
    Views: thread.views,
    'Last Activity': thread.lastActivity,
  }));

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Community', path: routePaths.customerCommunity },
          { label: 'Discussion Forum' },
        ]}
        title="Discussion Forum"
        description="Ask questions, share tips and discuss heritage crafts with the community."
        action={
          <Link to={routePaths.customerQA} className="text-sm font-medium text-link hover:underline">
            Go to Q&amp;A →
          </Link>
        }
      />

      <Table columns={['Thread', 'Category', 'Replies', 'Views', 'Last Activity']} rows={rows} />
    </div>
  );
}
