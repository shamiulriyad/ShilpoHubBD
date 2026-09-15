import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { SearchBar, SectionHeader, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { useFeaturedProducts, useProducts } from '../../hooks/useProducts';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useCourses } from '../../hooks/useCourses';
import { toProductCardItem } from '../../utils/productAdapters';
import SafeImage from '../../components/media/SafeImage';
import BangladeshMap from '../../components/media/BangladeshMap';

const images = { weaver: '/images/heritage-weaver.png', landscape: '/images/heritage-landscape.png', crafts: '/images/heritage-crafts.png' };
const listOf = (data) => Array.isArray(data) ? data : data?.items || [];
const buttonClass = 'inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white transition hover:bg-primary-dark';
function Photo({ image, alt, className = '' }) {
  return <SafeImage src={image} alt={alt} loading="lazy" className={`h-full w-full object-cover ${className}`} />;
}
function EditorialCard({ title, text, image, to, label }) {
  return <Link to={to} className="group overflow-hidden rounded-2xl border border-border bg-surface shadow-sm transition hover:-translate-y-1 hover:shadow-lg"><div className="aspect-[4/3] overflow-hidden"><Photo image={image} alt="" className="transition duration-700 group-hover:scale-105" /></div><div className="p-6"><p className="text-[10px] font-bold uppercase tracking-[0.2em] text-primary">{label}</p><h3 className="mt-2 text-xl font-semibold text-heading">{title}</h3><p className="mt-2 text-sm leading-6 text-muted">{text}</p><span className="mt-5 inline-block text-sm font-semibold text-primary">Explore collection ↗</span></div></Link>;
}

