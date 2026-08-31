import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useVillage, useProducts } from '../../hooks/queries/useCatalog';
import { mapProduct } from '../../utils/mappers';

export default function VillageDetails() {
  const { villageId } = useParams();
  const villageQuery = useVillage(villageId);
  const village = villageQuery.data;

  // No village-scoped product endpoint yet; fall back to the village's district.
  const productsQuery = useProducts(
    village?.districtId ? { districtId: village.districtId, pageSize: 8 } : {},
  );

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <QueryState query={villageQuery} loadingLabel="Loading village…" emptyLabel="Village not found.">
        {(v) => (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Heritage Villages', path: routePaths.exploreVillages },
                { label: v.name },
              ]}
              title={v.name}
              description={`${v.craft}${v.districtName ? ` · ${v.districtName}` : ''}`}
            />

            {v.imageUrl ? (
              <img
                src={v.imageUrl}
                alt={v.name}
                className="mb-10 aspect-[21/9] w-full rounded-2xl border border-border object-cover"
              />
            ) : (
              <div className="mb-10 flex aspect-[21/9] items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
                No gallery image yet
              </div>
            )}

            {v.description && <p className="mb-10 max-w-3xl text-sm text-body/70">{v.description}</p>}

            <p className="mb-3 text-sm font-semibold text-heading">
              Products from {v.districtName || 'this district'}
            </p>
            <QueryState
              query={productsQuery}
              loadingLabel="Loading products…"
              emptyLabel="No products listed from this district yet."
              isEmpty={(page) => !page?.items?.length}
            >
              {(page) => (
                <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                  {(page.items ?? []).map((p) => (
                    <ProductCard
                      key={p.id}
                      product={mapProduct(p)}
                      to={routePaths.marketplaceProductDetails.replace(':productId', p.id)}
                    />
                  ))}
                </div>
              )}
            </QueryState>
          </>
        )}
      </QueryState>
    </div>
  );
}
