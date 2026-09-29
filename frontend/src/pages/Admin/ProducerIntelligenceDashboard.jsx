import { useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { PageHeader, Badge, Pagination, QueryState } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useCategories } from '../../hooks/useCategories';
import { useProducerIntelligenceList, useProducerIntelligenceDashboard } from '../../hooks/useProducerIntelligence';
import { routePaths } from '../../routes/routePaths';

const fieldClass =
  'mt-1 w-full rounded-lg border border-border bg-background px-3 py-2 text-sm focus:border-primary focus:outline-none';

const money = (value) => `৳${Number(value ?? 0).toLocaleString('en-BD')}`;
const percent = (value) => (value == null ? '—' : `${value >= 0 ? '+' : ''}${value}%`);

function lastCompletedMonth() {
  const now = new Date();
  const lastDayOfPrevMonth = new Date(now.getFullYear(), now.getMonth(), 0);
  return { year: lastDayOfPrevMonth.getFullYear(), month: lastDayOfPrevMonth.getMonth() + 1 };
}

export default function ProducerIntelligenceDashboard() {
  const [params, setParams] = useSearchParams();
  const navigate = useNavigate();
  const districts = useDistricts();
  const categories = useCategories();

  const defaults = useMemo(lastCompletedMonth, []);
  const year = Number.parseInt(params.get('year'), 10) || defaults.year;
  const month = Number.parseInt(params.get('month'), 10) || defaults.month;
  const districtId = params.get('districtId') || '';
  const categoryId = params.get('categoryId') || '';
  const search = params.get('search') || '';
  const page = Math.max(1, Number.parseInt(params.get('page'), 10) || 1);

  const filters = { year, month, ...(districtId && { districtId }), ...(categoryId && { categoryId }), ...(search && { search }) };

  const dashboard = useProducerIntelligenceDashboard(filters);
  const list = useProducerIntelligenceList({ ...filters, page, pageSize: 20 });

  const setFilter = (key, value) => {
    const next = new URLSearchParams(params);
    if (value) next.set(key, value);
    else next.delete(key);
    next.delete('page');
    setParams(next);
  };

  const setMonthInput = (value) => {
    // value is "YYYY-MM" from <input type="month">
    const [y, m] = value.split('-');
    const next = new URLSearchParams(params);
    next.set('year', y);
    next.set('month', String(Number(m)));
    next.delete('page');
    setParams(next);
  };

  const kpis = dashboard.data
    ? [
        ['Total Producers', dashboard.data.totalProducers],
        ['Monthly Sales', money(dashboard.data.monthlySales)],
        ['Average Producer Income', money(dashboard.data.averageProducerIncome)],
        ['Average Rating', dashboard.data.averageRating == null ? '—' : `★ ${dashboard.data.averageRating.toFixed(2)}`],
        ['Growing Producers', dashboard.data.growingProducers],
        ['Declining Producers', dashboard.data.decliningProducers],
        ['Producers Needing Support', dashboard.data.producersNeedingSupport],
      ]
    : [];

  return (
    <div>
      <PageHeader
        title="Monthly Producer Intelligence"
        description="Compare every producer's performance for a given month, filtered by district, category, or search."
      />

      <div className="mb-6 grid grid-cols-2 gap-3 lg:grid-cols-4">
        {kpis.map(([label, value]) => (
          <div key={label} className="rounded-xl border border-border bg-surface p-4">
            <p className="text-2xl font-semibold text-primary">{value}</p>
            <p className="text-xs text-body/60">{label}</p>
          </div>
        ))}
      </div>

      <div className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2 lg:grid-cols-4">
        <label className="block text-sm font-medium">
          Month
          <input
            type="month"
            value={`${year}-${String(month).padStart(2, '0')}`}
            onChange={(e) => setMonthInput(e.target.value)}
            className={fieldClass}
          />
        </label>
        <label className="block text-sm font-medium">
          District
          <select value={districtId} onChange={(e) => setFilter('districtId', e.target.value)} className={fieldClass}>
            <option value="">All districts</option>
            {(districts.data || []).map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
        </label>
        <label className="block text-sm font-medium">
          Category
          <select value={categoryId} onChange={(e) => setFilter('categoryId', e.target.value)} className={fieldClass}>
            <option value="">All categories</option>
            {(categories.data || []).map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </label>
        <label className="block text-sm font-medium">
          Search producer
          <input
            type="search"
            placeholder="Name or email…"
            defaultValue={search}
            onKeyDown={(e) => e.key === 'Enter' && setFilter('search', e.currentTarget.value)}
            onBlur={(e) => setFilter('search', e.currentTarget.value)}
            className={fieldClass}
          />
        </label>
      </div>

      <QueryState query={list} emptyLabel="No producers match these filters." isEmpty={(data) => !data?.items?.length}>
        {(data) => (
          <>
            <div className="overflow-x-auto rounded-xl border border-border bg-surface">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-border bg-background/50 text-xs uppercase tracking-wide text-body/60">
                  <tr>
                    <th className="px-4 py-3">Producer</th>
                    <th className="px-4 py-3 text-right">Sales</th>
                    <th className="px-4 py-3 text-right">Income</th>
                    <th className="px-4 py-3 text-right">Orders</th>
                    <th className="px-4 py-3 text-right">Rating</th>
                    <th className="px-4 py-3 text-right">Growth</th>
                    <th className="px-4 py-3 text-right">Position</th>
                    <th className="px-4 py-3">Detected Problems</th>
                    <th className="px-4 py-3">Support Status</th>
                  </tr>
                </thead>
                <tbody>
                  {data.items.map((row) => (
                    <tr
                      key={row.reportId}
                      className="cursor-pointer border-b border-border last:border-0 hover:bg-background/50"
                      onClick={() => navigate(routePaths.adminProducerIntelligenceDetail.replace(':producerId', row.producerId))}
                    >
                      <td className="px-4 py-3">
                        <p className="font-medium text-heading">{row.producerName}</p>
                        <p className="text-xs text-body/50">{row.producerEmail}</p>
                      </td>
                      <td className="px-4 py-3 text-right">{money(row.sales)}</td>
                      <td className="px-4 py-3 text-right">{money(row.income)}</td>
                      <td className="px-4 py-3 text-right">{row.orders}</td>
                      <td className="px-4 py-3 text-right">{row.rating == null ? '—' : `★ ${row.rating.toFixed(1)}`}</td>
                      <td className="px-4 py-3 text-right">{percent(row.growth)}</td>
                      <td className="px-4 py-3 text-right">
                        #{row.position}
                        {row.positionPercentile != null && <span className="text-body/50"> ({row.positionPercentile}%ile)</span>}
                      </td>
                      <td className="px-4 py-3">
                        {row.detectedProblems.length === 0 ? (
                          <span className="text-xs text-body/40">None</span>
                        ) : (
                          <div className="flex flex-wrap gap-1">
                            {row.detectedProblems.map((problem) => (
                              <Badge key={problem} tone="secondary">{problem}</Badge>
                            ))}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <Badge tone={row.supportStatus === 'No active support case' ? 'neutral' : 'primary'}>
                          {row.supportStatus}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {data.totalPages > 1 && (
              <div className="mt-6">
                <Pagination
                  currentPage={page}
                  totalPages={data.totalPages}
                  onPageChange={(value) => {
                    const next = new URLSearchParams(params);
                    next.set('page', String(value));
                    setParams(next);
                  }}
                />
              </div>
            )}
          </>
        )}
      </QueryState>
    </div>
  );
}
