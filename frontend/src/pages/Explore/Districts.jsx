import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useDistricts } from '../../hooks/queries/useCatalog';

export default function Districts() {
  const query = useDistricts();

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
    </div>
  );
}
