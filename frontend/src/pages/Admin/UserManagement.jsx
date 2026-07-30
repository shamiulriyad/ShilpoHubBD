import { PageHeader, Table, SearchBar, Button, Badge } from '../../components/ui';
import { adminUsers } from '../../data/mockData';

export default function UserManagement() {
  return (
    <div>
      <PageHeader
        title="User Management"
        description="Manage producers, customers, partners and staff accounts."
        action={<Button variant="primary">Invite User</Button>}
      />
      <div className="mb-6 max-w-md">
        <SearchBar placeholder="Search users…" />
      </div>
      <Table
        columns={['name', 'role', 'status', 'joined']}
        rows={adminUsers.map((u) => ({
          name: u.name,
          role: u.role,
          status: <Badge tone={u.status === 'Active' ? 'success' : 'secondary'}>{u.status}</Badge>,
          joined: u.joined,
        }))}
      />
    </div>
  );
}
