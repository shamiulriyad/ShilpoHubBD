import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Button, SearchBar, SectionHeader, ChartPlaceholder, AsyncState } from '../../components/ui';
import { useAuth } from '../../hooks/useAuth';
import { ProductCard, VillageCard, FestivalCard, CourseCard, StatCard, EntityCard } from '../../components/cards';
import { useDistricts } from '../../hooks/useDistricts';
import { useVillages } from '../../hooks/useVillages';
import { useFeaturedProducts } from '../../hooks/useProducts';
import { useHeritageFestivals } from '../../hooks/useHeritageFestivals';
import { useCourses } from '../../hooks/useCourses';
import { useResearchPublications } from '../../hooks/useResearchPublications';
import { toProductCardItem } from '../../utils/productAdapters';
import { toVillageCardItem } from '../../utils/villageAdapters';
import BangladeshMap from '../../components/media/BangladeshMap';

const listOf = (data) => data?.items || data || [];

// TODO(backend): no platform-stats or heritage-timeline endpoint — editorial content.
const heritageStats = [
  { label: 'Registered Producers', value: '12,400+' },
  { label: 'Heritage Villages', value: '640+' },
  { label: 'Heritage Products', value: '8,900+' },
  { label: 'Districts Covered', value: '64' },
];

const timeline = [
  { year: '1971', label: 'Independence & the revival of national craft identity' },
  { year: '1985', label: 'First national craft cooperatives established' },
  { year: '2013', label: 'Jamdani recognized by UNESCO' },
  { year: '2020', label: 'Digital heritage documentation begins' },
  { year: '2026', label: 'ShilpoHub national ecosystem launches' },
];

const exploreHighlights = [
  { title: 'Districts', subtitle: 'Heritage documented by district', to: routePaths.exploreDistricts },
  { title: 'Heritage Villages', subtitle: 'Craft villages across the country', to: routePaths.exploreVillages },
  { title: 'Crafts', subtitle: 'Traditional craft disciplines', to: routePaths.exploreCrafts },
  { title: 'Festivals', subtitle: 'Seasonal & regional celebrations', to: routePaths.tourismFestivals },
  { title: 'Digital Museum', subtitle: 'Curated heritage collections', to: routePaths.exploreMuseum },
  { title: 'UNESCO Heritage', subtitle: 'Nationally recognized heritage', to: routePaths.exploreUnesco },
];

const courseToCardItem = (c) => ({
  level: c.status || 'Course',
  title: c.title,
  mentor: c.authorName,
  duration: `${c.lessonCount ?? 0} lessons`,
  enrolled: c.activeEnrollmentCount ?? 0,
});

