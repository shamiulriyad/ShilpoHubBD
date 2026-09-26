import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Pagination, QueryState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useProducts } from '../../hooks/useProducts';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';
import { useProducerDirectory } from '../../hooks/useProducerDirectory';
import { toProductCardItem } from '../../utils/productAdapters';
import { fieldClass, EmptyResults } from '../../components/tourism/TravelUI';
import AiProductSearch from '../../components/marketplace/AiProductSearch';

export default function ProductListing() {
  const [params,setParams]=useSearchParams();
  const [form,setForm]=useState({});
  useEffect(()=>setForm(Object.fromEntries(params)),[params]);
  const categories=useCategories(),districts=useDistricts(),makers=useProducerDirectory();
  const page=Math.max(1,Number.parseInt(params.get('page'),10)||1);
  const query=useProducts({page,pageSize:12,...Object.fromEntries(['categoryId','districtId','producerId','search','minPrice','maxPrice','sortBy'].filter(k=>params.get(k)).map(k=>[k,params.get(k)]))});
  const set=(key,value)=>setForm(old=>({...old,[key]:value}));
  const invalidPrice=form.minPrice!==''&&form.maxPrice!==''&&form.minPrice!=null&&form.maxPrice!=null&&Number(form.minPrice)>Number(form.maxPrice);
  const apply=e=>{e.preventDefault();if(invalidPrice)return;const next=new URLSearchParams();Object.entries(form).forEach(([k,v])=>{if(v&&k!=='page')next.set(k,v.trim());});setParams(next);};
  const clear=()=>{setForm({});setParams({});};
  const groups=[['categoryId','Craft category',categories.data||[]],['districtId','District',districts.data||[]],['producerId','Maker',makers.data||[]]];
  const filtersActive=['categoryId','districtId','producerId','search','minPrice','maxPrice'].filter(k=>params.get(k));
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Discover handmade work" description="Browse published products by craft, maker, district and budget." breadcrumbs={[{label:'Home',path:'/'},{label:'Products'}]}/>
    <AiProductSearch params={params} setParams={setParams} categories={categories.data||[]}/>
    {!params.get('ai')&&<>
    <form onSubmit={apply} aria-label="Product search and filters" className="mb-7 rounded-xl border border-border bg-surface p-5 sm:p-6">
      <label className="block text-sm font-medium">Search products<input type="search" value={form.search||''} onChange={e=>set('search',e.target.value)} placeholder="Product name or description…" className={fieldClass}/></label>
      <div className="mt-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">{groups.map(([key,label,options])=><label key={key} className="text-sm font-medium">{label}<select value={form[key]||''} onChange={e=>set(key,e.target.value)} className={fieldClass}><option value="">All {label === 'Craft category' ? 'craft categories' : label.toLowerCase()+'s'}</option>{options.map(o=><option key={o.id} value={o.id}>{o.name}</option>)}</select></label>)}
        <label className="text-sm font-medium">Minimum price (৳)<input type="number" min="0" step="any" value={form.minPrice||''} onChange={e=>set('minPrice',e.target.value)} className={fieldClass} placeholder="No minimum"/></label>
        <label className="text-sm font-medium">Maximum price (৳)<input type="number" min="0" step="any" value={form.maxPrice||''} onChange={e=>set('maxPrice',e.target.value)} className={fieldClass} placeholder="No maximum"/></label>
        <label className="text-sm font-medium">Sort by<select value={form.sortBy||'Newest'} onChange={e=>set('sortBy',e.target.value)} className={fieldClass}>{[['Newest','Newest first'],['PriceLowToHigh','Price: low to high'],['PriceHighToLow','Price: high to low'],['Popular','Most popular'],['TopRated','Top rated']].map(([v,l])=><option key={v} value={v}>{l}</option>)}</select></label>
      </div>
      {invalidPrice&&<p role="alert" className="mt-3 text-sm text-red-700">Maximum price must be equal to or greater than minimum price.</p>}
      {(categories.isError||districts.isError||makers.isError)&&<p role="alert" className="mt-3 text-sm">Some filter options could not load. <button type="button" onClick={()=>{categories.refetch();districts.refetch();makers.refetch();}} className="text-link underline">Retry options</button></p>}
      <div className="mt-5 flex flex-wrap items-center gap-4"><button type="submit" disabled={invalidPrice} className="rounded-lg bg-primary px-5 py-3 text-sm font-semibold text-white disabled:opacity-50">Apply filters</button><button type="button" onClick={clear} className="text-sm font-semibold text-primary underline">Reset all</button><p className="text-xs text-body/65">Price filters use the listed base price. Discounts appear on product cards.</p></div>
    </form>
    {filtersActive.length>0&&<div className="mb-5 flex flex-wrap gap-2" aria-label="Applied filters">{filtersActive.map(key=>{const options=groups.find(g=>g[0]===key)?.[2];const value=options?.find(o=>o.id===params.get(key))?.name||params.get(key);return <button key={key} onClick={()=>{const next=new URLSearchParams(params);next.delete(key);next.delete('page');setParams(next);}} className="rounded-full border border-border bg-surface px-3 py-2 text-xs">{({categoryId:'Craft',districtId:'District',producerId:'Maker',search:'Search',minPrice:'Min ৳',maxPrice:'Max ৳'})[key]}: {value} <span aria-hidden="true">×</span><span className="sr-only">Remove filter</span></button>;})}</div>}
    <p role="status" className="mb-5 text-sm text-body/70">{query.isFetching?'Updating products…':`${query.data?.totalCount??0} products`}</p>
    <QueryState query={query} isEmpty={()=>false}>{data=><><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3" aria-busy={query.isFetching}>{(data?.items||[]).map(p=><ProductCard key={p.id} product={toProductCardItem(p)} to={routePaths.marketplaceProductDetails.replace(':productId',p.id)}/>)}</div>{!data?.items?.length&&<EmptyResults onClear={clear}/>}</>}</QueryState>
    {query.data?.totalPages>1&&<div className="mt-8"><Pagination currentPage={page} totalPages={query.data.totalPages} onPageChange={value=>{const next=new URLSearchParams(params);next.set('page',String(value));setParams(next);}}/></div>}
    </>}
  </div>;
}
