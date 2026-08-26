import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useFollowedProducers, useProducerFollowMutations } from '../../hooks/useProducerFollows';

export default function FollowingProducers() {
  const { data, isLoading, isError, error } = useFollowedProducers();
  const { unfollow } = useProducerFollowMutations();
  const following = data || [];

  return (
    <div>
      <PageHeader
        title="Following"
        description={`You follow ${following.length} producer${following.length === 1 ? '' : 's'}. See their updates in your Community feed.`}
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {following.map((producer) => (
            <div key={producer.producerId} className="flex items-center gap-4 rounded-xl border border-border bg-surface p-4">
              <Link
                to={routePaths.customerProducerProfile.replace(':producerId', producer.producerId)}
                className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-primary/10 text-base font-semibold text-primary"
              >
                {producer.producerName.slice(0, 1)}
              </Link>
              <div className="min-w-0 flex-1">
                <Link
                  to={routePaths.customerProducerProfile.replace(':producerId', producer.producerId)}
                  className="truncate text-sm font-semibold text-heading hover:underline"
                >
                  {producer.producerName}
                </Link>
                <p className="truncate text-xs text-body/60">
                  Following since {new Date(producer.followedAt).toLocaleDateString()}
                </p>
              </div>
              <Button variant="secondary" onClick={() => unfollow.mutate(producer.producerId)}>
                Unfollow
              </Button>
            </div>
          ))}
          {following.length === 0 && (
            <p className="col-span-full text-sm text-body/60">You aren't following any producers yet.</p>
          )}
        </AsyncState>
      </div>
    </div>
  );
}
