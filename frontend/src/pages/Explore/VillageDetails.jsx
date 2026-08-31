import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
<<<<<<< HEAD
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
=======
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useVillage, useVillageFavoriteMutations } from '../../hooks/useVillages';
import { useAuth } from '../../hooks/useAuth';

export default function VillageDetails() {
  const { villageId } = useParams();
  const { isAuthenticated } = useAuth();
  const villageQuery = useVillage(villageId);
  const { favorite, unfavorite } = useVillageFavoriteMutations();
  const village = villageQuery.data;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={villageQuery.isLoading} isError={villageQuery.isError} error={villageQuery.error}>
        {village && (
>>>>>>> origin/main
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Heritage Villages', path: routePaths.exploreVillages },
<<<<<<< HEAD
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
=======
                { label: village.name },
              ]}
              title={village.name}
              description={`${village.craft} · ${village.districtName}`}
              action={
                isAuthenticated && (
                  <Button
                    variant="secondary"
                    onClick={() => (village.isFavorited ? unfavorite.mutate(villageId) : favorite.mutate(villageId))}
                  >
                    {village.isFavorited ? 'Remove Favorite' : 'Add to Favorites'}
                  </Button>
                )
              }
            />

            <div className="mb-10 flex aspect-[21/9] items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
              {village.imageUrl ? (
                <img src={village.imageUrl} alt={village.name} className="h-full w-full rounded-2xl object-cover" />
              ) : (
                'Village Gallery Placeholder'
              )}
            </div>

            {village.description && <p className="mb-8 max-w-3xl text-sm text-body/70">{village.description}</p>}

            <Link
              to={`${routePaths.marketplaceProducts}?districtId=${village.districtId}`}
              className="inline-block text-sm font-medium text-link hover:underline"
            >
              Browse products from {village.districtName} →
            </Link>
          </>
        )}
      </AsyncState>
>>>>>>> origin/main
    </div>
  );
}
