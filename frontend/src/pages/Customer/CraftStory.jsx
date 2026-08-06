import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, Badge } from '../../components/ui';
import { EntityCard, ProductCard } from '../../components/cards';
import { craftStories, producers, products } from '../../data/mockData';

export default function CraftStory() {
  const { craftId } = useParams();
  const story = craftStories.find((s) => s.id === craftId) || craftStories[0];
  const practitioners = producers.filter((p) => p.craft === story.title);
  const relatedProducts = products.filter((p) => p.category === story.title);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: `${story.title} Story` },
        ]}
        title={`The Story of ${story.title}`}
        description={story.summary}
      />

      <div className="mb-8 flex flex-wrap gap-3">
        <Badge tone="primary">Origin: {story.origin}</Badge>
        <Badge tone="secondary">Since {story.since}</Badge>
      </div>

      <div className="mb-12 space-y-8">
        {story.chapters.map((chapter) => (
          <div key={chapter.heading} className="max-w-3xl">
            <h2 className="mb-2 text-lg font-semibold text-heading">{chapter.heading}</h2>
            <p className="text-sm leading-relaxed text-body/80">{chapter.body}</p>
          </div>
        ))}
      </div>

      {practitioners.length > 0 && (
        <>
          <SectionHeader eyebrow="Meet the Makers" title={`Producers of ${story.title}`} />
          <div className="mb-12 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
            {practitioners.map((producer) => (
              <EntityCard
                key={producer.id}
                title={producer.name}
                subtitle={producer.craft}
                meta={producer.district}
                to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
              />
            ))}
          </div>
        </>
      )}

      {relatedProducts.length > 0 && (
        <>
          <SectionHeader
            eyebrow="Shop"
            title={`${story.title} Products`}
            action={
              <Link to={routePaths.customerMarketplace} className="text-sm font-medium text-link hover:underline">
                View marketplace →
              </Link>
            }
          />
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
            {relatedProducts.map((product) => (
              <ProductCard
                key={product.id}
                product={product}
                to={routePaths.customerProductDetails.replace(':productId', product.id)}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}
