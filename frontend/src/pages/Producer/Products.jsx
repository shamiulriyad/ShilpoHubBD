import { useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import CardMedia from '../../components/media/CardMedia';
import { useMyProducts } from '../../hooks/useProducts';
import NewProductForm from './NewProductForm';

const tones = { Approved: 'success', Pending: 'secondary', Rejected: 'neutral' };
const STATUS_TEXT = { Approved: 'Live', Pending: 'Waiting for review', Rejected: 'Needs changes' };

const isOut = (p) => p.stock === 0;
const isLow = (p) => p.stock > 0 && p.lowStockThreshold != null && p.stock <= p.lowStockThreshold;
const FILTERS = [
  ['all', 'All', () => true],
  ['Approved', 'Live', (p) => p.approvalStatus === 'Approved'],
  ['Pending', 'Waiting for review', (p) => p.approvalStatus === 'Pending'],
  ['Rejected', 'Needs changes', (p) => p.approvalStatus === 'Rejected'],
  ['low', 'Low stock', isLow],
  ['out', 'Out of stock', isOut],
];

export default function Products() {
  const query = useMyProducts();
  const [params, setParams] = useSearchParams();
  const [editing, setEditing] = useState(null);
  const products = query.data || [];
  const filter = FILTERS.find(([key]) => key === params.get('status')) || FILTERS[0];
  const counts = useMemo(() => Object.fromEntries(FILTERS.map(([key, , test]) => [key, products.filter(test).length])), [products]);
  const shown = products.filter(filter[2]);

  // "Edit" on the details page comes here with ?edit=<id>.
  const editId = params.get('edit');
  const editProduct = editing ?? (editId ? products.find((p) => p.id === editId) : null);
  const closeEditor = () => {
    setEditing(null);
    if (editId) { const next = new URLSearchParams(params); next.delete('edit'); setParams(next, { replace: true }); }
  };
  const setFilter = (key) => { const next = new URLSearchParams(); if (key !== 'all') next.set('status', key); setParams(next); };

  return <div>
    <PageHeader title="My products" description="See everything you sell, check what is live, and keep listings accurate." action={<Button onClick={() => setEditing({})}>Add product</Button>}/>
    {editProduct && <NewProductForm initial={editProduct.id ? editProduct : null} onCreated={() => { closeEditor(); query.refetch(); }} onCancel={closeEditor}/>}

    <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>
      <section aria-label="Product summary" className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4 xl:grid-cols-6">
        <StatCard label="Total products" value={products.length}/>
        <StatCard label="Live" value={counts.Approved}/>
        <StatCard label="Waiting for review" value={counts.Pending}/>
        <StatCard label="Needs changes" value={counts.Rejected}/>
        <StatCard label="Low stock" value={counts.low}/>
        <StatCard label="Out of stock" value={counts.out}/>
      </section>

      {products.length > 0 && <div className="mb-5 flex flex-wrap items-center gap-2" role="group" aria-label="Filter products">
        {FILTERS.map(([key, label]) => <button key={key} type="button" onClick={() => setFilter(key)} aria-pressed={filter[0] === key}
          className={`rounded-full border px-4 py-1.5 text-sm font-medium transition ${filter[0] === key ? 'border-primary bg-primary text-surface' : 'border-border bg-surface text-heading hover:border-primary/40'}`}>
          {label} <span className="opacity-70">({counts[key]})</span>
        </button>)}
      </div>}
      <p role="status" className="mb-4 text-sm text-body/70">Showing {shown.length} of {products.length} products</p>

      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">{shown.map(product => <article key={product.id} className="flex flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-sm">
        <Link to={routePaths.producerProductDetails.replace(':productId', product.id)} aria-label={`See details of ${product.name}`}>
          <CardMedia src={product.imageUrls?.[0]} name={product.name} category={product.categoryName}/>
        </Link>
        <div className="flex flex-1 flex-col p-5">
          <div className="flex items-start justify-between gap-3">
            <div><h2 className="font-semibold text-heading">{product.name}</h2><p className="mt-1 text-sm text-body">{product.categoryName} · {product.districtName}</p></div>
            <Badge tone={tones[product.approvalStatus] || 'neutral'}>{STATUS_TEXT[product.approvalStatus] || product.approvalStatus}</Badge>
          </div>
          <p className="mt-3 line-clamp-2 text-sm text-body/70">{product.description}</p>
          <p className="mt-4 font-semibold text-primary">৳ {(product.discountPrice ?? product.price).toLocaleString()}
            <span className={`ml-2 text-sm font-medium ${isOut(product) ? 'text-error' : isLow(product) ? 'text-secondary' : 'text-body/70'}`}>· {isOut(product) ? 'Out of stock' : `${product.stock} in stock${isLow(product) ? ' (low)' : ''}`}</span>
          </p>
          {product.rejectionReason && <p role="alert" className="mt-3 rounded-lg bg-red-50 p-3 text-sm text-red-800">Review note: {product.rejectionReason}</p>}
          <div className="mt-auto flex flex-wrap items-center gap-2 pt-5">
            <Link to={routePaths.producerProductDetails.replace(':productId', product.id)} className="rounded-lg bg-primary px-3.5 py-2 text-sm font-semibold text-surface hover:bg-primary-dark">See details</Link>
            <Button variant="secondary" onClick={() => setEditing(product)}>Edit</Button>
            <Link to={routePaths.producerProductAttributes.replace(':productId', product.id)} className="ml-auto text-sm font-medium text-primary hover:underline" title="Add types, materials and keywords so shoppers and AI search can find this product">AI search info</Link>
          </div>
        </div>
      </article>)}</div>

      {products.length > 0 && !shown.length && <div className="rounded-2xl border border-border bg-surface p-8 text-center text-sm text-body/70">No products in “{filter[1]}”. <button type="button" className="font-semibold text-primary underline" onClick={() => setFilter('all')}>Show all products</button></div>}
      {!products.length && <div className="rounded-2xl border border-border bg-surface p-10 text-center"><h2 className="text-xl font-semibold">Launch your first product</h2><p className="my-3 text-body">Add a clear photo, price, stock and a short story. An admin reviews it before it goes live.</p><Button onClick={() => setEditing({})}>Add product</Button></div>}
    </AsyncState>
  </div>;
}
