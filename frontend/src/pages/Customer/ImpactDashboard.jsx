import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, ChartPlaceholder } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { impactStats, producers, impactedProducerIds } from '../../data/mockData';

export default function ImpactDashboard() {
  const supported = producers.filter((p) => impactedProducerIds.includes(p.id));

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Impact Dashboard' }]}
        title="Your Impact"
        description="See how your purchases directly support Bangladesh's heritage artisans and villages."
      />

      <div className="mb-10 grid grid-cols-2 gap-4 lg:grid-cols-4">
        {impactStats.map((stat) => (
          <StatCard key={stat.label} label={stat.label} value={stat.value} />
        ))}
      </div>

      <div className="mb-10">
        <ChartPlaceholder title="Your Impact Over Time" type="bar" />
      </div>

      <SectionHeader eyebrow="Community" title="Artisans You Support" />
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {supported.map((producer) => (
          <Link
            key={producer.id}
            to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
            className="flex items-center gap-3 rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
          >
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
              {producer.name.slice(0, 1)}
            </span>
            <div className="min-w-0">
              <p className="truncate text-sm font-medium text-heading">{producer.name}</p>
              <p className="truncate text-xs text-body/60">{producer.craft}</p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
