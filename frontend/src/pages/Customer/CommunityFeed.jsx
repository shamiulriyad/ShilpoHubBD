import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { DiscussionCard } from '../../components/cards';
import { useDiscussions } from '../../hooks/useDiscussions';
import { useFollowedProducers } from '../../hooks/useProducerFollows';

export default function CommunityFeed() {
  const discussionsQuery = useDiscussions({ pageSize: 6 });
  const followsQuery = useFollowedProducers();
  const followedProducers = followsQuery.data || [];

  return (
    <div>
      <PageHeader
        title="Community"
        description="Keep up with producers you follow and join active heritage discussions."
        action={
          <div className="flex flex-wrap gap-2 text-sm">
            <Link to={routePaths.customerForum} className="font-medium text-link hover:underline">
              Discussion Forum →
            </Link>
            <span aria-hidden="true" className="text-body/30">|</span>
            <Link to={routePaths.customerQA} className="font-medium text-link hover:underline">
              Q&amp;A →
            </Link>
          </div>
        }
      />

      <div className="grid gap-8 lg:grid-cols-[minmax(0,2fr)_minmax(260px,1fr)]">
        <section aria-labelledby="community-discussions-heading" className="space-y-4">
          <div>
            <h2 id="community-discussions-heading" className="text-base font-semibold text-heading">
              Recent discussions
            </h2>
            <p className="mt-1 text-sm text-body/60">Conversations from the live community discussion service.</p>
          </div>

          <AsyncState
            isLoading={discussionsQuery.isLoading}
            isError={discussionsQuery.isError}
            error={discussionsQuery.error}
          >
            <div className="space-y-3">
              {(discussionsQuery.data?.items || []).map((thread) => (
                <DiscussionCard
                  key={thread.id}
                  thread={{
                    title: thread.title,
                    category: thread.category,
                    author: thread.authorName,
                    replies: thread.replyCount,
                    lastActivity: thread.createdAt ? new Date(thread.createdAt).toLocaleDateString() : '—',
                  }}
                  to={routePaths.customerForum}
                />
              ))}
              {(discussionsQuery.data?.items || []).length === 0 && !discussionsQuery.isLoading && (
                <div className="rounded-xl border border-dashed border-border bg-surface p-6 text-sm text-body/60">
                  No discussions are available yet. Start from the Discussion Forum when you have something to share.
                </div>
              )}
            </div>
          </AsyncState>
        </section>

        <aside className="space-y-4">
          <div className="rounded-xl border border-border bg-surface p-5">
            <div className="flex items-center justify-between gap-3">
              <h2 className="text-sm font-semibold text-heading">Following producers</h2>
              <Link to={routePaths.customerFollowing} className="text-xs font-medium text-link hover:underline">
                Manage →
              </Link>
            </div>

            <AsyncState isLoading={followsQuery.isLoading} isError={followsQuery.isError} error={followsQuery.error}>
              <div className="mt-4 space-y-3">
                {followedProducers.slice(0, 6).map((producer) => (
                  <Link
                    key={producer.producerId}
                    to={routePaths.customerProducerProfile.replace(':producerId', producer.producerId)}
                    className="flex items-center gap-3 rounded-lg border border-border px-3 py-2 transition hover:bg-background"
                  >
                    <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                      {(producer.producerName || 'P').slice(0, 1).toUpperCase()}
                    </span>
                    <span className="min-w-0">
                      <span className="block truncate text-sm font-medium text-heading">{producer.producerName || 'Producer'}</span>
                      <span className="block text-xs text-body/50">
                        {producer.followedAt ? `Following since ${new Date(producer.followedAt).toLocaleDateString()}` : 'Following'}
                      </span>
                    </span>
                  </Link>
                ))}

                {followedProducers.length === 0 && !followsQuery.isLoading && (
                  <p className="text-sm text-body/60">
                    You are not following any producers yet. Open a producer profile from the marketplace to follow one.
                  </p>
                )}
              </div>
            </AsyncState>
          </div>
        </aside>
      </div>
    </div>
  );
}
