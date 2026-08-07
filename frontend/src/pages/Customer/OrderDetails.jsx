import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader } from '../../components/ui';
import { orders } from '../../data/mockData';

const statusTone = { Delivered: 'success', Shipped: 'primary', Processing: 'secondary' };

export default function OrderDetails() {
  const { orderId } = useParams();
  const order = orders.find((o) => o.id === orderId) || orders[0];
  const subtotal = order.lineItems.reduce((sum, item) => sum + item.price * item.qty, 0);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Order History', path: routePaths.customerOrders },
          { label: order.id },
        ]}
        title={order.id}
        description={`Placed on ${order.date}`}
        action={<Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>}
      />

      <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-8">
          <div>
            <SectionHeader eyebrow="Tracking" title="Order Status" />
            <div className="flex flex-wrap items-center gap-3 rounded-xl border border-border bg-surface p-5">
              {order.trackingSteps.map((step, index) => (
                <div key={step.label} className="flex items-center gap-3">
                  <div className="flex flex-col items-center gap-1">
                    <span
                      className={`flex h-8 w-8 items-center justify-center rounded-full text-xs font-semibold ${
                        step.done ? 'bg-primary text-surface' : 'border border-border text-body/40'
                      }`}
                    >
                      {index + 1}
                    </span>
                    <span className={`text-xs ${step.done ? 'font-medium text-heading' : 'text-body/50'}`}>
                      {step.label}
                    </span>
                    {step.date && <span className="text-[10px] text-body/40">{step.date}</span>}
                  </div>
                  {index < order.trackingSteps.length - 1 && <span className="h-px w-8 bg-border sm:w-14" />}
                </div>
              ))}
            </div>
          </div>

          <div>
            <SectionHeader eyebrow="Items" title="Order Items" />
            <div className="divide-y divide-border rounded-xl border border-border bg-surface">
              {order.lineItems.map((item, i) => (
                <div key={i} className="flex items-center justify-between gap-4 p-4">
                  <div className="flex items-center gap-3">
                    <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-background text-[10px] text-body/40">
                      Item
                    </span>
                    <div>
                      <p className="text-sm font-medium text-heading">{item.product}</p>
                      <p className="text-xs text-body/60">Qty {item.qty}</p>
                    </div>
                  </div>
                  <p className="text-sm font-semibold text-heading">৳ {(item.price * item.qty).toLocaleString()}</p>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="h-fit space-y-4">
          <div className="space-y-3 rounded-xl border border-border bg-surface p-5">
            <p className="text-sm font-semibold text-heading">Order Summary</p>
            <div className="flex justify-between text-sm text-body/70">
              <span>Subtotal</span>
              <span>৳ {subtotal.toLocaleString()}</span>
            </div>
            <div className="flex justify-between text-sm text-body/70">
              <span>Shipping</span>
              <span>৳ {Math.max(order.total - subtotal, 0).toLocaleString()}</span>
            </div>
            <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
              <span>Total</span>
              <span>৳ {order.total.toLocaleString()}</span>
            </div>
            <p className="pt-2 text-xs text-body/60">Paid via {order.paymentMethod}</p>
          </div>

          <div className="space-y-2 rounded-xl border border-border bg-surface p-5">
            <p className="text-sm font-semibold text-heading">Shipping Address</p>
            <p className="text-sm text-body/70">{order.address}</p>
          </div>

          <div className="flex flex-col gap-2">
            <Link to={routePaths.customerReturns}>
              <Button variant="secondary" className="w-full">
                Request a Return
              </Button>
            </Link>
            <Button variant="secondary" className="w-full">
              Download Invoice
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
