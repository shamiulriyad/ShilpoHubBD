import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, SectionHeader, Badge, AsyncState } from '../../components/ui';
import { ProducerCard, ProductCard } from '../../components/cards';
import { TimelineViewer } from '../../components/media';
import { useCraftStory } from '../../hooks/useCraftStories';
import { useProducts } from '../../hooks/useProducts';
import { toProductCardItem } from '../../utils/productAdapters';

export default function CraftStory() {
  // Route param is named craftId for historical reasons, but CraftStoriesController
  // is actually keyed by category id.
  const { craftId: categoryId } = useParams();
  const storyQuery = useCraftStory(categoryId);
  const productsQuery = useProducts({ categoryId, pageSize: 50 });
  const story = storyQuery.data;
  const relatedProducts = productsQuery.data?.items || [];

  const practitioners = [
    ...new Map(relatedProducts.map((p) => [p.producerId, { id: p.producerId, name: p.producerName, craft: p.categoryName, district: p.districtName }])).values(),
  ];

  return (
    <div>
      <AsyncState isLoading={storyQuery.isLoading} isError={storyQuery.isError} error={storyQuery.error}>
        {story ? (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Dashboard', path: routePaths.customer },
                { label: 'Marketplace', path: routePaths.customerMarketplace },
                { label: `${story.categoryName} Story` },
              ]}
              title={`The Story of ${story.categoryName}`}
              description={story.summary}
            />

            <div className="mb-8 flex flex-wrap gap-3">
              <Badge tone="primary">Origin: {story.origin}</Badge>
              <Badge tone="secondary">Since {story.since}</Badge>
            </div>

            <div className="mb-12">
              <TimelineViewer
                items={story.chapters.map((chapter) => ({ title: chapter.heading, description: chapter.body }))}
              />
            </div>
          </>
        ) : (
          !storyQuery.isLoading && (
            <PageHeader
              breadcrumbs={[
                { label: 'Dashboard', path: routePaths.customer },
                { label: 'Marketplace', path: routePaths.customerMarketplace },
                { label: 'Craft Story' },
              ]}
              title="Craft story unavailable"
              description="This category doesn't have a published craft story yet."
            />
          )
        )}

        {practitioners.length > 0 && (
          <>
            <SectionHeader eyebrow="Meet the Makers" title="Producers of this Craft" />
            <div className="mb-12 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
              {practitioners.map((producer) => (
                <ProducerCard
                  key={producer.id}
                  producer={producer}
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
              title="Products in this Craft"
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
                  product={toProductCardItem(product)}
                  to={routePaths.customerProductDetails.replace(':productId', product.id)}
                />
              ))}
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
