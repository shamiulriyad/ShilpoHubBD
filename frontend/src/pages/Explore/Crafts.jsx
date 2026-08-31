import { routePaths } from '../../routes/routePaths';
import { PageHeader, QueryState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useCategories } from '../../hooks/queries/useCatalog';

export default function Crafts() {
  const query = useCategories();

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Crafts' },
        ]}
        title="Crafts"
        description="Traditional craft disciplines practiced across Bangladesh."
      />
      <QueryState query={query} emptyLabel="No craft categories have been added yet.">
        {(crafts) => (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {crafts.map((craft) => (
              <EntityCard
                key={craft.id}
                title={craft.name}
                subtitle={craft.description}
                meta={`${craft.productCount} product${craft.productCount === 1 ? '' : 's'}`}
                to={routePaths.exploreCraftDetails.replace(':craftId', craft.id)}
              />
            ))}
          </div>
        )}
      </QueryState>
    </div>
  );
}
