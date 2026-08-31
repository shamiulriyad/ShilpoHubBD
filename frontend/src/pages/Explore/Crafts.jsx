import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useCategories } from '../../hooks/useCategories';

export default function Crafts() {
  const { data, isLoading, isError, error } = useCategories();
  const crafts = data || [];

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
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
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
          {crafts.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No craft categories published yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
