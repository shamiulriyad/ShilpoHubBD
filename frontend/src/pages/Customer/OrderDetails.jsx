import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader, AsyncState } from '../../components/ui';
import { useOrder, useOrderTracking, useOrderMutations } from '../../hooks/useOrders';

const statusTone = {
  Delivered: 'success',
  Shipped: 'primary',
  Processing: 'secondary',
  Pending: 'secondary',
  Cancelled: 'neutral',
  ReturnRequested: 'secondary',
  Returned: 'neutral',
  Refunded: 'neutral',
};

const canCancel = (status) => ['Pending', 'Processing'].includes(status);
const canReturn = (status) => status === 'Delivered';

export default function OrderDetails() {
  const { orderId } = useParams();
  const orderQuery = useOrder(orderId);
  const trackingQuery = useOrderTracking(orderId);
  const { cancel, requestReturn } = useOrderMutations();
  const [returnReason, setReturnReason] = useState('');
  const order = orderQuery.data;

  return (
    <div>
      <AsyncState isLoading={orderQuery.isLoading} isError={orderQuery.isError} error={orderQuery.error}>
        {order && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Dashboard', path: routePaths.customer },
                { label: 'Order History', path: routePaths.customerOrders },
                { label: order.orderNumber },
              ]}
              title={order.orderNumber}
              description={`Placed on ${new Date(order.createdAt).toLocaleDateString()}`}
              action={<Badge tone={statusTone[order.status] || 'neutral'}>{order.status}</Badge>}
            />

            <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
              <div className="space-y-8">
                <div>
                  <SectionHeader eyebrow="Tracking" title="Order Status" />
                  <div className="rounded-xl border border-border bg-surface p-5">
                    <AsyncState isLoading={trackingQuery.isLoading} isError={trackingQuery.isError} error={trackingQuery.error}>
                      {trackingQuery.data?.events.length ? (
                        <div className="space-y-3">
                          {trackingQuery.data.events.map((event, i) => (
                            <div key={i} className="flex items-center justify-between text-sm">
                              <span className="font-medium text-heading">{event.status}</span>
                              <span className="text-body/50">{new Date(event.createdAt).toLocaleString()}</span>
                            </div>
                          ))}
                          {trackingQuery.data.trackingNumber && (
                            <p className="border-t border-border pt-3 text-xs text-body/60">
                              {trackingQuery.data.carrier}: {trackingQuery.data.trackingNumber}
                            </p>
                          )}
                        </div>
                      ) : (
                        <p className="text-sm text-body/60">No tracking events yet.</p>
                      )}
                    </AsyncState>
                  </div>
                </div>

                <div>
                  <SectionHeader eyebrow="Items" title="Order Items" />
                  <div className="divide-y divide-border rounded-xl border border-border bg-surface">
                    {order.items.map((item) => (
                      <div key={item.id} className="flex items-center justify-between gap-4 p-4">
                        <div className="flex items-center gap-3">
                          <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-background text-[10px] text-body/40">
                            {item.productImageUrl ? (
                              <img src={item.productImageUrl} alt="" className="h-full w-full rounded-lg object-cover" />
                            ) : (
                              'Item'
                            )}
                          </span>
                          <div>
                            <p className="text-sm font-medium text-heading">{item.productName}</p>
                            <p className="text-xs text-body/60">
                              Qty {item.quantity}
                              {item.variantName ? ` · ${item.variantName}` : ''}
                            </p>
                          </div>
                        </div>
                        <p className="text-sm font-semibold text-heading">৳ {item.lineTotal.toLocaleString()}</p>
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
                    <span>৳ {order.subtotal.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between border-t border-border pt-3 text-sm font-semibold text-heading">
                    <span>Total</span>
                    <span>৳ {order.total.toLocaleString()}</span>
                  </div>
                  <p className="pt-2 text-xs text-body/60">Paid via {order.paymentMethod}</p>
                </div>

                <div className="space-y-2 rounded-xl border border-border bg-surface p-5">
                  <p className="text-sm font-semibold text-heading">Shipping Address</p>
                  <p className="text-sm text-body/70">{order.recipientName}</p>
                  <p className="text-sm text-body/70">{order.shippingAddressLine}, {order.shippingDistrictName}</p>
                  <p className="text-xs text-body/60">{order.recipientPhone}</p>
                </div>

                <div className="flex flex-col gap-2">
                  {canCancel(order.status) && (
                    <Button
                      variant="secondary"
                      className="w-full"
                      disabled={cancel.isPending}
                      onClick={() => cancel.mutate({ id: order.id })}
                    >
                      {cancel.isPending ? 'Cancelling…' : 'Cancel Order'}
                    </Button>
                  )}
                  {canReturn(order.status) && (
                    <div className="space-y-2 rounded-xl border border-border bg-surface p-4">
                      <textarea
                        placeholder="Reason for return…"
                        value={returnReason}
                        onChange={(event) => setReturnReason(event.target.value)}
                        className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
                        rows={2}
                      />
                      <Button
                        variant="secondary"
                        className="w-full"
                        disabled={!returnReason.trim() || requestReturn.isPending}
                        onClick={() => requestReturn.mutate({ id: order.id, reason: returnReason })}
                      >
                        {requestReturn.isPending ? 'Requesting…' : 'Request a Return'}
                      </Button>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
