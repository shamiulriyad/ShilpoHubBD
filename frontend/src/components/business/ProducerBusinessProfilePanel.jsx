import { Badge } from '../ui';

const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;

// Partnership-evaluation view: revenue, orders, growth, best sellers and repeat-customer rate —
// aggregated business metrics only, no customer names/contact details.
export default function ProducerBusinessProfilePanel({ profile, isLoading }) {
  if (isLoading) return <p className="text-sm text-body/60">Loading business profile…</p>;
  if (!profile) return null;

  const growth = profile.salesGrowthPercent;
  const growthTone = growth == null ? 'secondary' : growth >= 0 ? 'success' : 'neutral';
  const growthLabel = growth == null ? 'Not enough history' : `${growth >= 0 ? '+' : ''}${growth}% vs. prior 30 days`;

  return (
    <div className="space-y-4 rounded-xl border border-border bg-surface p-5">
      <div>
        <p className="text-xs font-semibold uppercase tracking-wide text-body/50">Business performance</p>
        <p className="text-xs text-body/60">Aggregated from delivered orders. No customer details are shared.</p>
      </div>

      <div className="grid grid-cols-2 gap-3 text-sm">
        <Metric label="Total revenue" value={money(profile.totalRevenue)} />
        <Metric label="Total orders" value={profile.totalOrders} />
        <Metric label="Items sold" value={profile.totalItemsSold} />
        <Metric label="Avg. order value" value={money(profile.averageOrderValue)} />
        <Metric label="Total products" value={profile.totalProductCount} />
        <Metric label="Active products" value={profile.activeProductCount} />
        <Metric label="Total customers" value={profile.totalCustomerCount} />
        <Metric
          label="Repeat customers"
          value={profile.repeatCustomerRatePercent != null ? `${profile.repeatCustomerCount} (${profile.repeatCustomerRatePercent}%)` : profile.repeatCustomerCount}
        />
      </div>

      <div>
        <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Sales growth (30d)</p>
        <Badge tone={growthTone}>{growthLabel}</Badge>
      </div>

      {profile.bestSellingProducts?.length > 0 && (
        <div>
          <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-body/50">Best-selling products</p>
          <ul className="space-y-1.5">
            {profile.bestSellingProducts.map((p) => (
              <li key={p.productId} className="flex items-center justify-between text-sm">
                <span className="text-heading">{p.productName}</span>
                <span className="text-body/60">{p.quantitySold} sold · {money(p.revenue)}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

function Metric({ label, value }) {
  return (
    <div>
      <p className="text-xs text-body/50">{label}</p>
      <p className="font-semibold text-heading">{value}</p>
    </div>
  );
}
