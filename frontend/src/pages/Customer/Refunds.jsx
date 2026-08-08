import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { refunds } from '../../data/mockData';

const statusTone = { Processed: 'success', Pending: 'secondary' };

export default function Refunds() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Refunds' }]}
        title="Refunds"
        description="Status of refunds issued for your returned orders."
      />

      <div className="divide-y divide-border rounded-xl border border-border bg-surface">
        {refunds.map((refund) => (
          <div key={refund.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
            <div>
              <p className="text-sm font-medium text-heading">{refund.id}</p>
              <p className="text-xs text-body/60">
                Order {refund.orderId} · {refund.method} · {refund.date}
              </p>
            </div>
            <div className="flex items-center gap-4">
              <p className="text-sm font-semibold text-primary">৳ {refund.amount.toLocaleString()}</p>
              <Badge tone={statusTone[refund.status] || 'neutral'}>{refund.status}</Badge>
            </div>
          </div>
        ))}
        {refunds.length === 0 && <p className="p-6 text-center text-sm text-body/60">No refunds to show.</p>}
      </div>
    </div>
  );
}
