import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, SectionHeader, Badge, AsyncState } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { useProducerStory } from '../../hooks/useProducerStories';
import { useProducts } from '../../hooks/useProducts';
import { useWorkshopGallery } from '../../hooks/useWorkshopGallery';
import { useProducerFollowMutations, useFollowedProducers } from '../../hooks/useProducerFollows';
import { toProductCardItem } from '../../utils/productAdapters';

export default function ProducerProfile() {
  const { producerId } = useParams();
  const storyQuery = useProducerStory(producerId);
  // ProductsController has no producer-id filter on the public listing endpoint yet,
  // so products for a specific producer are found by filtering a broad fetch client-side.
  const productsQuery = useProducts({ pageSize: 100 });
  const galleryQuery = useWorkshopGallery(producerId);
  const followedQuery = useFollowedProducers();
  const { follow, unfollow } = useProducerFollowMutations();

  const story = storyQuery.data;
  const producerProducts = (productsQuery.data?.items || []).filter((p) => p.producerId === producerId);
  const isFollowing = (followedQuery.data || []).some((f) => f.producerId === producerId);
  const producerName = story?.producerName || producerProducts[0]?.producerName || 'Producer';

  return (
    <div>
      <AsyncState isLoading={storyQuery.isLoading} isError={storyQuery.isError && storyQuery.error?.response?.status !== 404} error={storyQuery.error}>
        <PageHeader
          breadcrumbs={[
            { label: 'Dashboard', path: routePaths.customer },
            { label: 'Marketplace', path: routePaths.customerMarketplace },
            { label: producerName },
          ]}
          title={producerName}
          action={
            <div className="flex flex-wrap gap-3">
              <Button
                variant="secondary"
                onClick={() => (isFollowing ? unfollow.mutate(producerId) : follow.mutate(producerId))}
              >
                {isFollowing ? 'Unfollow' : 'Follow'}
              </Button>
              <Link to={routePaths.customerCustomOrder}>
                <Button variant="secondary">Request Custom Order</Button>
              </Link>
              {story && (
                <Link to={routePaths.customerProducerStory.replace(':producerId', producerId)}>
                  <Button variant="primary">Read Full Story</Button>
                </Link>
              )}
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
          <StatCard label="Products Listed" value={producerProducts.length} />
          <StatCard label="Generations" value={story?.generations ?? '—'} />
          <StatCard label="Founded" value={story?.foundingYear ?? '—'} />
        </div>

        <SectionHeader eyebrow="Shop" title={`Products by ${producerName}`} />
        <div className="mb-12 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {producerProducts.map((product) => (
            <ProductCard
              key={product.id}
              product={toProductCardItem(product)}
              to={routePaths.customerProductDetails.replace(':productId', product.id)}
            />
          ))}
          {producerProducts.length === 0 && (
            <p className="col-span-full text-sm text-body/60">No products listed yet.</p>
          )}
        </div>

        {galleryQuery.data?.length > 0 && (
          <>
            <SectionHeader eyebrow="Behind the Scenes" title="Workshop Gallery" />
            <div className="grid gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {galleryQuery.data.map((item) => (
                <div key={item.id} className="overflow-hidden rounded-xl border border-border bg-surface">
                  <div className="flex aspect-square items-center justify-center bg-background text-xs text-body/40">
                    {item.mediaType === 'Video' ? (
                      <video src={item.mediaUrl} className="h-full w-full object-cover" controls />
                    ) : (
                      <img src={item.mediaUrl} alt={item.caption || ''} className="h-full w-full object-cover" />
                    )}
                  </div>
                  {item.caption && <p className="p-2 text-xs text-body/60">{item.caption}</p>}
                </div>
              ))}
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
