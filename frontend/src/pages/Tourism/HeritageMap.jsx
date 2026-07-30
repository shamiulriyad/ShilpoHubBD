import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { districts } from '../../data/mockData';

export default function HeritageMap() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'Heritage Map' },
        ]}
        title="Heritage Map"
        description="Explore heritage sites, villages and events across Bangladesh."
      />
      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <div className="flex aspect-[16/10] items-center justify-center rounded-2xl border border-dashed border-border bg-surface text-sm text-body/40">
          Interactive Bangladesh Map Placeholder
        </div>
        <div className="space-y-2">
          <p className="mb-2 text-sm font-semibold text-heading">Districts</p>
          {districts.map((district) => (
            <button
              key={district.id}
              type="button"
              className="block w-full rounded-lg border border-border bg-surface px-3 py-2 text-left text-sm text-body hover:border-primary hover:text-primary"
            >
              {district.name}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
