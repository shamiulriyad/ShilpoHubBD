import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, SectionHeader, AsyncState } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { TimelineViewer } from '../../components/media';
import { useProducerStory } from '../../hooks/useProducerStories';
import { useProducts } from '../../hooks/useProducts';
import { toProductCardItem } from '../../utils/productAdapters';

export default function ProducerStory() {
  const { producerId } = useParams();
  const storyQuery = useProducerStory(producerId);
  const productsQuery = useProducts({ producerId, pageSize: 8 });
  const story = storyQuery.data;
  const producerProducts = (productsQuery.data?.items || []).slice(0, 4);

  return (
    <div>
      <AsyncState isLoading={storyQuery.isLoading} isError={storyQuery.isError} error={storyQuery.error}>
        {story && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Dashboard', path: routePaths.customer },
                { label: 'Marketplace', path: routePaths.customerMarketplace },
                { label: story.producerName, path: routePaths.customerProducerProfile.replace(':producerId', producerId) },
                { label: 'Story' },
              ]}
              title={`${story.producerName}'s Story`}
            />

            <div className="mb-10 flex flex-wrap items-center gap-3">
              <Badge tone="primary">Digital Heritage ID {story.heritageId}</Badge>
              <Badge tone="secondary">{story.generations} generations of practice</Badge>
              {story.foundingYear && <Badge tone="secondary">Since {story.foundingYear}</Badge>}
            </div>

            {story.quote && (
              <blockquote className="mb-10 max-w-3xl border-l-4 border-primary/40 pl-4 text-lg italic text-heading">
                “{story.quote}”
              </blockquote>
            )}

            <div className="mb-12">
              <TimelineViewer
                items={story.chapters.map((chapter) => ({ title: chapter.heading, description: chapter.body }))}
              />
              {story.chapters.length === 0 && <p className="text-sm text-body/60">This producer hasn't added their story yet.</p>}
            </div>

            {producerProducts.length > 0 && (
              <>
                <SectionHeader
                  eyebrow="Shop"
                  title={`Products by ${story.producerName}`}
                  action={
                    <Link
                      to={routePaths.customerProducerProfile.replace(':producerId', producerId)}
                      className="text-sm font-medium text-link hover:underline"
                    >
                      View full profile →
                    </Link>
                  }
                />
                <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
                  {producerProducts.map((product) => (
                    <ProductCard
                      key={product.id}
                      product={toProductCardItem(product)}
                      to={routePaths.customerProductDetails.replace(':productId', product.id)}
                    />
                  ))}
                </div>
              </>
            )}
          </>
        )}
      </AsyncState>
    </div>
  );
}
