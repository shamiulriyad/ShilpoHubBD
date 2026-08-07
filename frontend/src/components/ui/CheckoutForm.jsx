const paymentMethods = ['Mobile Banking', 'Card Payment', 'Cash on Delivery'];

export default function CheckoutForm() {
  return (
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
          {paymentMethods.map((method) => (
            <label key={method} className="flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm">
              <input type="radio" name="payment" />
              {method}
            </label>
          ))}
        </div>
      </div>
    </div>
  );
}
