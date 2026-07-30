import { routePaths } from '../../routes/routePaths';
import { PageHeader, Table, SearchBar } from '../../components/ui';
import { districts } from '../../data/mockData';

export default function HeritageDatabase() {
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
      <Table
        columns={['district', 'villages', 'crafts']}
        rows={districts.map((d) => ({ district: d.name, villages: d.villages, crafts: d.crafts }))}
      />
    </div>
  );
}
