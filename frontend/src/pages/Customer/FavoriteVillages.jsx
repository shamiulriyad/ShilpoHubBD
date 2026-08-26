import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { useFavoriteVillages, useVillageFavoriteMutations } from '../../hooks/useVillages';
import { toVillageCardItem } from '../../utils/villageAdapters';

export default function FavoriteVillages() {
  const { data, isLoading, isError, error } = useFavoriteVillages();
  const { unfavorite } = useVillageFavoriteMutations();
  const favorites = data || [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Favorite Villages' }]}
        title="Favorite Villages"
        description={`${favorites.length} heritage villages you've saved to revisit.`}
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          {favorites.map((village) => (
            <div key={village.id} className="space-y-2">
              <VillageCard village={toVillageCardItem(village)} to={routePaths.exploreVillageDetails.replace(':villageId', village.id)} />
              <Button variant="secondary" className="w-full" onClick={() => unfavorite.mutate(village.id)}>
                Remove from Favorites
              </Button>
            </div>
          ))}
          {favorites.length === 0 && <p className="col-span-full text-sm text-body/60">No favorite villages yet.</p>}
        </AsyncState>
      </div>
    </div>
  );
}
