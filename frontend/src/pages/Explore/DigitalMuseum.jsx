import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { useFeaturedProducts } from '../../hooks/queries/useCatalog';

export default function DigitalMuseum() {
  const query = useFeaturedProducts(16);

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
    </div>
  );
}