export default function HomePage() {
  const [selectedDistrict, setSelectedDistrict] = useState(null);
  const districtsQuery = useDistricts();
  const villagesQuery = useVillages();
  const { isAuthenticated } = useAuth();
  const productsQuery = useFeaturedProducts(6);
  const festivalsQuery = useHeritageFestivals({ pageSize: 6 });
  const coursesQuery = useCourses({ pageSize: 3 });
  // The publications repository requires auth — skip the call for anonymous visitors.
  const publicationsQuery = useResearchPublications({ pageSize: 3 }, isAuthenticated);

  const districts = listOf(districtsQuery.data);
  const villages = listOf(villagesQuery.data);
  const products = productsQuery.data || [];
  const festivals = listOf(festivalsQuery.data);
  const courses = listOf(coursesQuery.data);
  const publications = listOf(publicationsQuery.data);

  const producers = [
    ...new Map(
      products
        .filter((p) => p.producerName)
        .map((p) => [p.producerName, { name: p.producerName, craft: p.categoryName, district: p.districtName }]),
    ).values(),
  ];

  return (
    <div className="premium-shell overflow-hidden">
      {/* 1. Hero */}
      <section className="relative isolate overflow-hidden border-b border-border bg-title">
        <div className="absolute inset-0 opacity-70 [background-image:radial-gradient(circle_at_14%_16%,rgba(217,155,61,.35),transparent_22rem),radial-gradient(circle_at_86%_84%,rgba(168,79,45,.45),transparent_25rem)]" />
        <div className="absolute -right-32 top-8 h-80 w-80 rounded-full border border-surface/15" />
        <div className="absolute -right-16 top-24 h-56 w-56 rounded-full border border-surface/10" />
        <div className="relative mx-auto max-w-7xl px-4 py-20 text-center lg:px-8 lg:py-28">
          <p className="inline-flex rounded-full border border-surface/20 bg-surface/10 px-4 py-2 text-[11px] font-bold uppercase tracking-[0.18em] text-[#F6D5AA]">Bangladesh's National Heritage Ecosystem</p>
          <h1 className="mx-auto mt-5 max-w-4xl text-4xl font-bold tracking-[-0.055em] text-surface sm:text-5xl lg:text-7xl">
            Heritage, made <span className="text-[#F3C79D]">living.</span>
          </h1>
          <p className="mx-auto mt-5 max-w-2xl text-base leading-7 text-surface/75 sm:text-lg">
            Discover authentic Bangladeshi craft, meet the people behind it, and help safeguard the traditions that shape us.
          </p>
          <div className="mx-auto mt-8 max-w-xl">
            <SearchBar size="lg" placeholder="Search districts, crafts, products, festivals…" />
          </div>
          <div className="mt-7 flex flex-wrap items-center justify-center gap-3">
            <Link to={routePaths.explore}>
              <Button variant="primary">Explore Heritage</Button>
            </Link>
            <Link to={routePaths.marketplace}>
              <Button variant="secondary" className="border-surface/25 bg-surface/10 text-surface hover:bg-surface hover:text-title">Visit Marketplace</Button>
            </Link>
          </div>
          <div className="mx-auto mt-12 grid max-w-2xl grid-cols-3 divide-x divide-surface/15 rounded-2xl border border-surface/15 bg-surface/[.06] px-3 py-4 text-left backdrop-blur-sm">
            <div className="px-4"><p className="text-xl font-bold text-[#F3C79D]">64</p><p className="mt-1 text-[10px] font-semibold uppercase tracking-wider text-surface/60">Districts</p></div>
            <div className="px-4"><p className="text-xl font-bold text-[#F3C79D]">640+</p><p className="mt-1 text-[10px] font-semibold uppercase tracking-wider text-surface/60">Villages</p></div>
            <div className="px-4"><p className="text-xl font-bold text-[#F3C79D]">12.4K</p><p className="mt-1 text-[10px] font-semibold uppercase tracking-wider text-surface/60">Producers</p></div>
          </div>
        </div>
      </section>

      {/* 2. Heritage Statistics */}
      <section className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {heritageStats.map((stat) => (
            <StatCard key={stat.label} label={stat.label} value={stat.value} />
          ))}
        </div>
      </section>

      {/* 3. Explore Bangladesh Heritage */}
      <section className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <SectionHeader
          eyebrow="Explore"
          title="Explore Bangladesh Heritage"
          description="Browse heritage by district, village, craft, festival and collection."
          action={
            <Link to={routePaths.explore} className="text-sm font-medium text-link hover:underline">
              View all →
            </Link>
          }
        />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
          {exploreHighlights.map((item) => (
            <EntityCard key={item.title} title={item.title} subtitle={item.subtitle} to={item.to} />
          ))}
        </div>
      </section>

      {/* 4. Featured Heritage Products */}
      <section className="border-y border-border/70 bg-surface py-16">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <SectionHeader
            eyebrow="Marketplace"
            title="Featured Heritage Products"
            description="Authentic products sourced directly from verified producers."
            action={
              <Link to={routePaths.marketplaceProducts} className="text-sm font-medium text-link hover:underline">
                View all →
              </Link>
            }
          />
          <AsyncState isLoading={productsQuery.isLoading} isError={productsQuery.isError} error={productsQuery.error}>
            <div className="flex snap-x gap-4 overflow-x-auto pb-2">
              {products.map((product) => (
                <div key={product.id} className="w-56 shrink-0 snap-start">
                  <ProductCard
                    product={toProductCardItem(product)}
                    to={routePaths.marketplaceProductDetails.replace(':productId', product.id)}
                  />
                </div>
              ))}
            </div>
          </AsyncState>
        </div>
      </section>

      {/* 5. Featured Producers */}
      <section className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <SectionHeader
          eyebrow="Community"
          title="Featured Producers"
          description="Meet the artisans and producers behind the collections."
          action={
            <Link to={routePaths.exploreProducers} className="text-sm font-medium text-link hover:underline">
              View all →
            </Link>
          }
        />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
          {producers.map((producer) => (
            <EntityCard
              key={producer.name}
              title={producer.name}
              subtitle={producer.craft}
              meta={producer.district}
              to={routePaths.exploreProducers}
            />
          ))}
        </div>
      </section>

      {/* 6. Interactive Bangladesh Map */}
      <section className="border-y border-border/70 bg-surface py-16">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <SectionHeader
            eyebrow="Heritage Map"
            title="Interactive Bangladesh Map"
            description="Select a district to explore its villages, crafts and producers."
          />
          <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
            <BangladeshMap selectedDistrict={selectedDistrict?.name} />
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-2 lg:grid-cols-1">
              {districts.map((district) => (
                <button
                  key={district.id}
                  type="button"
                  onClick={() => setSelectedDistrict(district)}
                  className={`rounded-lg border px-3 py-2 text-left text-sm transition ${selectedDistrict?.id === district.id ? 'border-primary bg-primary-soft font-semibold text-primary' : 'border-border bg-background text-body hover:border-primary hover:text-primary'}`}
                >
                  {district.name}
                </button>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* 7. Heritage Villages */}
      <section className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <SectionHeader
          eyebrow="Explore"
          title="Heritage Villages"
          description="Villages recognized for keeping traditional crafts alive."
          action={
            <Link to={routePaths.exploreVillages} className="text-sm font-medium text-link hover:underline">
              View all →
            </Link>
          }
        />
        <AsyncState isLoading={villagesQuery.isLoading} isError={villagesQuery.isError} error={villagesQuery.error}>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
            {villages.slice(0, 6).map((village) => (
              <VillageCard
                key={village.id}
                village={toVillageCardItem(village)}
                to={routePaths.exploreVillageDetails.replace(':villageId', village.id)}
              />
            ))}
          </div>
        </AsyncState>
      </section>

      {/* 8. Heritage Timeline */}
      <section className="border-y border-border/70 bg-surface py-16">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <SectionHeader eyebrow="History" title="Heritage Timeline" description="Milestones in the national heritage movement." />
          <div className="grid gap-4 sm:grid-cols-5">
            {timeline.map((item) => (
              <div key={item.year} className="relative rounded-2xl border border-border bg-background p-5 transition hover:-translate-y-1 hover:border-primary/30">
                <p className="text-lg font-bold text-primary">{item.year}</p>
                <p className="mt-1 text-xs text-body/70">{item.label}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 9. Festivals & Events */}
      <section className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <SectionHeader
          eyebrow="Tourism"
          title="Festivals & Events"
          description="Upcoming cultural festivals and heritage events."
          action={
            <Link to={routePaths.tourismFestivals} className="text-sm font-medium text-link hover:underline">
              View all →
            </Link>
          }
        />
        <AsyncState isLoading={festivalsQuery.isLoading} isError={festivalsQuery.isError} error={festivalsQuery.error}>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {festivals.map((festival) => (
              <FestivalCard
                key={festival.id}
                festival={{ name: festival.name, date: festival.startDate, district: festival.districtName }}
              />
            ))}
          </div>
        </AsyncState>
      </section>

      {/* 10. Heritage Academy */}
      <section className="border-y border-border/70 bg-surface py-16">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <SectionHeader
            eyebrow="Academy"
            title="Heritage Academy"
            description="Learn traditional crafts from certified master artisans."
            action={
              <Link to={routePaths.academy} className="text-sm font-medium text-link hover:underline">
                Browse courses →
              </Link>
            }
          />
          <AsyncState isLoading={coursesQuery.isLoading} isError={coursesQuery.isError} error={coursesQuery.error}>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {courses.map((course) => (
                <CourseCard
                  key={course.id}
                  course={courseToCardItem(course)}
                  to={routePaths.academyCourseDetails.replace(':courseId', course.id)}
                />
              ))}
            </div>
          </AsyncState>
        </div>
      </section>

      {/* 11. Innovation Hub */}
      <section className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <SectionHeader
          eyebrow="Innovation Hub"
          title="Innovation Hub"
          description="Research, publications and open heritage analytics."
          action={
            <Link to={routePaths.research} className="text-sm font-medium text-link hover:underline">
              Visit Innovation Hub →
            </Link>
          }
        />
        <div className="grid gap-4 lg:grid-cols-[1fr_1fr]">
          <ChartPlaceholder title="Heritage Analytics Preview" type="line" />
          <div className="space-y-3">
            {publications.map((pub) => (
              <div key={pub.id} className="rounded-xl border border-border bg-surface p-4">
                <p className="text-sm font-semibold text-heading">{pub.title}</p>
                <p className="mt-1 text-xs text-body/60">
                  {pub.authors}
                  {pub.publishedOn ? ` · ${new Date(pub.publishedOn).getFullYear()}` : ''}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 12. Call To Action */}
      <section className="relative overflow-hidden bg-title py-20 text-surface">
        <div className="mx-auto max-w-7xl px-4 text-center lg:px-8">
          <p className="text-[11px] font-bold uppercase tracking-[0.18em] text-[#F3C79D]">One living ecosystem</p>
          <h2 className="mt-3 text-3xl font-bold tracking-[-0.04em] sm:text-4xl">Join the ShilpoHub Ecosystem</h2>
          <p className="mx-auto mt-2 max-w-xl text-sm text-surface/80">
            Whichever role you play in heritage — there's a place for you here.
          </p>
          <div className="mt-8 grid gap-4 sm:grid-cols-3">
            {[
              { label: 'Join as Producer', desc: 'Sell your crafts to the nation and beyond' },
              { label: 'Join as Customer', desc: 'Discover and shop authentic heritage products' },
              { label: 'Join as Business Partner', desc: 'Partner with ShilpoHub on distribution & growth' },
            ].map((cta) => (
              <div key={cta.label} className="rounded-2xl border border-surface/15 bg-surface/[.08] p-6 text-left backdrop-blur-sm transition hover:-translate-y-1 hover:bg-surface/[.13]">
                <p className="text-sm font-semibold">{cta.label}</p>
                <p className="mt-1 text-xs text-surface/70">{cta.desc}</p>
                <Link to={routePaths.register} className="mt-4 inline-block text-xs font-medium underline">
                  Get started →
                </Link>
              </div>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}