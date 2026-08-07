import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, CheckoutForm } from '../../components/ui';

const steps = ['Shipping', 'Payment', 'Review'];

export default function Checkout() {
  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
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
        <CheckoutForm />

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
          <Link to={routePaths.customerOrderSuccess}>
            <Button variant="primary" className="w-full">
              Place Order
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
