import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { producers, followedProducerIds } from '../../data/mockData';

export default function FollowingProducers() {
  const following = producers.filter((p) => followedProducerIds.includes(p.id));

  return (
    <div>
      <PageHeader
        title="Following"
        description={`You follow ${following.length} producer${following.length === 1 ? '' : 's'}. See their updates in your Community feed.`}
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {following.map((producer) => (
          <div key={producer.id} className="flex items-center gap-4 rounded-xl border border-border bg-surface p-4">
            <Link
              to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
              className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-primary/10 text-base font-semibold text-primary"
            >
              {producer.name.slice(0, 1)}
            </Link>
            <div className="min-w-0 flex-1">
              <Link
                to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
                className="truncate text-sm font-semibold text-heading hover:underline"
              >
                {producer.name}
              </Link>
              <p className="truncate text-xs text-body/60">
                {producer.craft} · {producer.district}
              </p>
            </div>
            <Button variant="secondary">Unfollow</Button>
          </div>
        ))}
      </div>
    </div>
  );
}
