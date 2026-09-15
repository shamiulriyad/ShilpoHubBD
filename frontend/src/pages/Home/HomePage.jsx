import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { routePaths as routes } from '../../routes/routePaths';
import { SearchBar, SectionHeader, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import SafeImage from '../../components/media/SafeImage';
import BangladeshMap from '../../components/media/BangladeshMap';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { useFeaturedProducts, useProducts } from '../../hooks/useProducts';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useCourses } from '../../hooks/useCourses';
import { toProductCardItem } from '../../utils/productAdapters';

const list = data => Array.isArray(data) ? data : Array.isArray(data?.items) ? data.items : [];
const photos = { loom: '/images/loom-photo.jpg', river: '/images/bangladesh-river.jpg', pottery: '/images/pottery-photo.jpg' };
const shell = 'mx-auto max-w-7xl px-5 lg:px-8';
const cta = 'inline-flex items-center justify-center rounded-full bg-primary px-6 py-3 text-sm font-semibold text-white transition hover:bg-primary-dark';
function Photo({ src, alt, className = '', eager = false }) {
  return <SafeImage src={src} alt={alt} loading={eager ? 'eager' : 'lazy'} fetchPriority={eager ? 'high' : 'auto'} className={`h-full w-full object-cover ${className}`} />;
}
function Collection({ eyebrow, title, description, image, alt, to, query, titleOf, toItem }) {
  const items = list(query.data).slice(0, 3);
  return <article className="overflow-hidden rounded-2xl border border-border bg-surface">
    <Link to={to} className="block h-56 overflow-hidden"><Photo src={image} alt={alt} /></Link>
    <div className="p-6"><p className="text-xs font-bold uppercase tracking-[.18em] text-primary">{eyebrow}</p><h3 className="mt-3 text-2xl tracking-tight">{title}</h3><p className="mt-3 text-sm leading-6 text-muted">{description}</p>
      <AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}><ul className="mt-5 divide-y divide-border">{items.map(item => <li key={item.id}><Link className="block py-3 text-sm font-medium text-heading hover:text-primary" to={toItem(item)}>{titleOf(item)} <span aria-hidden="true">↗</span></Link></li>)}</ul>{!items.length && <p className="mt-4 text-sm text-muted">New listings will appear here when published.</p>}</AsyncState>
      <Link to={to} className="mt-5 inline-block text-sm font-semibold text-primary">Explore {eyebrow.toLowerCase()} →</Link>
    </div>
  </article>;
}
export default function HomePage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [districtId, setDistrictId] = useState('');
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const featuredQuery = useFeaturedProducts(6);
  const catalogQuery = useProducts({ pageSize: 6 });
  const festivalsQuery = useHeritageFestivals({ pageSize: 3 });
  const coursesQuery = useCourses({ pageSize: 3 });
  const districts = list(districtsQuery.data);
  const district = districts.find(item => item.id === districtId);
  const productsQuery = list(featuredQuery.data).length ? featuredQuery : catalogQuery;
  const products = list(productsQuery.data).slice(0, 6);
  return <div className="overflow-hidden">
    <section className="border-b border-border bg-background">
      <div className={`${shell} grid items-center gap-10 py-12 lg:grid-cols-2 lg:gap-16 lg:py-20`}>
        <div><p className="text-xs font-bold uppercase tracking-[.24em] text-primary">Craft · Culture · Community</p><h1 className="mt-6 text-5xl leading-[1.08] tracking-[-.04em] sm:text-6xl lg:text-7xl" style={{fontFamily:'Georgia, serif'}}>Made by hand.<br/><span className="text-primary">Carried by stories.</span></h1><p className="mt-6 max-w-lg text-base leading-8 text-muted">Discover the craft, places and people of Bangladesh. Find something meaningful, learn a timeless skill, and become part of a living tradition.</p>
          <div className="mt-8"><SearchBar placeholder="Find a craft, a product, a story…" value={search} onChange={event=>setSearch(event.target.value)} onSubmit={value=>navigate(`${routes.marketplaceProducts}${value?.trim() ? `?search=${encodeURIComponent(value.trim())}` : ''}`)} /></div>
          <div className="mt-6 flex flex-wrap items-center gap-5"><Link className={cta} to={routes.marketplaceProducts}>Shop the collection ↗</Link><Link className="text-sm font-semibold text-heading" to={routes.explore}>Explore Bangladesh →</Link></div>
          <p className="mt-8 border-t border-border pt-5 text-xs leading-6 text-muted">Thoughtfully made. Deeply rooted. Ready to be discovered.</p>
        </div>
        <figure className="relative"><div className="h-[380px] overflow-hidden rounded-[2rem] sm:h-[520px]"><Photo src={photos.pottery} alt="A potter shaping wet clay by hand on a pottery wheel" eager /></div><figcaption className="absolute bottom-5 left-5 right-5 rounded-xl bg-surface/95 px-5 py-4 shadow-lg"><p className="text-[10px] font-bold uppercase tracking-[.2em] text-primary">The art of making</p><p className="mt-1 text-lg font-medium text-heading">Every piece begins with a human touch.</p></figcaption></figure>
      </div>
    </section>
    <div className={`${shell} grid grid-cols-3 divide-x divide-border border-b border-border py-7 text-center`}>
      {[['Districts to explore',districtsQuery,districts.length],['Heritage villages',villagesQuery,list(villagesQuery.data).length],['Marketplace finds',catalogQuery,catalogQuery.data?.totalCount ?? list(catalogQuery.data).length]].map(([label,query,value])=><div key={label} className="px-2"><p className="text-2xl font-semibold text-heading">{query.isLoading || query.isError ? '—' : value.toLocaleString()}</p><p className="mt-1 text-xs text-muted">{label}</p></div>)}
    </div>
    <section className={`${shell} py-16 lg:py-20`}>
      <SectionHeader eyebrow="Start your discovery" title="A world of heritage, closer to you." description="Follow your curiosity through craft, culture and the places that connect them." />
      <div className="grid gap-5 md:grid-cols-3">{[
        ['01 / Places','Beyond the familiar',photos.river,'Riverside homes in rural Bangladesh',routes.exploreVillages,'Explore villages, waterways and local traditions.'],
        ['02 / Craft','Objects with a story',photos.pottery,'Potter working with clay',routes.exploreCrafts,'Discover the techniques behind handmade objects.'],
        ['03 / Learning','Keep the skill alive',photos.loom,'Hands weaving fabric on a wooden loom',routes.academy,'Make room for creativity and learn something lasting.']
      ].map(([label,title,src,alt,to,description])=><Link to={to} key={title} className="group overflow-hidden rounded-2xl border border-border bg-surface"><div className="h-64 overflow-hidden"><Photo src={src} alt={alt} className="transition duration-500 group-hover:scale-105" /></div><div className="p-6"><p className="text-xs font-semibold uppercase tracking-widest text-primary">{label}</p><h3 className="mt-3 text-2xl">{title} ↗</h3><p className="mt-2 text-sm leading-6 text-muted">{description}</p></div></Link>)}</div>
      <div className="mt-6 flex flex-wrap gap-3">{[['District directory',routes.exploreDistricts],['Digital museum',routes.exploreMuseum],['UNESCO heritage',routes.exploreUnesco]].map(([label,to])=><Link key={label} to={to} className="rounded-full border border-border px-5 py-2 text-sm text-heading hover:border-primary">{label} →</Link>)}</div>
    </section>
    <section className="border-y border-border bg-surface py-16"><div className={shell}><SectionHeader eyebrow="The marketplace" title="Find your next treasured piece." description="Browse products from the ShilpoHub community." action={<Link to={routes.marketplaceProducts} className="text-sm font-semibold text-primary">Shop all products →</Link>} /><AsyncState isLoading={productsQuery.isLoading} isError={productsQuery.isError} error={productsQuery.error}><div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">{products.map(product=><ProductCard key={product.id} product={toProductCardItem(product)} to={routes.marketplaceProductDetails.replace(':productId',product.id)} />)}</div>{!products.length && <p className="rounded-xl bg-background p-6 text-muted">The collection is being prepared. Check back for new handmade finds.</p>}</AsyncState></div></section>
    <section className={`${shell} grid items-center gap-10 py-16 lg:grid-cols-2 lg:py-24`}><figure className="h-[380px] overflow-hidden rounded-2xl"><Photo src={photos.loom} alt="A close-up of hands carefully working threads on a loom" /></figure><div><p className="text-xs font-bold uppercase tracking-[.2em] text-primary">People behind the craft</p><h2 className="mt-4 text-4xl leading-tight tracking-tight" style={{fontFamily:'Georgia, serif'}}>Meet the makers.<br/>Understand the making.</h2><p className="mt-5 text-base leading-8 text-muted">Behind every woven thread and carefully shaped vessel is a skill developed over time. Explore producer profiles, their collections, and the communities they call home.</p><Link className={`${cta} mt-7`} to={routes.exploreProducers}>Discover our producers →</Link></div></section>
    <section className="border-y border-border bg-surface py-16"><div className={`${shell} grid items-center gap-10 lg:grid-cols-[1fr_1.4fr]`}><div><p className="text-xs font-bold uppercase tracking-[.2em] text-primary">A sense of place</p><h2 className="mt-4 text-4xl tracking-tight">Your journey starts here.</h2><p className="mt-4 text-sm leading-7 text-muted">Browse the district directory to find local heritage and plan your next discovery.</p><AsyncState isLoading={districtsQuery.isLoading} isError={districtsQuery.isError} error={districtsQuery.error}><label className="mt-6 block text-sm font-medium">Choose a district<select value={districtId} onChange={event=>setDistrictId(event.target.value)} className="mt-2 w-full rounded-xl border border-border bg-background p-3"><option value="">All districts</option>{districts.map(item=><option key={item.id} value={item.id}>{item.name}</option>)}</select></label></AsyncState><Link className="mt-5 inline-block text-sm font-semibold text-primary" to={district ? routes.exploreDistrictDetails.replace(':districtId',district.id) : routes.exploreDistricts}>{district ? `Explore ${district.name}` : 'Browse all districts'} →</Link></div><div className="overflow-hidden rounded-2xl border border-border"><BangladeshMap selectedDistrict={district?.name} /></div></div></section>
    <section className={`${shell} py-16 lg:py-20`}><SectionHeader eyebrow="Experience more" title="Places to go. Skills to grow." description="Make heritage a part of your everyday life." /><div className="grid items-start gap-6 lg:grid-cols-3">
      <Collection eyebrow="Villages" title="Go beyond the city" description="Discover communities where tradition is part of daily life." image={photos.river} alt="Riverside village in Bangladesh" to={routes.exploreVillages} query={villagesQuery} titleOf={item=>item.name} toItem={item=>routes.exploreVillageDetails.replace(':villageId',item.id)} />
      <Collection eyebrow="Festivals" title="Come together" description="Find cultural celebrations and plan your next visit." image={photos.pottery} alt="Traditional pottery craftsmanship" to={routes.tourismFestivals} query={festivalsQuery} titleOf={item=>item.name} toItem={()=>routes.tourismFestivals} />
      <Collection eyebrow="Academy" title="Learn by making" description="Explore courses and build your creative practice." image={photos.loom} alt="Hands demonstrating weaving techniques" to={routes.academy} query={coursesQuery} titleOf={item=>item.title} toItem={item=>routes.academyCourseDetails.replace(':courseId',item.id)} />
    </div></section>
    <section className={`${shell} pb-16`}><div className="grid overflow-hidden rounded-2xl bg-[#292d35] md:grid-cols-2"><div className="p-8 lg:p-12"><p className="text-xs font-bold uppercase tracking-[.2em] text-[#e8bd98]">Heritage, looking forward</p><h2 className="mt-4 text-3xl leading-tight text-white">Old knowledge.<br/>New possibilities.</h2><p className="mt-4 text-sm leading-7 text-white/75">Explore research, ideas and collaborations that help preserve heritage for the next generation.</p><Link className="mt-6 inline-block text-sm font-semibold text-white" to={routes.research}>Visit the Innovation Hub →</Link></div><div className="min-h-64"><Photo src={photos.river} alt="Traditional riverside architecture in Bangladesh" /></div></div></section>
    <section className="border-t border-border bg-primary-soft py-14"><div className={`${shell} flex flex-wrap items-center justify-between gap-6`}><div><p className="text-xs font-bold uppercase tracking-widest text-primary">Be part of the story</p><h2 className="mt-3 text-3xl">A place for makers, explorers and you.</h2><p className="mt-3 text-sm text-muted">Join the community and discover what you can create together.</p></div><Link className={cta} to={routes.register}>Join ShilpoHub →</Link></div></section>
    <p className={`${shell} py-4 text-[11px] text-muted`}>Editorial photography: <a href="https://www.pexels.com/photo/a-woman-using-a-loom-6634701/">Kaboompics / Pexels</a> · <a href="https://unsplash.com/photos/a-group-of-huts-sitting-on-top-of-a-lake-x6SoJq2xyEs">Jaman Asad / Unsplash</a> · <a href="https://pxhere.com/en/photo/989432">PxHere</a>. Craft photographs illustrate techniques and do not identify listed producers or products.</p>
  </div>;
}
