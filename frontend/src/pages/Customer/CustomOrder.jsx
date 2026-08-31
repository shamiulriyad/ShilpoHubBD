import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';
import { useCategories } from '../../hooks/useCategories';
import { useProducts } from '../../hooks/useProducts';

export default function CustomOrder() {
  const { data: categories } = useCategories();
  const { data: productPage } = useProducts({ pageSize: 50 });

  const crafts = categories || [];
  // No producer-directory endpoint yet — offer the producers that appear in the catalog.
  const producerNames = [
    ...new Set((productPage?.items || []).map((p) => p.producerName).filter(Boolean)),
  ].sort();

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: 'Custom Order' },
        ]}
        title="Request a Custom Order"
        description="Commission a bespoke piece directly from a heritage producer, made to your specifications."
      />

      {/* TODO(backend): POST /api/custom-orders exists — wire this form's submit to it. */}
      <form onSubmit={(event) => event.preventDefault()} className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-6 rounded-xl border border-border bg-surface p-6">
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Craft & Producer</p>
            <div className="grid gap-4 sm:grid-cols-2">
              <select className="rounded-md border border-border bg-background px-3 py-2 text-sm">
                <option>Select a craft</option>
                {crafts.map((craft) => (
                  <option key={craft.id}>{craft.name}</option>
                ))}
              </select>
              <select className="rounded-md border border-border bg-background px-3 py-2 text-sm">
                <option>Any producer (we’ll match you)</option>
                {producerNames.map((name) => (
                  <option key={name}>{name}</option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Order Details</p>
            <div className="grid gap-4">
              <input placeholder="Item title (e.g. Custom Jamdani Saree)" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
              <textarea
                placeholder="Describe colors, size, materials and any inspiration references…"
                rows={5}
                className="rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <div className="grid gap-4 sm:grid-cols-2">
                <input placeholder="Budget range (৳)" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
                <input type="date" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
              </div>
              <div className="flex aspect-[3/1] items-center justify-center rounded-lg border border-dashed border-border text-xs text-body/40">
                Upload reference images
              </div>
            </div>
          </div>
        </div>

        <div className="h-fit space-y-3 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">How Custom Orders Work</p>
          <ol className="space-y-2 text-sm text-body/70">
            <li>1. Submit your request with details and budget.</li>
            <li>2. A matching producer reviews and sends a quote.</li>
            <li>3. Approve the quote to begin production.</li>
            <li>4. Track progress until delivery.</li>
          </ol>
          <Button variant="primary" className="w-full">
            Submit Request
          </Button>
        </div>
      </form>
    </div>
  );
}
