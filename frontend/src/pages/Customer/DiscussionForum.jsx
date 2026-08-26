import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Table, Button, AsyncState, Pagination } from '../../components/ui';
import { useDiscussions, useDiscussionMutations } from '../../hooks/useDiscussions';

export default function DiscussionForum() {
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ title: '', category: '', body: '' });
  const { data, isLoading, isError, error } = useDiscussions({ page, pageSize: 10 });
  const { create } = useDiscussionMutations();

  const threads = data?.items || [];
  const rows = threads.map((thread) => ({
    Thread: (
      <div>
        <p className="font-medium text-heading">{thread.title}</p>
        <p className="text-xs text-body/60">by {thread.authorName}</p>
      </div>
    ),
    Category: <Badge tone="secondary">{thread.category}</Badge>,
    Replies: thread.replyCount,
    'Last Activity': new Date(thread.createdAt).toLocaleDateString(),
  }));

  const handleSubmit = (event) => {
    event.preventDefault();
    create.mutate(form, {
      onSuccess: () => {
        setForm({ title: '', category: '', body: '' });
        setShowForm(false);
      },
    });
  };

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
          <div className="flex items-center gap-3">
            <Link to={routePaths.customerQA} className="text-sm font-medium text-link hover:underline">
              Go to Q&amp;A →
            </Link>
            <Button variant="primary" onClick={() => setShowForm((prev) => !prev)}>
              {showForm ? 'Cancel' : 'New Thread'}
            </Button>
          </div>
        }
      />

      {showForm && (
        <form onSubmit={handleSubmit} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
          <input
            required
            placeholder="Title"
            value={form.title}
            onChange={(event) => setForm((prev) => ({ ...prev, title: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <input
            required
            placeholder="Category (e.g. Shipping, Authenticity)"
            value={form.category}
            onChange={(event) => setForm((prev) => ({ ...prev, category: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <textarea
            required
            rows={3}
            placeholder="What's on your mind?"
            value={form.body}
            onChange={(event) => setForm((prev) => ({ ...prev, body: event.target.value }))}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <Button type="submit" variant="primary" disabled={create.isPending}>
            {create.isPending ? 'Posting…' : 'Post Thread'}
          </Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <Table columns={['Thread', 'Category', 'Replies', 'Last Activity']} rows={rows} />
        {threads.length === 0 && <p className="mt-4 text-center text-sm text-body/60">No discussions yet — start one.</p>}
      </AsyncState>

      {data?.totalPages > 1 && (
        <div className="mt-6">
          <Pagination currentPage={page} totalPages={data.totalPages} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
}
