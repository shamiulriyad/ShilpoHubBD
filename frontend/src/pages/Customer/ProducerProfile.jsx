import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, SectionHeader, Badge } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { producers, producerStories, products, workshops } from '../../data/mockData';

export default function ProducerProfile() {
  const { producerId } = useParams();
  const producer = producers.find((p) => p.id === producerId) || producers[0];
  const story = producerStories.find((s) => s.id === producer.id);
  const producerProducts = products.filter((p) => p.producer === producer.name);
  const producerWorkshops = workshops.filter((w) => w.producerId === producer.id);

  return (
    <div>
      <PageHeader
        breadcrumbs={[
          { label: 'Dashboard', path: routePaths.customer },
          { label: 'Marketplace', path: routePaths.customerMarketplace },
          { label: producer.name },
        ]}
        title={producer.name}
        description={`${producer.craft} · ${producer.district}`}
        action={
          <div className="flex gap-3">
            <Button variant="secondary">Contact Producer</Button>
            <Link to={routePaths.customerProducerStory.replace(':producerId', producer.id)}>
              <Button variant="primary">Read Full Story</Button>
            </Link>
          </div>
        }
      />

      {story && (
        <div className="mb-8 flex items-center gap-3 rounded-xl border border-border bg-surface p-4">
          <Badge tone="primary">Heritage ID {story.heritageId}</Badge>
          <span className="text-sm text-body/70">{story.generations} generations of practice</span>
        </div>
      )}

      <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <StatCard label="Rating" value={`★ ${producer.rating}`} />
        <StatCard label="Products Listed" value={producerProducts.length || '24'} />
        <StatCard label="Years Active" value="12" />
      </div>

      <SectionHeader eyebrow="Shop" title={`Products by ${producer.name}`} />
      <div className="mb-12 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {(producerProducts.length ? producerProducts : products.slice(0, 4)).map((product) => (
          <ProductCard
            key={product.id}
            product={product}
            to={routePaths.customerProductDetails.replace(':productId', product.id)}
          />
        ))}
      </div>

      {producerWorkshops.length > 0 && (
        <>
          <SectionHeader
            eyebrow="Live Commerce"
            title="Workshops"
            action={
              <Link to={routePaths.customerWorkshops} className="text-sm font-medium text-link hover:underline">
                View gallery →
              </Link>
            }
          />
          <div className="grid gap-4 sm:grid-cols-3">
            {producerWorkshops.map((workshop) => (
              <div key={workshop.id} className="rounded-xl border border-border bg-surface p-4">
                <Badge tone={workshop.status === 'live' ? 'success' : 'secondary'}>
                  {workshop.status === 'live' ? 'Live Now' : workshop.status === 'upcoming' ? 'Upcoming' : 'Replay'}
                </Badge>
                <p className="mt-3 text-sm font-semibold text-heading">{workshop.title}</p>
                <p className="text-xs text-body/60">{workshop.scheduledFor}</p>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
