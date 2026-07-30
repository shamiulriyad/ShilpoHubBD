import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { crafts } from '../../data/mockData';

export default function Unesco() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'UNESCO Heritage' },
        ]}
        title="UNESCO Heritage"
        description="Elements of Bangladesh's intangible cultural heritage recognized by UNESCO."
      />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {crafts.slice(0, 3).map((craft) => (
          <div key={craft.id} className="overflow-hidden rounded-xl border border-border bg-surface">
            <div className="flex aspect-video items-center justify-center bg-background text-xs text-body/40">
              Heritage Image
            </div>
            <div className="space-y-2 p-4">
              <Badge tone="success">UNESCO Recognized</Badge>
              <p className="text-sm font-semibold text-heading">{craft.name}</p>
              <p className="text-xs text-body/60">
                Recognized for its cultural significance and continued practice across generations.
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
