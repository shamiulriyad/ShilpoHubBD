import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Button, SearchBar, SectionHeader, ChartPlaceholder } from '../../components/ui';
import { ProductCard, VillageCard, FestivalCard, CourseCard, StatCard, EntityCard } from '../../components/cards';
import {
  heritageStats,
  districts,
  villages,
  crafts,
  producers,
  products,
  festivals,
  courses,
  publications,
  timeline,
} from '../../data/mockData';

const exploreHighlights = [
  { title: 'Districts', subtitle: `${districts.length * 8}+ districts documented`, to: routePaths.exploreDistricts },
  { title: 'Heritage Villages', subtitle: 'Craft villages across the country', to: routePaths.exploreVillages },
  { title: 'Crafts', subtitle: `${crafts.length}+ traditional craft disciplines`, to: routePaths.exploreCrafts },
  { title: 'Festivals', subtitle: 'Seasonal & regional celebrations', to: routePaths.tourismFestivals },
  { title: 'Digital Museum', subtitle: 'Curated heritage collections', to: routePaths.exploreMuseum },
  { title: 'UNESCO Heritage', subtitle: 'Nationally recognized heritage', to: routePaths.exploreUnesco },
];

export default function HomePage() {
  return (
    <div>
      {/* 1. Hero */}
      <section className="border-b border-border bg-surface">
        <div className="mx-auto max-w-7xl px-4 py-16 text-center lg:px-8 lg:py-24">
          <p className="text-xs font-semibold uppercase tracking-wide text-primary">National Heritage Ecosystem</p>
          <h1 className="mx-auto mt-3 max-w-3xl text-3xl font-bold text-heading sm:text-4xl lg:text-5xl">
            Discover, Learn and Trade Bangladesh's Living Heritage
          </h1>
          <p className="mx-auto mt-4 max-w-2xl text-base text-body/70">
            ShilpoHub connects artisans, farmers, producers, customers, tourists, researchers and institutions
            around one shared heritage ecosystem.
          </p>
          <div className="mx-auto mt-8 max-w-xl">
            <SearchBar size="lg" placeholder="Search districts, crafts, products, festivals…" />
          </div>
          <div className="mt-6 flex flex-wrap items-center justify-center gap-3">
            <Link to={routePaths.explore}>
              <Button variant="primary">Explore Heritage</Button>
            </Link>
            <Link to={routePaths.marketplace}>
              <Button variant="secondary">Visit Marketplace</Button>
            </Link>
          </div>
        </div>
      </section>

      {/* 2. Heritage Statistics */}
      <section className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {heritageStats.map((stat) => (
            <StatCard key={stat.label} label={stat.label} value={stat.value} />
          ))}
        </div>
      </section>

      {/* 3. Explore Bangladesh Heritage */}
      <section className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
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
      <section className="bg-surface py-12">
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
          <div className="flex snap-x gap-4 overflow-x-auto pb-2">
            {products.slice(0, 6).map((product) => (
              <div key={product.id} className="w-56 shrink-0 snap-start">
                <ProductCard product={product} to={routePaths.marketplaceProducts} />
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 5. Featured Producers */}
      <section className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
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
              key={producer.id}
              title={producer.name}
              subtitle={producer.craft}
              meta={producer.district}
              to={routePaths.exploreProducers}
            />
          ))}
        </div>
      </section>

      {/* 6. Interactive Bangladesh Map */}
      <section className="bg-surface py-12">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <SectionHeader
            eyebrow="Heritage Map"
            title="Interactive Bangladesh Map"
            description="Select a district to explore its villages, crafts and producers."
          />
          <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
            <div className="flex aspect-[16/10] items-center justify-center rounded-2xl border border-dashed border-border bg-background text-sm text-body/40">
              Interactive Map Placeholder
            </div>
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-2 lg:grid-cols-1">
              {districts.map((district) => (
                <button
                  key={district.id}
                  type="button"
                  className="rounded-lg border border-border bg-background px-3 py-2 text-left text-sm text-body hover:border-primary hover:text-primary"
                >
                  {district.name}
                </button>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* 7. Heritage Villages */}
      <section className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
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
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
          {villages.map((village) => (
            <VillageCard key={village.id} village={village} to={routePaths.exploreVillages} />
          ))}
        </div>
      </section>

      {/* 8. Heritage Timeline */}
      <section className="bg-surface py-12">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <SectionHeader eyebrow="History" title="Heritage Timeline" description="Milestones in the national heritage movement." />
          <div className="grid gap-4 sm:grid-cols-5">
            {timeline.map((item) => (
              <div key={item.year} className="rounded-xl border border-border bg-background p-4">
                <p className="text-lg font-bold text-primary">{item.year}</p>
                <p className="mt-1 text-xs text-body/70">{item.label}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 9. Festivals & Events */}
      <section className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
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
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {festivals.map((festival) => (
            <FestivalCard key={festival.id} festival={festival} />
          ))}
        </div>
      </section>

      {/* 10. Heritage Academy */}
      <section className="bg-surface py-12">
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
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {courses.slice(0, 3).map((course) => (
              <CourseCard key={course.id} course={course} to={routePaths.academyCourseDetails.replace(':courseId', course.id)} />
            ))}
          </div>
        </div>
      </section>

      {/* 11. Innovation Hub */}
      <section className="mx-auto max-w-7xl px-4 py-12 lg:px-8">
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
            {publications.slice(0, 3).map((pub) => (
              <div key={pub.id} className="rounded-xl border border-border bg-surface p-4">
                <p className="text-sm font-semibold text-heading">{pub.title}</p>
                <p className="mt-1 text-xs text-body/60">
                  {pub.author} · {pub.year}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 12. Call To Action */}
      <section className="bg-title py-16 text-surface">
        <div className="mx-auto max-w-7xl px-4 text-center lg:px-8">
          <h2 className="text-2xl font-semibold sm:text-3xl">Join the ShilpoHub Ecosystem</h2>
          <p className="mx-auto mt-2 max-w-xl text-sm text-surface/80">
            Whichever role you play in heritage — there's a place for you here.
          </p>
          <div className="mt-8 grid gap-4 sm:grid-cols-3">
            {[
              { label: 'Join as Producer', desc: 'Sell your crafts to the nation and beyond' },
              { label: 'Join as Customer', desc: 'Discover and shop authentic heritage products' },
              { label: 'Join as Business Partner', desc: 'Partner with ShilpoHub on distribution & growth' },
            ].map((cta) => (
              <div key={cta.label} className="rounded-xl border border-surface/20 bg-surface/10 p-6 text-left">
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
