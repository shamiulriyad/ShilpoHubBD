import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { products } from '../../data/mockData';

export default function DigitalMuseum() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'Digital Museum' },
        ]}
        title="Digital Museum"
        description="A curated digital collection of heritage artefacts and craft pieces."
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {products.map((item) => (
          <div key={item.id} className="overflow-hidden rounded-xl border border-border bg-surface">
            <div className="flex aspect-square items-center justify-center bg-background text-xs text-body/40">
              Museum Piece
            </div>
            <div className="p-3">
              <p className="text-sm font-medium text-heading">{item.name}</p>
              <p className="text-xs text-body/60">{item.category}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
