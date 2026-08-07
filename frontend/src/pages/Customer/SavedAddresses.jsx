import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button } from '../../components/ui';
import { addresses } from '../../data/mockData';

export default function SavedAddresses() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Saved Addresses' }]}
        title="Saved Addresses"
        description="Manage the delivery addresses on your account."
        action={<Button variant="primary">Add New Address</Button>}
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {addresses.map((address) => (
          <div key={address.id} className="space-y-2 rounded-xl border border-border bg-surface p-5">
            <div className="flex items-center justify-between">
              <p className="text-sm font-semibold text-heading">{address.label}</p>
              {address.isDefault && <Badge tone="primary">Default</Badge>}
            </div>
            <p className="text-sm text-body/80">{address.name}</p>
            <p className="text-sm text-body/70">{address.address}</p>
            <p className="text-xs text-body/60">{address.phone}</p>
            <div className="flex gap-2 pt-2">
              <Button variant="secondary" className="flex-1">
                Edit
              </Button>
              <Button variant="secondary" className="flex-1">
                Remove
              </Button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
