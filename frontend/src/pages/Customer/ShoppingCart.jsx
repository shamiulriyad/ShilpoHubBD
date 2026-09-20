import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import CardMedia from '../../components/media/CardMedia';
import { useCart, useCartMutations } from '../../hooks/useCart';

export default function ShoppingCart() {
  const cart = useCart();
  const { updateQuantity, remove, add } = useCartMutations();
  const [removed, setRemoved] = useState(null);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const items = cart.data || [];
  const busy = cart.isFetching || updateQuantity.isPending || remove.isPending || add.isPending;
  const subtotal = items.reduce((sum,item) => sum + item.unitPrice * item.quantity, 0);
  const run = async (mutation, payload, success) => {
    setError(''); setMessage('');
    try { await mutation.mutateAsync(payload); success(); }
    catch { setError('Your change could not be saved. Check your connection and stock availability, then try again.'); }
  };
  return <div>
    <PageHeader title="Shopping cart" description="Review your pieces and quantities before checkout." breadcrumbs={[{label:'Marketplace',path:routePaths.customerMarketplace},{label:'Cart'}]} action={<Link className="font-medium text-primary underline" to={routePaths.customerMarketplace}>Continue shopping</Link>}/>
    {error && <p role="alert" className="mb-5 rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">{error}</p>}
    <div role="status" aria-live="polite">{message && <p className="mb-4">{message}</p>}{removed && <div className="mb-5 flex flex-wrap items-center gap-3 rounded-lg border border-border bg-surface p-4"><span>{removed.productName} removed.</span><button disabled={busy} className="font-semibold text-primary underline disabled:opacity-50" onClick={() => run(add, {productId:removed.productId,productVariantId:removed.productVariantId,quantity:removed.quantity}, () => {setRemoved(null);setMessage('Item restored to your cart.');})}>Undo removal</button></div>}</div>
    <AsyncState isLoading={cart.isLoading} isError={cart.isError} error={cart.error}>
      {items.length ? <div className="grid items-start gap-6 xl:grid-cols-[minmax(0,2fr)_minmax(280px,1fr)]">
        <div className="divide-y divide-border rounded-2xl border border-border bg-surface">{items.map(item => <article key={item.id} className="flex flex-wrap gap-5 p-5 sm:flex-nowrap">
          <Link to={routePaths.customerProductDetails.replace(':productId',item.productId)} className="w-32 shrink-0 overflow-hidden rounded-xl"><CardMedia src={item.primaryImageUrl} name={item.productName}/></Link>
          <div className="min-w-0 flex-1"><Link to={routePaths.customerProductDetails.replace(':productId',item.productId)} className="font-semibold text-heading hover:underline">{item.productName}</Link>{item.variantName && <p className="mt-1 text-sm">{item.variantName}</p>}<p className="mt-2 text-sm">৳ {item.unitPrice.toLocaleString()} each</p>
            <div className="mt-4 flex flex-wrap items-center gap-4"><div className="inline-flex items-center rounded-lg border border-border"><button aria-label={`Decrease quantity of ${item.productName}`} disabled={busy || item.quantity <= 1} className="h-11 w-11 disabled:opacity-30" onClick={() => run(updateQuantity,{itemId:item.id,quantity:item.quantity-1},() => setMessage('Quantity updated.'))}>−</button><span className="min-w-8 text-center" aria-label="Quantity">{item.quantity}</span><button aria-label={`Increase quantity of ${item.productName}`} disabled={busy} className="h-11 w-11 disabled:opacity-30" onClick={() => run(updateQuantity,{itemId:item.id,quantity:item.quantity+1},() => setMessage('Quantity updated.'))}>+</button></div><button disabled={busy} className="min-h-11 text-sm text-primary underline disabled:opacity-50" onClick={() => run(remove,item.id,() => setRemoved(item))}>Remove<span className="sr-only"> {item.productName}</span></button></div>
          </div><p className="font-semibold text-heading">৳ {(item.unitPrice*item.quantity).toLocaleString()}</p>
        </article>)}</div>
        <aside className="rounded-2xl border border-border bg-surface p-6"><h2 className="text-lg font-semibold text-heading">Order summary</h2><div className="my-5 flex justify-between border-b border-border pb-5"><span>Subtotal ({items.reduce((n,i) => n+i.quantity,0)} items)</span><strong>৳ {subtotal.toLocaleString()}</strong></div><p className="mb-5 text-sm text-body">Review delivery details and the final amount at checkout.</p>{busy ? <button disabled className="w-full rounded-full bg-primary/50 px-5 py-3 text-white">Updating cart…</button> : <Link to={routePaths.customerCheckout} className="block rounded-full bg-primary px-5 py-3 text-center font-semibold text-white hover:opacity-90">Proceed to checkout</Link>}</aside>
      </div> : <div className="rounded-2xl border border-border bg-surface px-6 py-16 text-center"><h2 className="text-2xl font-semibold text-heading">Your next treasured piece awaits</h2><p className="mb-6 mt-3 text-body">Your cart is empty. Explore the collection and add something you love.</p><Link to={routePaths.customerMarketplace} className="inline-block rounded-full bg-primary px-6 py-3 font-semibold text-white">Browse marketplace</Link></div>}
    </AsyncState>
    {cart.isError && <button className="mt-4 rounded-lg border border-border px-5 py-3" onClick={() => cart.refetch()}>Retry loading cart</button>}
  </div>;
}
