import { useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader, Badge, Button, AnalyticsChart, QueryState } from '../../components/ui';
import { useProducerMonthlyHistory, useProducerMonthlyComparison } from '../../hooks/useProducerIntelligence';

const MONTH_NAMES = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;
const percent = (value) => (value == null ? '—' : `${value >= 0 ? '+' : ''}${value}%`);
const monthLabel = (report) => `${MONTH_NAMES[report.month - 1]} ${report.year}`;
const deltaTone = (value) => (value > 0 ? 'success' : value < 0 ? 'secondary' : 'neutral');

export default function ProducerIntelligenceDetail() {
  const { producerId } = useParams();
  const navigate = useNavigate();

  const history = useProducerMonthlyHistory(producerId, { pageSize: 60 });
  const comparison = useProducerMonthlyComparison(producerId, {});

  // History comes back most-recent-first; charts read left-to-right chronologically.
  const chronological = useMemo(
    () => [...(history.data?.items || [])].reverse().map((r) => ({ ...r, monthLabel: monthLabel(r) })),
    [history.data],
  );

  const latest = history.data?.items?.[0];

  return (
    <div>
      <PageHeader
        title={latest ? latest.producerName : 'Producer performance'}
        description={latest?.producerEmail}
        action={<Button variant="secondary" onClick={() => navigate(-1)}>Back to list</Button>}
      />

      <QueryState query={history} emptyLabel="This producer has no monthly reports yet.">
        {() => (
          <div className="space-y-6">
            {comparison.data && (
              <div className="rounded-xl border border-border bg-surface p-5">
                <p className="mb-3 text-sm font-semibold text-heading">
                  {monthLabel(comparison.data.currentMonth)} vs{' '}
                  {comparison.data.previousMonth ? monthLabel(comparison.data.previousMonth) : 'no prior month'}
                </p>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
                  <DeltaTile label="Sales" value={comparison.data.salesChange} formatter={money} />
                  <DeltaTile label="Orders" value={comparison.data.ordersChange} />
                  <DeltaTile label="Net income" value={comparison.data.netIncomeChange} formatter={money} />
                  <DeltaTile label="Units sold" value={comparison.data.unitsSoldChange} />
                  <DeltaTile label="Rating" value={comparison.data.averageRatingChange} />
                  <DeltaTile
                    label="Cancellation rate"
                    value={comparison.data.cancellationRateChange}
                    formatter={(v) => `${v}%`}
                    invert
                  />
                </div>
              </div>
            )}

            <div className="grid gap-4 lg:grid-cols-2">
              <AnalyticsChart title="Sales trend" type="line" data={chronological} labelKey="monthLabel" valueKey="totalSales" valueFormatter={money} />
              <AnalyticsChart title="Net income trend" type="line" data={chronological} labelKey="monthLabel" valueKey="netIncome" valueFormatter={money} />
              <AnalyticsChart title="Orders trend" type="line" data={chronological} labelKey="monthLabel" valueKey="totalOrders" />
              <AnalyticsChart title="Rating trend" type="bar" data={chronological} labelKey="monthLabel" valueKey="averageRating" />
            </div>

            <div className="overflow-x-auto rounded-xl border border-border bg-surface">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-border bg-background/50 text-xs uppercase tracking-wide text-body/60">
                  <tr>
                    <th className="px-4 py-3">Month</th>
                    <th className="px-4 py-3 text-right">Sales</th>
                    <th className="px-4 py-3 text-right">Income</th>
                    <th className="px-4 py-3 text-right">Orders</th>
                    <th className="px-4 py-3 text-right">Rating</th>
                    <th className="px-4 py-3 text-right">Growth</th>
                    <th className="px-4 py-3 text-right">Position</th>
                    <th className="px-4 py-3 text-right">Cancellation rate</th>
                  </tr>
                </thead>
                <tbody>
                  {(history.data?.items || []).map((r) => (
                    <tr key={r.id} className="border-b border-border last:border-0">
                      <td className="px-4 py-3 font-medium text-heading">{monthLabel(r)}</td>
                      <td className="px-4 py-3 text-right">{money(r.totalSales)}</td>
                      <td className="px-4 py-3 text-right">{money(r.netIncome)}</td>
                      <td className="px-4 py-3 text-right">{r.totalOrders}</td>
                      <td className="px-4 py-3 text-right">{r.averageRating == null ? '—' : `★ ${r.averageRating.toFixed(1)}`}</td>
                      <td className="px-4 py-3 text-right">{percent(r.salesGrowthPercentage)}</td>
                      <td className="px-4 py-3 text-right">
                        #{r.overallSalesRank}
                        {r.overallSalesPercentile != null && <span className="text-body/50"> ({r.overallSalesPercentile}%ile)</span>}
                      </td>
                      <td className="px-4 py-3 text-right">{r.cancellationRate}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </QueryState>
    </div>
  );
}

function DeltaTile({ label, value, formatter = (v) => v, invert = false }) {
  const tone = value == null ? 'neutral' : deltaTone(invert ? -value : value);
  return (
    <div className="rounded-lg border border-border bg-background p-3">
      <p className="text-xs text-body/50">{label}</p>
      <Badge tone={tone} className="mt-1">
        {value == null ? '—' : `${value >= 0 ? '+' : ''}${formatter(value)}`}
      </Badge>
    </div>
  );
}
