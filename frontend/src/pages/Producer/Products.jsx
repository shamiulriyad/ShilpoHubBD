import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import CardMedia from '../../components/media/CardMedia';
import { useMyProducts } from '../../hooks/useProducts';
import NewProductForm from './NewProductForm';

const tones = { Approved: 'success', Pending: 'secondary', Rejected: 'neutral' };
export default function Products() {
  const query = useMyProducts();
  const [editing, setEditing] = useState(null);
  const products = query.data || [];
  return <div>
    <PageHeader title="My products" description="Create complete listings, monitor approval, and keep customer-facing information accurate." action={<Button onClick={() => setEditing({})}>Add product</Button>}/>
    {editing && <NewProductForm initial={editing.id ? editing : null} onCreated={() => { setEditing(null); query.refetch(); }} onCancel={() => setEditing(null)}/>} 
    <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">{products.map(product => <article key={product.id} className="overflow-hidden rounded-2xl border border-border bg-surface shadow-sm"><CardMedia src={product.imageUrls?.[0]} name={product.name} category={product.categoryName}/><div className="p-5"><div className="flex items-start justify-between gap-3"><div><h2 className="font-semibold text-heading">{product.name}</h2><p className="mt-1 text-sm text-body">{product.categoryName} · {product.districtName}</p></div><Badge tone={tones[product.approvalStatus] || 'neutral'}>{product.approvalStatus}</Badge></div><p className="mt-3 line-clamp-2 text-sm text-body/70">{product.description}</p><div className="mt-4 flex items-center justify-between"><p className="font-semibold text-primary">৳ {(product.discountPrice ?? product.price).toLocaleString()} · {product.stock} in stock</p><Button variant="secondary" onClick={() => setEditing(product)}>Edit</Button></div>{product.rejectionReason && <p role="alert" className="mt-3 rounded-lg bg-red-50 p-3 text-sm text-red-800">Review note: {product.rejectionReason}</p>}</div></article>)}</div>
      {!products.length && <div className="rounded-2xl border border-border bg-surface p-10 text-center"><h2 className="text-xl font-semibold">Launch your first product</h2><p className="my-3 text-body">Add a clear photo, price, stock, description, and origin details for admin review.</p><Button onClick={() => setEditing({})}>Add product</Button></div>}
    </AsyncState>
  </div>;
}
