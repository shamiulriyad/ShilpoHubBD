import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel } from '../../components/ui';
import { publications } from '../../data/mockData';

const filterGroups = [{ label: 'Year', options: ['2022', '2023', '2024', '2025'] }];

export default function Publications() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Innovation Hub', path: routePaths.research },
          { label: 'Publications' },
        ]}
        title="Publications"
        description="Research papers, reports and case studies from the ShilpoHub network."
      />
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <div className="space-y-3">
          {publications.map((pub) => (
            <div key={pub.id} className="rounded-xl border border-border bg-surface p-4">
              <p className="text-sm font-semibold text-heading">{pub.title}</p>
              <p className="mt-1 text-xs text-body/60">
                {pub.author} · {pub.year}
              </p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
