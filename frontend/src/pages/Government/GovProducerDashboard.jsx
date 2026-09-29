import { Link } from 'react-router-dom';
import { PageHeader, Badge, QueryState } from '../../components/ui';
import { useSupportDashboard, useSupportCases, useImpactReport } from '../../hooks/useArtisanSupport';
import { useGovProducerReports } from '../../hooks/useGovProducerReports';
import { routePaths } from '../../routes/routePaths';

const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;
const percent = (value) => (value == null ? '—' : `${value >= 0 ? '+' : ''}${value}%`);
const impactStatusTone = (status) => (status === 'Improved' ? 'success' : status === 'Declined' ? 'secondary' : 'neutral');
const caseStatusTone = (status) => (status === 'Closed' ? 'success' : status?.includes('Report') ? 'secondary' : 'neutral');

export default function GovProducerDashboard() {
  const dashboard = useSupportDashboard();
  const cases = useSupportCases();
  const sharedReports = useGovProducerReports({ pageSize: 10 });
  const impactReport = useImpactReport();

  const kpis = dashboard.data
    ? [
        ['Shared Producer Reports', sharedReports.data?.totalCount ?? 0],
        ['Active Cases', dashboard.data.activeCases],
        ['Support In Progress', dashboard.data.supportInProgress],
        ['Beneficiaries Supported', dashboard.data.beneficiariesSupported],
      ]
    : [];

  return (
    <div>
      <PageHeader
        title="Government/NGO Dashboard"
        description="Producer reports shared with your organization, your support interventions, and their measured before/after impact. You can only see what has been shared with you or assigned to your organization."
      />

      <div className="mb-8 grid grid-cols-2 gap-3 lg:grid-cols-4">
        {kpis.map(([label, value]) => (
          <div key={label} className="rounded-xl border border-border bg-surface p-4">
            <p className="text-2xl font-semibold text-primary">{value}</p>
            <p className="text-xs text-body/60">{label}</p>
          </div>
        ))}
      </div>

      {/* Shared Producer Reports */}
      <section className="mb-8">
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-body/60">Shared Producer Reports</h2>
        <QueryState query={sharedReports} emptyLabel="No producer reports have been shared with your organization yet.">
          {(data) => (
            <div className="overflow-x-auto rounded-xl border border-border bg-surface">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-border bg-background/50 text-xs uppercase tracking-wide text-body/60">
                  <tr>
                    <th className="px-4 py-3">Producer</th>
                    <th className="px-4 py-3">Month</th>
                    <th className="px-4 py-3 text-right">Sales</th>
                    <th className="px-4 py-3 text-right">Income</th>
                    <th className="px-4 py-3 text-right">Orders</th>
                    <th className="px-4 py-3 text-right">Rating</th>
                    <th className="px-4 py-3">Producer Problems</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((r) => (
                    <tr key={r.id} className="border-b border-border last:border-0">
                      <td className="px-4 py-3">
                        <p className="font-medium text-heading">{r.producerName}</p>
                        <p className="text-xs text-body/50">{r.producerEmail}</p>
                      </td>
                      <td className="px-4 py-3">{r.year}-{String(r.month).padStart(2, '0')}</td>
                      <td className="px-4 py-3 text-right">{money(r.totalSales)}</td>
                      <td className="px-4 py-3 text-right">{money(r.netIncome)}</td>
                      <td className="px-4 py-3 text-right">{r.totalOrders}</td>
                      <td className="px-4 py-3 text-right">{r.averageRating == null ? '—' : `★ ${r.averageRating.toFixed(1)}`}</td>
                      <td className="px-4 py-3">
                        {r.detectedProblems.length === 0 ? (
                          <span className="text-xs text-body/40">None</span>
                        ) : (
                          <div className="flex flex-wrap gap-1">
                            {r.detectedProblems.map((p) => <Badge key={p} tone="secondary">{p}</Badge>)}
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </QueryState>
      </section>

      {/* Existing support / interventions */}
      <section className="mb-8">
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-body/60">Existing Support / Interventions</h2>
          <Link to={routePaths.governmentArtisanSupport} className="text-xs font-semibold text-primary underline">
            Manage cases →
          </Link>
        </div>
        <AsyncCaseList cases={cases} />
      </section>

      {/* Before/after performance & impact status */}
      <section>
        <h2 className="mb-3 text-sm font-semibold uppercase tracking-wide text-body/60">Producer Support Impact</h2>
        <QueryState query={impactReport} emptyLabel="No completed interventions (support provided) yet.">
          {(rows) => (
            <div className="space-y-3">
              {rows.map((row) => (
                <article key={row.caseId} className="rounded-xl border border-border bg-surface p-4">
                  <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                    <div>
                      <p className="font-semibold text-heading">{row.producerName}</p>
                      <p className="text-xs text-body/60">
                        {row.supportType} · Support given {new Date(row.supportDate).toLocaleDateString()} ·{' '}
                        {row.beforeYear}-{String(row.beforeMonth).padStart(2, '0')} vs {row.afterYear}-{String(row.afterMonth).padStart(2, '0')}{' '}
                        ({row.monthsUsedForComparison}/2 months of data)
                      </p>
                    </div>
                  </div>
                  <div className="overflow-x-auto">
                    <table className="w-full text-left text-xs">
                      <thead className="text-body/50">
                        <tr><th className="py-1 pr-3">Metric</th><th className="py-1 pr-3">Before</th><th className="py-1 pr-3">After</th><th className="py-1 pr-3">Change</th><th className="py-1">Impact Status</th></tr>
                      </thead>
                      <tbody>
                        {row.metrics.map((m) => (
                          <tr key={m.metricType} className="border-t border-border">
                            <td className="py-1.5 pr-3 font-medium">{m.metricType}</td>
                            <td className="py-1.5 pr-3">{m.beforeValue ?? '—'}</td>
                            <td className="py-1.5 pr-3">{m.afterValue ?? '—'}</td>
                            <td className="py-1.5 pr-3">{percent(m.changePercentage)}</td>
                            <td className="py-1.5"><Badge tone={impactStatusTone(m.status)}>{m.status}</Badge></td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  <p className="mt-2 text-xs italic text-body/50">{row.disclaimer}</p>
                </article>
              ))}
            </div>
          )}
        </QueryState>
      </section>
    </div>
  );
}

function AsyncCaseList({ cases }) {
  return (
    <QueryState query={cases} emptyLabel="No support cases yet.">
      {(items) => (
        <div className="space-y-2">
          {items.slice(0, 5).map((c) => (
            <Link
              key={c.id}
              to={routePaths.governmentArtisanSupport}
              className="flex items-center justify-between rounded-lg border border-border bg-surface px-4 py-3 text-sm hover:border-primary/30"
            >
              <div>
                <p className="font-medium text-heading">{c.problemTitle}</p>
                <p className="text-xs text-body/60">{c.caseNumber} · {c.artisanName}</p>
              </div>
              <Badge tone={caseStatusTone(c.status)}>{c.status}</Badge>
            </Link>
          ))}
        </div>
      )}
    </QueryState>
  );
}
