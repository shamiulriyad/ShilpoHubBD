import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { useCart, useCartSummary } from '../../hooks/useCart';
import { useCheckout } from '../../hooks/useOrders';
import { useDistricts } from '../../hooks/useDistricts';

const steps = ['Shipping', 'Payment', 'Review'];

export default function Checkout() {
  const navigate = useNavigate();
  const cartQuery = useCart();
  const summaryQuery = useCartSummary();
  const districtsQuery = useDistricts();
  const checkout = useCheckout();

  const [form, setForm] = useState({
    recipientName: '',
    recipientPhone: '',
    shippingAddressLine: '',
    shippingDistrictId: '',
  });

  const update = (field) => (event) => setForm((prev) => ({ ...prev, [field]: event.target.value }));

  const handleSubmit = (event) => {
    event.preventDefault();
    checkout.mutate(
      { ...form, paymentMethod: 'CashOnDelivery' },
      {
        onSuccess: (order) => {
          navigate(routePaths.customerOrderSuccess, { state: { order } });
        },
      },
    );
  };

  const errorMessage = checkout.error?.response?.data?.title || checkout.error?.message;
  const itemCount = summaryQuery.data?.itemCount ?? cartQuery.data?.length ?? 0;
  const subtotal = summaryQuery.data?.subtotal ?? 0;

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

      <form onSubmit={handleSubmit} className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-6 rounded-xl border border-border bg-surface p-6">
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Shipping Address</p>
            <div className="grid gap-4 sm:grid-cols-2">
              <input
                required
                placeholder="Full Name"
                value={form.recipientName}
                onChange={update('recipientName')}
                className="rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <input
                required
                placeholder="Phone Number (01XXXXXXXXX)"
                value={form.recipientPhone}
                onChange={update('recipientPhone')}
                pattern="01[3-9]\d{8}"
                className="rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
              <select
                required
                value={form.shippingDistrictId}
                onChange={update('shippingDistrictId')}
                className="rounded-md border border-border bg-background px-3 py-2 text-sm sm:col-span-2"
              >
                <option value="">Select district</option>
                {(districtsQuery.data || []).map((district) => (
                  <option key={district.id} value={district.id}>
                    {district.name}
                  </option>
                ))}
              </select>
              <input
                required
                placeholder="Full Address"
                value={form.shippingAddressLine}
                onChange={update('shippingAddressLine')}
                className="sm:col-span-2 rounded-md border border-border bg-background px-3 py-2 text-sm"
              />
            </div>
          </div>
          <div>
            <p className="mb-3 text-sm font-semibold text-heading">Payment Method</p>
            <div className="grid gap-3 sm:grid-cols-3">
              <label className="flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm">
                <input type="radio" name="payment" checked readOnly />
                Cash on Delivery
              </label>
            </div>
            <p className="mt-2 text-xs text-body/50">
              Only Cash on Delivery is available right now — card and mobile-banking gateways aren't connected yet.
            </p>
          </div>
        </div>

        <div className="h-fit space-y-3 rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">Order Summary</p>
          <AsyncState isLoading={summaryQuery.isLoading} isError={summaryQuery.isError} error={summaryQuery.error}>
            <div className="flex justify-between text-sm text-body/70">
              <span>Items ({itemCount})</span>
              <span>৳ {subtotal.toLocaleString()}</span>
            </div>
            <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
              <span>Total</span>
              <span>৳ {subtotal.toLocaleString()}</span>
            </div>
          </AsyncState>
          {errorMessage && <p className="text-sm text-red-600">{errorMessage}</p>}
          <Button type="submit" variant="primary" className="w-full" disabled={checkout.isPending || itemCount === 0}>
            {checkout.isPending ? 'Placing order…' : 'Place Order'}
          </Button>
        </div>
      </form>
    </div>
  );
}
