import { Link } from 'react-router-dom';
import { PageHeader, AsyncState, Badge } from '../../components/ui';
import StatCard from '../../components/cards/StatCard';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import { routePaths } from '../../routes/routePaths';
import { useFundingPrograms } from '../../hooks/useFunding';

const money = (value) => `৳ ${Number(value || 0).toLocaleString('en-BD')}`;
const STATUS_TONE = { Open: 'success', Draft: 'secondary', Closed: 'neutral', Archived: 'neutral' };

// The NGO workspace used to show fixed numbers (6 projects, 32 communities, 2,150 beneficiaries,
// 78% utilised) that came from nowhere. Everything here is now computed from the live funding
// programmes; nothing is shown that the system does not actually track.
export default function NGOPage() {
  const programsQuery = useFundingPrograms({ pageSize: 100 });
  const programs = programsQuery.data?.items || [];

  const open = programs.filter((p) => p.status === 'Open');
  const totalBudget = programs.reduce((sum, p) => sum + Number(p.totalBudget || 0), 0);
  const allocated = programs.reduce((sum, p) => sum + Number(p.allocatedAmount || 0), 0);
  const disbursed = programs.reduce((sum, p) => sum + Number(p.disbursedAmount || 0), 0);
  const applications = programs.reduce((sum, p) => sum + Number(p.applicationCount || 0), 0);
  const utilised = totalBudget > 0 ? Math.round((disbursed / totalBudget) * 100) : null;

  return (
    <div>
      <PageHeader
        title="NGO Dashboard"
        description="Funding programmes and producer support, from the live funding records."
        action={
          <Link to={routePaths.governmentFunding} className="rounded-full border border-border px-4 py-2 text-sm font-semibold text-primary hover:border-primary">
            Manage funding →
          </Link>
        }
      />

      <AsyncState isLoading={programsQuery.isLoading} isError={programsQuery.isError} error={programsQuery.error}>
        <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
          <StatCard label="Open programmes" value={String(open.length)} />
          <StatCard label="Applications received" value={applications.toLocaleString('en-BD')} />
          <StatCard label="Budget allocated" value={money(allocated)} />
          <StatCard label="Funds disbursed" value={utilised == null ? money(disbursed) : `${money(disbursed)} (${utilised}%)`} />
        </div>

        <h2 className="mb-3 text-sm font-semibold text-heading">Funding programmes</h2>
        {programs.length === 0 ? (
          <TravelEmptyState title="No funding programmes yet" description="Programmes created under Funding & Grants will appear here with their budgets and applications." />
        ) : (
          <div className="divide-y divide-border rounded-xl border border-border bg-surface">
            {programs.map((p) => (
              <div key={p.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
                <div>
                  <p className="text-sm font-medium text-heading">{p.name}</p>
                  <p className="text-xs text-body/60">
                    {p.type} · {money(p.disbursedAmount)} of {money(p.totalBudget)} disbursed · {p.applicationCount} application{p.applicationCount === 1 ? '' : 's'}
                    {p.applicationClosesAt ? ` · closes ${new Date(p.applicationClosesAt).toLocaleDateString()}` : ''}
                  </p>
                </div>
                <Badge tone={STATUS_TONE[p.status] || 'neutral'}>{p.status}</Badge>
              </div>
            ))}
          </div>
        )}
      </AsyncState>
    </div>
  );
}
