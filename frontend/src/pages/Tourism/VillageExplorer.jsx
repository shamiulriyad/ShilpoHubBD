import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useVillageTourStops } from '../../hooks/useVillageTour';

export default function VillageExplorer() {
  const { data, isLoading, isError, error } = useVillageTourStops({ pageSize: 50 });
  const stops = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Village Explorer' },
        ]}
        title="Village Explorer"
        description="Immersive 360°/video stops from Bangladesh's heritage craft villages — explore before you visit."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {stops.map((stop) => (
            <div key={stop.id} className="overflow-hidden rounded-xl border border-border bg-surface">
              <div className="flex aspect-square items-center justify-center bg-background text-xs text-body/40">
                {stop.mediaType === 'Video' || stop.mediaType === 'Video360' ? (
                  <video src={stop.mediaUrl} poster={stop.thumbnailUrl} className="h-full w-full object-cover" controls />
                ) : stop.thumbnailUrl || stop.mediaUrl ? (
                  <img src={stop.thumbnailUrl || stop.mediaUrl} alt={stop.title} className="h-full w-full object-cover" />
                ) : (
                  stop.mediaType
                )}
              </div>
              <div className="space-y-1 p-3">
                <Badge tone="secondary">{stop.mediaType}</Badge>
                <p className="text-sm font-medium text-heading">{stop.title}</p>
                <p className="text-xs text-body/60">{stop.heritagePlaceName}</p>
              </div>
            </div>
          ))}
          {stops.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No village tour stops published yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
