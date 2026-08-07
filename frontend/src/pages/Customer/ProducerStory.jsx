import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, SectionHeader } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { TimelineViewer } from '../../components/media';
import { producers, producerStories, products } from '../../data/mockData';

export default function ProducerStory() {
  const { producerId } = useParams();
  const producer = producers.find((p) => p.id === producerId) || producers[0];
  const story = producerStories.find((s) => s.id === producer.id) || producerStories[0];
  const producerProducts = products.filter((p) => p.producer === producer.name).slice(0, 4);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: producer.name, path: routePaths.customerProducerProfile.replace(':producerId', producer.id) },
          { label: 'Story' },
        ]}
        title={`${producer.name}'s Story`}
        description={`${producer.craft} · ${producer.district}`}
      />

      <div className="mb-10 flex flex-wrap items-center gap-3">
        <Badge tone="primary">Digital Heritage ID {story.heritageId}</Badge>
        <Badge tone="secondary">{story.generations} generations of practice</Badge>
      </div>

      <blockquote className="mb-10 max-w-3xl border-l-4 border-primary/40 pl-4 text-lg italic text-heading">
        “{story.quote}”
      </blockquote>

      <div className="mb-12">
        <TimelineViewer
          items={story.chapters.map((chapter) => ({ title: chapter.heading, description: chapter.body }))}
        />
      </div>

      {producerProducts.length > 0 && (
        <>
          <SectionHeader
            eyebrow="Shop"
            title={`Products by ${producer.name}`}
            action={
              <Link
                to={routePaths.customerProducerProfile.replace(':producerId', producer.id)}
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
