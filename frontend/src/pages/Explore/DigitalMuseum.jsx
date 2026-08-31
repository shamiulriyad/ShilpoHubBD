import { routePaths } from '../../routes/routePaths';
<<<<<<< HEAD
import { PageHeader, QueryState } from '../../components/ui';
import { useFeaturedProducts } from '../../hooks/queries/useCatalog';

export default function DigitalMuseum() {
  const query = useFeaturedProducts(16);
=======
import { PageHeader, AsyncState } from '../../components/ui';
import { useMuseumItems } from '../../hooks/useMuseumItems';

export default function DigitalMuseum() {
  const { data, isLoading, isError, error } = useMuseumItems({ pageSize: 24 });
  const items = data?.items || [];
>>>>>>> origin/main

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
<<<<<<< HEAD
      <QueryState query={query} emptyLabel="The curated collection is being prepared.">
        {(items) => (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {items.map((item) => (
              <a
                key={item.id}
                href={routePaths.marketplaceProductDetails.replace(':productId', item.id)}
                className="group overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
              >
                {item.primaryImageUrl ? (
                  <img
                    src={item.primaryImageUrl}
                    alt={item.name}
                    className="aspect-square w-full object-cover"
                  />
                ) : (
                  <div className="flex aspect-square items-center justify-center bg-background text-xs text-body/40">
                    Museum Piece
                  </div>
                )}
                <div className="p-3">
                  <p className="text-sm font-medium text-heading group-hover:text-primary">{item.name}</p>
                  <p className="text-xs text-body/60">{item.categoryName}</p>
                </div>
              </a>
            ))}
          </div>
        )}
      </QueryState>
=======
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
>>>>>>> origin/main
    </div>
  );
}
