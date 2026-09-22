import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, AsyncState, Pagination } from '../../components/ui';
import { FestivalCard } from '../../components/cards';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useDistricts } from '../../hooks/useDistricts';
import TravelEmptyState from '../../components/ui/TravelEmptyState';

export default function FestivalDirectory() {
  const [districtId, setDistrictId] = useState('');
  const [page, setPage] = useState(1);
  const festivalsQuery = useHeritageFestivals({ page, pageSize: 12, districtId: districtId || undefined });
  const districtsQuery = useDistricts();

  const filterGroups = [
    {
      key: 'districtId',
      label: 'District',
      options: (districtsQuery.data || []).map((district) => ({ label: district.name, value: district.id })),
    },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Festivals' },
        ]}
        title="Festival Directory"
        description="Seasonal and regional cultural festivals."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_minmax(0,1fr)]">
        <FilterPanel
          groups={filterGroups}
          values={{ districtId }}
          onChange={(_key, value, checked) => {
            setDistrictId(checked ? value : '');
            setPage(1);
          }}
          onClear={() => {
            setDistrictId('');
            setPage(1);
          }}
        />
        <div className="min-w-0">
          <AsyncState isLoading={festivalsQuery.isLoading} isError={festivalsQuery.isError} error={festivalsQuery.error}>
            <div className="grid gap-4 sm:grid-cols-2">
              {(festivalsQuery.data?.items || []).map((festival) => (
                <FestivalCard
                  key={festival.id}
                  festival={{ name: festival.name, date: festival.startDate, district: festival.districtName }}
                />
              ))}
              {festivalsQuery.data?.items?.length === 0 && (
                <TravelEmptyState title={districtId ? 'No festivals in this district yet' : 'The festival calendar is being prepared'} description={districtId ? 'Choose another district or clear the filter to see all published festivals.' : 'Explore heritage places while upcoming celebrations are added.'} onReset={districtId ? () => { setDistrictId(''); setPage(1); } : undefined} />
              )}
            </div>
          </AsyncState>
          {festivalsQuery.data?.totalPages > 1 && (
            <div className="mt-8">
              <Pagination currentPage={page} totalPages={festivalsQuery.data.totalPages} onPageChange={setPage} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