export default function HomePage() {
  const navigate = useNavigate();
  const [selectedDistrictId, setSelectedDistrictId] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const featuredQuery = useFeaturedProducts(6);
  const catalogQuery = useProducts({ pageSize: 6 });
  const festivalsQuery = useHeritageFestivals({ pageSize: 3 });
  const coursesQuery = useCourses({ pageSize: 3 });
  const districts = listOf(districtsQuery.data);
  const villages = listOf(villagesQuery.data);
  const featured = listOf(featuredQuery.data);
  const products = featured.length ? featured : listOf(catalogQuery.data);
  const productQuery = featured.length ? featuredQuery : catalogQuery;
  const selectedDistrict = districts.find(item => item.id === selectedDistrictId);
  const producers = [...new Map(listOf(catalogQuery.data).filter(p=>p.producerId).map(p=>[p.producerId,{id:p.producerId,name:p.producerName,craft:p.categoryName,district:p.districtName}])).values()];
  return <div className="bg-background">
    <section className="mx-auto grid max-w-7xl items-center gap-10 px-5 py-12 lg:grid-cols-[.95fr_1.05fr] lg:px-8 lg:py-20">
      <div>
        <p className="eyebrow">Crafted in Bangladesh</p>
        <h1 className="mt-5 max-w-xl text-5xl font-medium leading-[1.08] tracking-[-0.045em] text-heading sm:text-6xl lg:text-7xl" style={{fontFamily:'Georgia, serif'}}>A living heritage.<br /><span className="italic text-primary">A story to share.</span></h1>
        <p className="mt-6 max-w-lg text-base leading-7 text-muted">Discover the craft, people and places that make Bangladesh extraordinary. Thoughtful objects. Generations of skill. Connections that last.</p>
        <div className="mt-8 flex flex-wrap gap-3"><Link to={routePaths.marketplaceProducts} className={buttonClass}>Shop the marketplace ↗</Link><Link to={routePaths.explore} className="rounded-full border border-border bg-surface px-6 py-3 text-sm font-semibold text-heading hover:border-primary">Discover our heritage</Link></div>
        <div className="mt-9 max-w-lg"><SearchBar placeholder="Find a product or craft…" value={searchInput} onChange={event=>setSearchInput(event.target.value)} onSubmit={query=>navigate(`${routePaths.marketplaceProducts}?search=${encodeURIComponent(query || '')}`)} /></div>
        <p className="mt-5 text-xs text-muted">Made by people. Rooted in place. Shared with you.</p>
      </div>
      <figure className="relative m-0">
        <div className="aspect-[4/5] overflow-hidden rounded-t-[10rem] rounded-b-3xl border border-border sm:aspect-[5/4] lg:aspect-[4/5]"><SafeImage loading="eager" src={images.weaver} alt="Illustrative scene of a Bangladeshi artisan working at a handloom" fetchPriority="high" className="h-full w-full object-cover object-[42%_center]" /></div>
        <figcaption className="absolute bottom-6 left-5 right-5 rounded-2xl border border-white/40 bg-white/90 p-5 shadow-lg backdrop-blur"><p className="text-[10px] font-bold uppercase tracking-widest text-primary">The hands behind the heritage</p><p className="mt-1 text-xl text-slate-800" style={{fontFamily:'Georgia,serif'}}>Every thread carries a tradition.</p><span className="mt-2 block text-[10px] text-slate-500">Editorial illustration</span></figcaption>
      </figure>
    </section>

    <section aria-label="Explore the ShilpoHub directory" className="border-y border-border bg-surface"><div className="mx-auto grid max-w-7xl grid-cols-3 divide-x divide-border px-5 py-7 lg:px-8">{[
      ['Districts to discover', districtsQuery.isSuccess ? districts.length : '—'],['Heritage villages',villagesQuery.isSuccess ? villages.length : '—'],['Marketplace products',catalogQuery.data?.totalCount ?? '—'],
    ].map(([label,value])=><div key={label} className="px-3 text-center"><p className="text-2xl font-semibold text-heading sm:text-3xl">{value}</p><p className="mt-1 text-xs text-muted sm:text-sm">{label}</p></div>)}</div></section>

    <section className="mx-auto max-w-7xl px-5 py-16 lg:px-8 lg:py-20"><SectionHeader eyebrow="Discover" title="Many ways to belong." description="Start with a place, an object, or a skill. Follow the story from there." /><div className="grid gap-6 sm:grid-cols-3">
      <EditorialCard title="Places with a past" text="Explore villages, waterways and regional traditions." label="01 / Places" image={images.landscape} to={routePaths.exploreVillages} />
      <EditorialCard title="Objects with meaning" text="Discover textiles, pottery and everyday craft." label="02 / Craft" image={images.crafts} to={routePaths.exploreCrafts} />
      <EditorialCard title="Skills worth sharing" text="Learn from the craft traditions that connect generations." label="03 / Learning" image={images.weaver} to={routePaths.academy} />
    </div><div className="mt-6 flex flex-wrap gap-3">{[['District directory',routePaths.exploreDistricts],['Digital museum',routePaths.exploreMuseum],['UNESCO heritage',routePaths.exploreUnesco]].map(([label,to])=><Link key={to} to={to} className="rounded-full border border-border px-4 py-2 text-xs font-medium text-heading hover:bg-surface">{label} ↗</Link>)}</div></section>

    <section className="border-y border-border bg-surface"><div className="mx-auto max-w-7xl px-5 py-16 lg:px-8"><SectionHeader eyebrow="The marketplace" title="Made with care. Chosen with purpose." description="Discover products from the ShilpoHub producer community." action={<Link to={routePaths.marketplaceProducts} className="text-sm font-semibold text-primary">Shop all products ↗</Link>} /><div className="grid gap-6 lg:grid-cols-[.75fr_2fr]"><div className="relative min-h-64 overflow-hidden rounded-2xl"><Photo image={images.crafts} alt="Illustrative arrangement of pottery, woven cloth and a basket" /><div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-slate-950/80 to-transparent p-6 pt-20 text-white"><p className="text-xl font-medium">A little heritage,<br />for everyday life.</p><p className="mt-2 text-xs text-white/70">Editorial collection</p></div></div><AsyncState isLoading={productQuery.isLoading} isError={productQuery.isError} error={productQuery.error}><div className="grid grid-cols-2 gap-4 xl:grid-cols-3">{products.map(product=><ProductCard key={product.id} product={toProductCardItem(product)} to={routePaths.marketplaceProductDetails.replace(':productId',product.id)} />)}{products.length===0 && <p className="col-span-full self-center p-6 text-sm text-muted">The collection is growing. Explore the marketplace or join as a producer to share your work.</p>}</div></AsyncState></div></div></section>

    <section className="mx-auto grid max-w-7xl items-center gap-10 px-5 py-16 lg:grid-cols-2 lg:px-8 lg:py-20"><div className="aspect-[5/4] overflow-hidden rounded-3xl"><Photo image={images.weaver} alt="Editorial illustration celebrating handloom craft" /></div><div><p className="eyebrow">The producer community</p><h2 className="mt-4 text-3xl font-semibold tracking-tight sm:text-4xl">Behind every object,<br />there is a maker.</h2><p className="mt-4 text-sm leading-7 text-muted">Meet the people sharing their craft through ShilpoHub. Discover their work, explore their stories and support their next chapter.</p><div className="mt-6 divide-y divide-border">{producers.slice(0,3).map(producer=><Link key={producer.id} to={routePaths.exploreProducerDetails.replace(':producerId',producer.id)} className="flex items-center gap-4 py-4"><span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-primary-soft font-semibold text-primary">{producer.name?.slice(0,1)}</span><div className="min-w-0"><h3 className="text-sm font-semibold">{producer.name}</h3><p className="text-xs text-muted">{producer.craft} · {producer.district}</p></div><span className="ml-auto text-primary">↗</span></Link>)}</div><Link to={routePaths.exploreProducers} className="mt-6 inline-block text-sm font-semibold text-primary">Meet the community ↗</Link></div></section>

    <section className="border-y border-border bg-surface"><div className="mx-auto max-w-7xl px-5 py-16 lg:px-8"><SectionHeader eyebrow="Across Bangladesh" title="Find the story near you." description="Choose a district and start exploring its heritage." /><div className="grid gap-7 lg:grid-cols-[1.5fr_1fr]"><BangladeshMap selectedDistrict={selectedDistrict?.name} /><div className="overflow-hidden rounded-3xl border border-border"><div className="h-44"><Photo image={images.landscape} alt="Illustrative Bengal riverside landscape" /></div><div className="p-6"><label className="block text-sm font-semibold">Choose a district<select value={selectedDistrictId} onChange={event=>setSelectedDistrictId(event.target.value)} className="mt-3 w-full rounded-xl border border-border bg-background px-4 py-3 text-sm"><option value="">All Bangladesh</option>{districts.map(district=><option key={district.id} value={district.id}>{district.name}</option>)}</select></label>{districtsQuery.isError && <p className="mt-2 text-sm text-error">Districts could not be loaded. Please refresh to try again.</p>}<Link to={selectedDistrict ? routePaths.exploreDistrictDetails.replace(':districtId',selectedDistrict.id) : routePaths.exploreDistricts} className="mt-5 inline-block text-sm font-semibold text-primary">{selectedDistrict ? `Explore ${selectedDistrict.name}` : 'Browse the district directory'} ↗</Link></div></div></div></div></section>

    <section className="mx-auto max-w-7xl px-5 py-16 lg:px-8"><SectionHeader eyebrow="Keep discovering" title="Go further into the story." description="Places to explore, traditions to celebrate, and skills to learn." /><div className="grid gap-6 lg:grid-cols-3">{[
      {title:'Heritage villages',image:images.landscape,to:routePaths.exploreVillages,query:villagesQuery,items:villages.slice(0,3),empty:'Explore the village directory as new stories are added.'},
      {title:'Festivals & events',image:images.crafts,to:routePaths.tourismFestivals,query:festivalsQuery,items:listOf(festivalsQuery.data),empty:'New cultural events will appear here when published.'},
      {title:'Heritage academy',image:images.weaver,to:routePaths.academy,query:coursesQuery,items:listOf(coursesQuery.data),empty:'Browse the academy for available learning opportunities.'},
    ].map(section=><article key={section.title} className="overflow-hidden rounded-2xl border border-border bg-surface"><div className="h-52"><Photo image={section.image} alt="" /></div><div className="p-6"><h3 className="text-xl font-semibold">{section.title}</h3><AsyncState isLoading={section.query.isLoading} isError={section.query.isError} error={section.query.error}><ul className="mt-4 space-y-3">{section.items.map(item=><li key={item.id} className="border-b border-border pb-3 text-sm text-body">{item.name || item.title}</li>)}</ul>{section.items.length===0 && <p className="mt-4 text-sm leading-6 text-muted">{section.empty}</p>}</AsyncState><Link to={section.to} className="mt-5 inline-block text-sm font-semibold text-primary">Discover more ↗</Link></div></article>)}</div></section>

    <section className="mx-auto max-w-7xl px-5 pb-16 lg:px-8"><div className="grid overflow-hidden rounded-3xl bg-slate-800 md:grid-cols-[1.25fr_1fr]"><div className="p-8 sm:p-12"><p className="text-[10px] font-bold uppercase tracking-[.2em] text-orange-200">Research & preservation</p><h2 className="mt-4 text-3xl font-semibold text-white">Understanding our past.<br />Shaping what comes next.</h2><p className="mt-4 max-w-lg text-sm leading-7 text-slate-300">Explore heritage research, publications and the ideas helping craft traditions grow.</p><Link to={routePaths.research} className="mt-6 inline-block text-sm font-semibold text-orange-200">Visit the Innovation Hub ↗</Link></div><div className="min-h-64"><Photo image={images.crafts} alt="Editorial illustration of handmade craft materials" /></div></div></section>

    <section className="border-t border-border bg-[#eee6dc]"><div className="mx-auto grid max-w-7xl items-center gap-8 px-5 py-14 lg:grid-cols-[1fr_auto] lg:px-8"><div><p className="eyebrow">Be part of the story</p><h2 className="mt-3 text-3xl font-semibold">Your next chapter starts here.</h2><p className="mt-3 text-sm text-muted">Shop, create, learn or collaborate. Find your place in ShilpoHub.</p></div><div className="flex flex-wrap gap-3"><Link to={routePaths.register} className={buttonClass}>Join ShilpoHub ↗</Link><Link to={routePaths.marketplace} className="rounded-full border border-slate-300 px-6 py-3 text-sm font-semibold text-slate-800">Explore first</Link></div></div></section>
  </div>;
}
