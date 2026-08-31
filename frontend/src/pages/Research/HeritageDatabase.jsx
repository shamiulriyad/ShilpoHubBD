import { routePaths } from '../../routes/routePaths';
import { PageHeader, Table, SearchBar, AsyncState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';

export default function HeritageDatabase() {
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();

  const villageCountByDistrict = (villagesQuery.data || []).reduce((acc, v) => {
    acc[v.districtId] = (acc[v.districtId] || 0) + 1;
    return acc;
  }, {});

  const rows = (districtsQuery.data || []).map((d) => ({
    district: d.name,
    division: d.division,
    villages: villageCountByDistrict[d.id] || 0,
  }));

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Innovation Hub', path: routePaths.research },
          { label: 'Heritage Database' },
        ]}
        title="Heritage Database"
        description="Open datasets on districts, villages, crafts and producers."
      />
      <div className="mb-6 max-w-xl">
        <SearchBar placeholder="Search the heritage database…" />
      </div>
      <AsyncState
        isLoading={districtsQuery.isLoading}
        isError={districtsQuery.isError}
        error={districtsQuery.error}
      >
        <Table columns={['district', 'division', 'villages']} rows={rows} />
      </AsyncState>
    </div>
  );
}
