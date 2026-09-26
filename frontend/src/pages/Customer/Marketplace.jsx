import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { PageHeader, SearchBar, AsyncState, Pagination } from '../../components/ui';
import ShoppingCartLink from '../../components/ui/ShoppingCartLink';
import { ProductCard } from '../../components/cards';
import { useProducts } from '../../hooks/useProducts';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';
import { priceRangeToQuery } from '../../components/ui/MarketplaceFilter';
import { toProductCardItem } from '../../utils/productAdapters';
import { routePaths } from '../../routes/routePaths';
import AiProductSearch from '../../components/marketplace/AiProductSearch';

export default function Marketplace() {
  const [params, setParams] = useSearchParams();
  const search = params.get('search') || '';
  const [input, setInput] = useState(search);
  useEffect(() => setInput(search), [search]);
  const page = Math.max(1, Number.parseInt(params.get('page'), 10) || 1);
  const categories = useCategories();
  const districts = useDistricts();
  const products = useProducts({ search: search || undefined, categoryId: params.get('categoryId') || undefined, districtId: params.get('districtId') || undefined, expertise: params.get('expertise') || undefined, ...priceRangeToQuery(params.get('priceRange')), sortBy: params.get('sortBy') || 'Newest', page, pageSize: 12 });
  const change = (key, value) => {
    const next = new URLSearchParams(params);
    if (value) next.set(key, value); else next.delete(key);
    if (key !== 'page') next.delete('page');
    setParams(next);
  };
  const groups = [
    ['categoryId', 'Category', (categories.data || []).map(c => [c.id, c.name])],
    ['districtId', 'District', (districts.data || []).map(d => [d.id, d.name])],
    ['priceRange', 'Price range', [['under-1000','Under ৳1,000'],['1000-3000','৳1,000–3,000'],['3000-6000','৳3,000–6,000'],['above-6000','Above ৳6,000']]],
    ['sortBy', 'Sort by', [['Newest','Newest'],['PriceLowToHigh','Price: low to high'],['PriceHighToLow','Price: high to low'],['Popular','Most popular'],['TopRated','Top rated']]],
  ];
  return <div>
    <PageHeader title="Marketplace" description="Find handmade pieces for everyday living. Search by product name or description." breadcrumbs={[{label:'Dashboard',path:routePaths.customer},{label:'Marketplace'}]} action={<ShoppingCartLink/>}/>
    <AiProductSearch params={params} setParams={setParams} categories={categories.data || []} productPath={routePaths.customerProductDetails}/>
    {!params.get('ai') && <>
    <section aria-label="Search and filter products" className="mb-8 space-y-5 rounded-2xl border border-border bg-surface p-5 sm:p-6">
      <SearchBar placeholder="Search products…" value={input} onChange={e => setInput(e.target.value)} onSubmit={value => change('search', (value || '').trim())}/>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{groups.map(([key,label,options]) => <label key={key} className="space-y-2 text-sm font-medium text-heading"><span>{label}</span><select className="block min-h-11 w-full rounded-lg border border-border bg-background px-3 focus:outline-primary" value={params.get(key) || (key === 'sortBy' ? 'Newest' : '')} onChange={e => change(key,e.target.value)}>{key !== 'sortBy' && <option value="">All {label.toLowerCase()}</option>}{options.map(([value,name]) => <option key={value} value={value}>{name}</option>)}</select></label>)}</div>
      {(categories.isError || districts.isError) && <p role="alert" className="text-sm">Some filters could not load. <button className="underline" onClick={() => { categories.refetch(); districts.refetch(); }}>Retry filters</button></p>}
      {params.size > 0 && <button onClick={() => { setInput(''); setParams({}); }} className="text-sm font-semibold text-primary underline">Clear search and filters</button>}
    </section>
    <div className="mb-5 flex flex-wrap items-center justify-between gap-3"><h2 className="text-xl font-semibold text-heading">{search ? `Results for “${search}”` : 'Explore the collection'}</h2><p role="status" className="text-sm text-body">{products.isFetching ? 'Updating products…' : products.isError ? 'Products could not load' : `${products.data?.totalCount ?? 0} products`}</p></div>
    <AsyncState isLoading={products.isLoading} isError={products.isError} error={products.error}>
      <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3" aria-busy={products.isFetching}>{(products.data?.items || []).map(product => <ProductCard key={product.id} product={toProductCardItem(product)} to={routePaths.customerProductDetails.replace(':productId',product.id)}/>)}</div>
      {products.isSuccess && !products.data?.items?.length && <div className="rounded-2xl border border-border bg-surface p-10 text-center"><h3 className="text-lg font-semibold">No matching products</h3><p className="my-3 text-body">Try a different product name or clear your filters.</p><button className="font-semibold text-primary underline" onClick={() => {setInput('');setParams({});}}>Show all products</button></div>}
    </AsyncState>
    {products.isError && <button className="mt-4 rounded-lg border border-border px-5 py-3" onClick={() => products.refetch()}>Retry loading products</button>}
    {!products.isError && products.data?.totalPages > 1 && <div className="mt-8"><Pagination currentPage={page} totalPages={products.data.totalPages} onPageChange={value => change('page',String(value))}/></div>}
    </>}
  </div>;
}
