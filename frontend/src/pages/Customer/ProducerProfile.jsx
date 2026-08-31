import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, SectionHeader, Badge, AsyncState } from '../../components/ui';
import { ProductCard, StatCard } from '../../components/cards';
import { CertificateViewer, TimelineViewer } from '../../components/media';
import { useProducerStory } from '../../hooks/useProducerStories';
import { useProducts } from '../../hooks/useProducts';
import { useWorkshopGallery } from '../../hooks/useWorkshopGallery';
import { useHeritageIdentity } from '../../hooks/useHeritageIdentity';
import { useProducerFollowMutations, useFollowedProducers } from '../../hooks/useProducerFollows';
import { toProductCardItem } from '../../utils/productAdapters';

const byOrder = (a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0);

export default function ProducerProfile() {
  const { producerId } = useParams();
  const storyQuery = useProducerStory(producerId);
  const productsQuery = useProducts({ producerId, pageSize: 50 });
  const galleryQuery = useWorkshopGallery(producerId);
  const identityQuery = useHeritageIdentity(producerId);
  const followedQuery = useFollowedProducers();
  const { follow, unfollow } = useProducerFollowMutations();

  const story = storyQuery.data;
  const identity = identityQuery.data;
  const producerProducts = productsQuery.data?.items || [];
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

        {identity && (
          <div className="mb-12 space-y-8">
            <div>
              <SectionHeader eyebrow="Verified Identity" title="Digital Heritage Identity" />
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard label="Heritage ID" value={identity.heritageIdNumber || '—'} />
                <StatCard label="Legacy Score" value={identity.legacyScore ?? '—'} />
                <StatCard label="Primary Craft" value={identity.primaryCraft || '—'} />
                <StatCard label="Experience" value={identity.yearsOfExperience ? `${identity.yearsOfExperience} yrs` : '—'} />
              </div>
              {identity.workshopName && (
                <p className="mt-3 text-sm text-body/70">
                  <span className="font-medium text-heading">{identity.workshopName}</span>
                  {identity.establishedYear ? ` · est. ${identity.establishedYear}` : ''}
                  {identity.workshopAddress ? ` · ${identity.workshopAddress}` : ''}
                </p>
              )}
            </div>

            {identity.familyMembers?.length > 0 && (
              <div>
                <p className="mb-3 text-sm font-semibold text-heading">Family Heritage</p>
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                  {[...identity.familyMembers].sort(byOrder).map((m) => (
                    <div key={`${m.fullName}-${m.generation}`} className="rounded-xl border border-border bg-surface p-4">
                      <p className="text-sm font-semibold text-heading">{m.fullName}</p>
                      <p className="text-xs text-body/60">
                        {m.relation} · Generation {m.generation}
                        {m.activeYearsRange ? ` · ${m.activeYearsRange}` : ''}
                      </p>
                      {m.role && <p className="mt-1 text-xs text-body/50">{m.role}</p>}
                      {m.story && <p className="mt-2 text-sm text-body/70">{m.story}</p>}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {identity.skillTimeline?.length > 0 && (
              <div>
                <p className="mb-3 text-sm font-semibold text-heading">Skill Journey</p>
                <TimelineViewer
                  items={[...identity.skillTimeline].sort(byOrder).map((s) => ({
                    marker: s.year,
                    title: s.title,
                    description: s.description,
                  }))}
                />
              </div>
            )}

            {identity.certifications?.length > 0 && (
              <div>
                <p className="mb-3 text-sm font-semibold text-heading">Certificates of Authenticity</p>
                <div className="grid gap-4 sm:grid-cols-2">
                  {[...identity.certifications].sort(byOrder).map((c) => (
                    <CertificateViewer
                      key={c.name + c.issuedYear}
                      title={c.name}
                      issuedTo={c.issuingBody}
                      issuedDate={String(c.issuedYear)}
                      certId={c.certificateNumber}
                    />
                  ))}
                </div>
              </div>
            )}

            {identity.awards?.length > 0 && (
              <div>
                <p className="mb-3 text-sm font-semibold text-heading">Awards & Recognition</p>
                <ul className="space-y-2">
                  {[...identity.awards].sort(byOrder).map((a) => (
                    <li key={a.title + a.year} className="rounded-lg border border-border bg-surface p-3 text-sm">
                      <span className="font-medium text-heading">{a.title}</span>
                      <span className="text-body/60"> — {a.issuingOrganization}, {a.year}</span>
                      {a.description && <p className="mt-1 text-xs text-body/60">{a.description}</p>}
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}

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
