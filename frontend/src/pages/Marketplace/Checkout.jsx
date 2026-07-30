import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button } from '../../components/ui';

const steps = ['Shipping', 'Payment', 'Review'];

export default function Checkout() {
  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Marketplace', path: routePaths.marketplace },
          { label: 'Checkout' },
        ]}
        title="Checkout"
      />

      <div className="mb-8 flex items-center gap-3">
        {steps.map((step, index) => (
          <div key={step} className="flex items-center gap-2">
            <span
              className={`flex h-7 w-7 items-center justify-center rounded-full text-xs font-semibold ${
                index === 0 ? 'bg-primary text-surface' : 'border border-border text-body/60'
              }`}
            >
              {index + 1}
            </span>
            <span className={`text-sm ${index === 0 ? 'font-medium text-heading' : 'text-body/60'}`}>{step}</span>
            {index < steps.length - 1 && <span className="mx-2 h-px w-8 bg-border" />}
          </div>
        ))}
      </div>

      <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-6 rounded-xl border border-border bg-surface p-6">
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Shipping Address</p>
            <div className="grid gap-4 sm:grid-cols-2">
              <input placeholder="Full Name" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
              <input placeholder="Phone Number" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
              <input placeholder="District" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
              <input placeholder="Postal Code" className="rounded-md border border-border bg-background px-3 py-2 text-sm" />
              <input placeholder="Full Address" className="sm:col-span-2 rounded-md border border-border bg-background px-3 py-2 text-sm" />
            </div>
          </div>
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Payment Method</p>
            <div className="grid gap-3 sm:grid-cols-3">
              {['Mobile Banking', 'Card Payment', 'Cash on Delivery'].map((method) => (
                <label key={method} className="flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm">
                  <input type="radio" name="payment" />
                  {method}
                </label>
              ))}
            </div>
          </div>
        </div>

        <div className="h-fit space-y-3 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">Order Summary</p>
          <div className="flex justify-between text-sm text-body/70">
            <span>Items (3)</span>
            <span>৳ 5,850</span>
          </div>
          <div className="flex justify-between text-sm text-body/70">
            <span>Shipping</span>
            <span>৳ 120</span>
          </div>
          <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
            <span>Total</span>
            <span>৳ 5,970</span>
          </div>
          <Button variant="primary" className="w-full">
            Place Order
          </Button>
        </div>
      </div>
    </div>
  );
}
