import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { villages, favoriteVillageIds } from '../../data/mockData';

export default function FavoriteVillages() {
  const favorites = villages.filter((v) => favoriteVillageIds.includes(v.id));

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Favorite Villages' }]}
        title="Favorite Villages"
        description={`${favorites.length} heritage villages you've saved to revisit.`}
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {favorites.map((village) => (
          <div key={village.id} className="space-y-2">
            <VillageCard village={village} to={routePaths.exploreVillageDetails.replace(':villageId', village.id)} />
            <Button variant="secondary" className="w-full">
              Remove from Favorites
            </Button>
          </div>
        ))}
      </div>
    </div>
  );
}
