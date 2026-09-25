import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useProducts } from '../../hooks/useProducts';
import { useAuth } from '../../hooks/useAuth';
import { roleSidebars, mainNav, megaMenus } from '../../data/navigation';
import { routePaths } from '../../routes/routePaths';
import { presentNavigation } from './workspaceNavigation';
import NavigationIcon from './NavigationIcon';

export default function GlobalSearch({ navItems }) {
  const dialog = useRef(null);
  const input = useRef(null);
  const trigger = useRef(null);
  const [open, setOpen] = useState(false);
  useEffect(()=>{
    const shortcut = event => { if((event.ctrlKey || event.metaKey) && event.key.toLowerCase()==='k') { event.preventDefault(); setOpen(value=>!value); } };
    window.addEventListener('keydown',shortcut);
    return ()=>window.removeEventListener('keydown',shortcut);
  },[]);
  useEffect(()=>{
    if(open) { dialog.current?.showModal(); input.current?.focus(); }
    else if(dialog.current?.open) { dialog.current.close(); trigger.current?.focus(); }
  },[open]);
  return <>
    <button ref={trigger} type="button" onClick={()=>setOpen(true)} className="global-search-trigger" aria-label="Search ShilpoHub" aria-haspopup="dialog"><NavigationIcon label="Search"/><span>Search products or find a workspace page…</span><kbd>Ctrl K</kbd></button>
    <dialog ref={dialog} className="global-search-dialog" aria-label="Search ShilpoHub" onCancel={()=>setOpen(false)} onClick={event=>{if(event.target===dialog.current)setOpen(false);}}>
      {open && <SearchContent inputRef={input} navItems={navItems} onClose={()=>setOpen(false)}/>}
    </dialog>
  </>;
}
function SearchContent({ inputRef, navItems, onClose }) {
  const { activeRole } = useAuth();
  const navigate = useNavigate();
  const [value,setValue] = useState('');
  const [query,setQuery] = useState('');
  useEffect(()=>{ const timer=setTimeout(()=>setQuery(value.trim()),300); return ()=>clearTimeout(timer); },[value]);
  const config = navItems || roleSidebars[activeRole]?.nav;
  const publicLinks = [...mainNav,...Object.values(megaMenus).flatMap(menu=>menu.links || [])];
  const candidates = config ? presentNavigation(config,activeRole).all : publicLinks;
  const links = candidates.filter((item,index,array)=>array.findIndex(other=>other.path===item.path)===index && item.label.toLowerCase().includes(value.trim().toLowerCase())).slice(0,6);
  const productPath = activeRole === 'Customer' ? routePaths.customerProductDetails : routePaths.marketplaceProductDetails;
  const searchPath = activeRole === 'Customer' ? routePaths.customerMarketplace : routePaths.marketplaceProducts;
  return <div>
    <form className="global-search-input" role="search" onSubmit={event=>{event.preventDefault();navigate(`${searchPath}?search=${encodeURIComponent(value.trim())}`);onClose();}}><NavigationIcon label="Search"/><input ref={inputRef} aria-label="Global search" placeholder="Search products or find a page…" value={value} onChange={event=>setValue(event.target.value)}/><button type="button" onClick={onClose} aria-label="Close search">Esc</button></form>
    <div className="global-search-results"><p className="menu-eyebrow">{value ? 'Matching pages' : 'Quick navigation'}</p>{links.map(item=><Link key={item.path} to={item.path} onClick={onClose} className="search-result"><NavigationIcon label={item.label}/><span>{item.label}</span><span aria-hidden="true">↗</span></Link>)}
      {value && !links.length && <p className="px-3 py-3 text-sm text-muted">No matching workspace pages.</p>}
      {query.length>=2 && <ProductResults query={query} productPath={productPath} onClose={onClose}/>}
    </div><div className="search-footer">Find a page by name, or search the product collection.<span>Enter to view all products</span></div>
  </div>;
}

function ProductResults({ query, productPath, onClose }) {
  const results=useProducts({search:query,page:1,pageSize:5});
  return <section className="mt-4 border-t border-border pt-4"><p className="menu-eyebrow">Products</p><div role="status">{results.isFetching&&<p className="px-3 py-2 text-sm text-muted">Searching products…</p>}{results.isError&&<p className="px-3 py-2 text-sm text-error">Product search is unavailable. <button onClick={()=>results.refetch()} className="underline">Retry</button></p>}</div>
    {!results.isFetching&&!results.isError&&(results.data?.items||[]).map(product=><Link key={product.id} to={productPath.replace(':productId',product.id)} onClick={onClose} className="search-result"><NavigationIcon label="Products"/><span><strong className="block font-medium">{product.name}</strong><span className="text-xs text-muted">{product.producerName}{product.districtName ? ` · ${product.districtName}` : ''}</span></span><span aria-hidden="true">↗</span></Link>)}
    {!results.isFetching&&!results.isError&&!results.data?.items?.length&&<p className="px-3 py-2 text-sm text-muted">No products found. Try another search.</p>}
  </section>;
}
