import { PageHeader, Table, Button } from '../../components/ui';
import { districts, villages, crafts } from '../../data/mockData';

export default function HeritageManagement() {
  return (
    <div>
      <PageHeader
        title="Heritage Management"
        description="Manage districts, villages, crafts and UNESCO entries."
        action={<Button variant="primary">Add Entry</Button>}
      />
      <div className="space-y-8">
        <div>
          <p className="mb-3 text-sm font-semibold text-heading">Districts</p>
          <Table columns={['name', 'villages', 'crafts']} rows={districts.map((d) => ({ name: d.name, villages: d.villages, crafts: d.crafts }))} />
        </div>
        <div>
          <p className="mb-3 text-sm font-semibold text-heading">Villages</p>
          <Table columns={['name', 'craft', 'district']} rows={villages} />
        </div>
        <div>
          <p className="mb-3 text-sm font-semibold text-heading">Crafts</p>
          <Table columns={['name', 'category', 'producers']} rows={crafts} />
        </div>
      </div>
    </div>
  );
}
