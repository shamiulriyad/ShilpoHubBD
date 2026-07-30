import { PageHeader, Button } from '../../components/ui';

export default function Portfolio() {
  return (
    <div>
      <PageHeader
        title="Portfolio"
        description="Showcase the work you've created through your courses."
        action={<Button variant="primary">Add Work</Button>}
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {Array.from({ length: 6 }).map((_, i) => (
          <div key={i} className="flex aspect-square items-center justify-center rounded-xl border border-dashed border-border bg-surface text-xs text-body/40">
            Portfolio Item
          </div>
        ))}
      </div>
    </div>
  );
}
