import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useProducts } from '../../hooks/useProducts';

const uniqueSorted = (values) => [...new Set(values.filter(Boolean))].sort();

// The backend does not expose a public producer directory. This read-only
// directory is derived from distinct producer names in the product list DTO.
export default function Producers() {
  const { data, isLoading, isError, error } = useProducts({ pageSize: 50 });
  const [craft, setCraft] = useState('');
  const [district, setDistrict] = useState('');
  const items = data?.items || [];

  const producers = Object.values(
    items.reduce((acc, product) => {
      if (product.producerName && !acc[product.producerName]) {
        acc[product.producerName] = {
          name: product.producerName,
          craft: product.categoryName,
          district: product.districtName,
        };
      }
      return acc;
    }, {}),
  );

  const filteredProducers = producers.filter(
    (producer) => (!craft || producer.craft === craft) && (!district || producer.district === district),
  );

  const filterGroups = [
    { key: 'craft', label: 'Craft', options: uniqueSorted(producers.map((producer) => producer.craft)) },
    { key: 'district', label: 'District', options: uniqueSorted(producers.map((producer) => producer.district)) },
  ];

  const updateFilter = (key, value, checked) => {
    const next = checked ? value : '';
    if (key === 'craft') setCraft(next);
    if (key === 'district') setDistrict(next);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Producers' },
        ]}
        title="Producers"
        description="Artisans, farmers and makers represented in the current product catalog."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_minmax(0,1fr)]">
        <FilterPanel
          groups={filterGroups}
          values={{ craft, district }}
          onChange={updateFilter}
          onClear={() => {
            setCraft('');
            setDistrict('');
          }}
        />
        <AsyncState isLoading={isLoading} isError={isError} error={error}>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
            {filteredProducers.map((producer) => (
              <EntityCard key={producer.name} title={producer.name} subtitle={producer.craft} meta={producer.district} />
            ))}
            {filteredProducers.length === 0 && (
              <p className="col-span-full text-sm text-body/60">No producers match the selected filters.</p>
            )}
          </div>
        </AsyncState>
      </div>
    </div>
  );
}
