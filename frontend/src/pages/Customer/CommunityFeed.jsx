import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { ProducerCard, DiscussionCard } from '../../components/cards';
import { communityPosts, producers, forumThreads } from '../../data/mockData';

export default function CommunityFeed() {
  return (
    <div>
      <PageHeader
        title="Community"
        description="Updates from the producers and heritage communities you follow."
        action={
          <div className="flex gap-2">
            <Link to={routePaths.customerForum} className="text-sm font-medium text-link hover:underline">
              Discussion Forum →
            </Link>
            <span className="text-body/30">|</span>
            <Link to={routePaths.customerQA} className="text-sm font-medium text-link hover:underline">
              Q&amp;A →
            </Link>
          </div>
        }
      />

      <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-4">
          {communityPosts.map((post) => (
            <div key={post.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex items-center gap-3">
                <Link
                  to={routePaths.customerProducerProfile.replace(':producerId', post.authorId)}
                  className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary"
                >
                  {post.author.slice(0, 1)}
                </Link>
                <div className="min-w-0 flex-1">
                  <Link
                    to={routePaths.customerProducerProfile.replace(':producerId', post.authorId)}
                    className="text-sm font-semibold text-heading hover:underline"
                  >
                    {post.author}
                  </Link>
                  <p className="text-xs text-body/60">
                    {post.craft} · {post.time}
                  </p>
                </div>
              </div>
              <p className="mt-3 text-sm text-body/80">{post.content}</p>
              <div className="mt-3 flex gap-4 text-xs text-body/60">
                <span>♥ {post.likes}</span>
                <span>💬 {post.comments} comments</span>
              </div>
            </div>
          ))}
        </div>

        <div className="space-y-4">
          <div className="space-y-4 rounded-xl border border-border bg-surface p-5">
            <p className="text-sm font-semibold text-heading">Producers to Follow</p>
            <div className="grid grid-cols-2 gap-3">
              {producers.slice(0, 4).map((producer) => (
                <ProducerCard
                  key={producer.id}
                  producer={producer}
                  to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
                />
              ))}
            </div>
            <Link to={routePaths.customerFollowing} className="block text-sm font-medium text-link hover:underline">
              View who you follow →
            </Link>
            <Badge tone="secondary">Tip: follow producers to see their updates here</Badge>
          </div>

          <div className="space-y-3 rounded-xl border border-border bg-surface p-5">
            <p className="text-sm font-semibold text-heading">Recent Discussions</p>
            <div className="space-y-3">
              {forumThreads.slice(0, 3).map((thread) => (
                <DiscussionCard key={thread.id} thread={thread} to={routePaths.customerForum} />
              ))}
            </div>
            <Link to={routePaths.customerForum} className="block text-sm font-medium text-link hover:underline">
              View discussion forum →
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
