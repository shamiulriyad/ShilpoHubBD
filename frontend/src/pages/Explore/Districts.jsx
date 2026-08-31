import { routePaths } from '../../routes/routePaths';
<<<<<<< HEAD
import { PageHeader, QueryState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useDistricts } from '../../hooks/queries/useCatalog';

export default function Districts() {
  const query = useDistricts();
=======
import { PageHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';

export default function Districts() {
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();

  const villageCountByDistrict = (villagesQuery.data || []).reduce((acc, village) => {
    acc[village.districtId] = (acc[village.districtId] || 0) + 1;
    return acc;
  }, {});
>>>>>>> origin/main

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Districts' },
        ]}
        title="Districts"
        description="Browse heritage villages, crafts and producers by district."
      />
<<<<<<< HEAD
      <QueryState query={query} emptyLabel="No districts have been added yet.">
        {(districts) => (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {districts.map((district) => (
              <EntityCard
                key={district.id}
                title={district.name}
                subtitle={district.division}
                to={routePaths.exploreDistrictDetails.replace(':districtId', district.id)}
              />
            ))}
          </div>
        )}
      </QueryState>
=======
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        <AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}>
          {districtsQuery.data?.map((district) => (
            <EntityCard
              key={district.id}
              title={district.name}
              subtitle={`${villageCountByDistrict[district.id] || 0} villages · ${district.division}`}
              to={routePaths.exploreDistrictDetails.replace(':districtId', district.id)}
            />
          ))}
        </AsyncState>
      </div>
>>>>>>> origin/main
    </div>
  );
}
