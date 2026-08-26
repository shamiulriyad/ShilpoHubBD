import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { useLocalCuisines } from '../../hooks/useLocalCuisines';

export default function LocalCuisines() {
  const { data, isLoading, isError, error } = useLocalCuisines({ pageSize: 50 });
  const cuisines = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Local Cuisine' },
        ]}
        title="Local Cuisine"
        description="Traditional dishes to try on your heritage journey, and where to find them."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {cuisines.map((cuisine) => (
            <div key={cuisine.id} className="overflow-hidden rounded-xl border border-border bg-surface">
              <div className="flex aspect-[4/3] items-center justify-center bg-background text-xs text-body/40">
                {cuisine.imageUrl ? (
                  <img src={cuisine.imageUrl} alt={cuisine.name} className="h-full w-full object-cover" />
                ) : (
                  'Dish Photo'
                )}
              </div>
              <div className="space-y-1.5 p-4">
                <p className="text-sm font-semibold text-heading">{cuisine.name}</p>
                <p className="text-xs text-body/60">{cuisine.districtName}{cuisine.heritagePlaceName ? ` · ${cuisine.heritagePlaceName}` : ''}</p>
                <p className="text-sm text-body/70">{cuisine.description}</p>
                {cuisine.whereToTry && (
                  <p className="text-xs text-body/50">
                    <span className="font-medium text-heading">Where to try: </span>
                    {cuisine.whereToTry}
                  </p>
                )}
              </div>
            </div>
          ))}
          {cuisines.length === 0 && <p className="col-span-full text-sm text-body/60">No local cuisine entries yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
