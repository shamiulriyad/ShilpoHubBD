import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, QueryState } from '../../components/ui';
import { VillageCard } from '../../components/cards';
import { useVillages } from '../../hooks/queries/useCatalog';

const uniqueSorted = (values) => [...new Set(values.filter(Boolean))].sort();

export default function Villages() {
  const query = useVillages();
  const villages = query.data ?? [];
  const [district, setDistrict] = useState('');
  const [craft, setCraft] = useState('');

  const filteredVillages = villages.filter(
    (village) => (!district || village.districtName === district) && (!craft || village.craft === craft),
  );

  const filterGroups = [
    { key: 'district', label: 'District', options: uniqueSorted(villages.map((village) => village.districtName)) },
    { key: 'craft', label: 'Craft', options: uniqueSorted(villages.map((village) => village.craft)) },
  ];

  const updateFilter = (key, value, checked) => {
    const next = checked ? value : '';
    if (key === 'district') setDistrict(next);
    if (key === 'craft') setCraft(next);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Heritage Villages' },
        ]}
        title="Heritage Villages"
        description="Villages recognized for keeping traditional crafts alive."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_minmax(0,1fr)]">
        <FilterPanel
          groups={filterGroups}
          values={{ district, craft }}
          onChange={updateFilter}
          onClear={() => {
            setDistrict('');
            setCraft('');
          }}
        />
        <QueryState query={query} emptyLabel="No heritage villages have been added yet.">
          {() => (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
              {filteredVillages.map((village) => (
                <VillageCard
                  key={village.id}
                  village={{ ...village, district: village.districtName }}
                  to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
                />
              ))}
              {filteredVillages.length === 0 && (
                <p className="col-span-full text-sm text-body/60">No villages match the selected filters.</p>
              )}
            </div>
          )}
        </QueryState>
      </div>
    </div>
  );
}
