import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
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
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Heritage Villages', path: routePaths.exploreVillages },
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
    </div>
  );
}
