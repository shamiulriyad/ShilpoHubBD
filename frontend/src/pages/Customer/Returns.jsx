import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button } from '../../components/ui';
import { returns } from '../../data/mockData';

const statusTone = { Approved: 'success', 'Under Review': 'secondary', Completed: 'primary' };

export default function Returns() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Returns' }]}
        title="Returns"
        description="Track and manage your return requests."
        action={<Button variant="primary">Start a Return</Button>}
      />

      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {returns.map((item) => (
          <div key={item.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
            <div>
              <p className="text-sm font-medium text-heading">{item.product}</p>
              <p className="text-xs text-body/60">
                {item.id} · Order {item.orderId} · Requested {item.requestedDate}
              </p>
              <p className="mt-1 text-xs text-body/70">Reason: {item.reason}</p>
            </div>
            <Badge tone={statusTone[item.status] || 'neutral'}>{item.status}</Badge>
          </div>
        ))}
        {returns.length === 0 && <p className="p-6 text-center text-sm text-body/60">You have no return requests.</p>}
      </div>
    </div>
  );
}
