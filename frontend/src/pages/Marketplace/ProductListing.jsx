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
  const filterLabels={categoryId:'Craft',districtId:'District',producerId:'Maker',search:'Search',minPrice:'Min ৳',maxPrice:'Max ৳'};
  const aiMode=Boolean(params.get('ai'));
  const removeFilter=key=>{const next=new URLSearchParams(params);next.delete(key);next.delete('page');setParams(next);};
  const sidebar=<form onSubmit={apply} aria-label="Product search and filters" className="rounded-xl border border-border bg-surface p-4 lg:sticky lg:top-24">
    <div className="mb-4 flex items-center justify-between"><h2 className="text-base font-semibold text-heading">Filters</h2><button type="button" onClick={clear} className="text-xs font-semibold text-primary underline">Reset all</button></div>
    <div className="space-y-4">
      <label className="block text-sm font-medium">Search<input type="search" value={form.search||''} onChange={e=>set('search',e.target.value)} placeholder="Name or description…" className={fieldClass}/></label>
      {groups.map(([key,label,options])=><label key={key} className="block text-sm font-medium">{label}<select value={form[key]||''} onChange={e=>set(key,e.target.value)} className={fieldClass}><option value="">All {label==='Craft category'?'craft categories':label.toLowerCase()+'s'}</option>{options.map(o=><option key={o.id} value={o.id}>{o.name}</option>)}</select></label>)}
      <fieldset><legend className="text-sm font-medium">Price range (৳)</legend>
        <div className="mt-1 grid grid-cols-2 gap-2"><input aria-label="Minimum price" type="number" min="0" step="any" value={form.minPrice||''} onChange={e=>set('minPrice',e.target.value)} className={fieldClass} placeholder="Min"/><input aria-label="Maximum price" type="number" min="0" step="any" value={form.maxPrice||''} onChange={e=>set('maxPrice',e.target.value)} className={fieldClass} placeholder="Max"/></div>
      </fieldset>
      {invalidPrice&&<p role="alert" className="text-sm text-red-700">Max price must be at least the min price.</p>}
      <label className="block text-sm font-medium">Sort by<select value={form.sortBy||'Newest'} onChange={e=>set('sortBy',e.target.value)} className={fieldClass}>{[['Newest','Newest first'],['PriceLowToHigh','Price: low to high'],['PriceHighToLow','Price: high to low'],['Popular','Most popular'],['TopRated','Top rated']].map(([v,l])=><option key={v} value={v}>{l}</option>)}</select></label>
    </div>
    {(categories.isError||districts.isError||makers.isError)&&<p role="alert" className="mt-3 text-sm">Some filter options could not load. <button type="button" onClick={()=>{categories.refetch();districts.refetch();makers.refetch();}} className="text-link underline">Retry</button></p>}
    <button type="submit" disabled={invalidPrice} className="mt-5 w-full rounded-lg bg-primary px-5 py-3 text-sm font-semibold text-white disabled:opacity-50">Apply filters</button>
    <p className="mt-3 text-xs text-body/65">Price filters use the listed base price. Discounts appear on product cards.</p>
  </form>;
  return <div className="mx-auto max-w-7xl px-4 py-8 lg:px-6">
    <PageHeader title="Discover handmade work" description="Browse published products by craft, maker, district and budget." breadcrumbs={[{label:'Home',path:'/'},{label:'Products'}]}/>
    <AiProductSearch params={params} setParams={setParams} categories={categories.data||[]}/>
    {!aiMode&&<div className="grid gap-6 lg:grid-cols-[280px_minmax(0,1fr)] lg:items-start">
      <aside>{sidebar}</aside>
      <section aria-label="Products">
        <div className="mb-4 flex flex-wrap items-center gap-2">
          <p role="status" className="mr-2 text-sm text-body/70">{query.isFetching?'Updating products…':`${query.data?.totalCount??0} products`}</p>
          {filtersActive.map(key=>{const options=groups.find(g=>g[0]===key)?.[2];const value=options?.find(o=>o.id===params.get(key))?.name||params.get(key);return <button key={key} onClick={()=>removeFilter(key)} className="rounded-full border border-border bg-surface px-3 py-1.5 text-xs">{filterLabels[key]}: {value} <span aria-hidden="true">×</span><span className="sr-only">Remove filter</span></button>;})}
        </div>
        <QueryState query={query} isEmpty={()=>false}>{data=><><div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3" aria-busy={query.isFetching}>{(data?.items||[]).map(p=><ProductCard key={p.id} product={toProductCardItem(p)} to={routePaths.marketplaceProductDetails.replace(':productId',p.id)}/>)}</div>{!data?.items?.length&&<EmptyResults onClear={clear}/>}</>}</QueryState>
        {query.data?.totalPages>1&&<div className="mt-8"><Pagination currentPage={page} totalPages={query.data.totalPages} onPageChange={value=>{const next=new URLSearchParams(params);next.set('page',String(value));setParams(next);}}/></div>}
      </section>
    </div>}
  </div>;
}
