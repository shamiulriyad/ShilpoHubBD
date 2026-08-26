import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { useMuseumItems } from '../../hooks/useMuseumItems';

export default function DigitalMuseum() {
  const { data, isLoading, isError, error } = useMuseumItems({ pageSize: 24 });
  const items = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Digital Museum' },
        ]}
        title="Digital Museum"
        description="A curated digital collection of heritage artefacts and craft pieces."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {items.map((item) => (
            <div key={item.id} className="overflow-hidden rounded-xl border border-border bg-surface">
              <div className="flex aspect-square items-center justify-center bg-background text-xs text-body/40">
                {item.coverImageUrl ? (
                  <img src={item.coverImageUrl} alt={item.title} className="h-full w-full object-cover" />
                ) : (
                  'Museum Piece'
                )}
              </div>
              <div className="p-3">
                <p className="text-sm font-medium text-heading">{item.title}</p>
                <p className="text-xs text-body/60">
                  {item.category}
                  {item.era ? ` · ${item.era}` : ''}
                </p>
              </div>
            </div>
          ))}
          {items.length === 0 && <p className="col-span-full text-sm text-body/60">No museum items published yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
