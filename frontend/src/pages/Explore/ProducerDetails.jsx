import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, AsyncState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { useProducerStory } from '../../hooks/useProducerStories';

// NOTE: no producer-profile endpoint yet — this view is built from the producer's
// heritage story (`GET /api/producer-stories/{producerId}`).
export default function ProducerDetails() {
  const { producerId } = useParams();
  const { data: story, isLoading, isError, error } = useProducerStory(producerId);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={isLoading} isError={isError} error={error} loadingText="Loading producer…">
        {story ? (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Explore', path: routePaths.explore },
                { label: 'Producers', path: routePaths.exploreProducers },
                { label: story.producerName },
              ]}
              title={story.producerName}
              description={story.quote ? `“${story.quote}”` : undefined}
              action={<Button variant="primary">Contact Producer</Button>}
            />

            <div className="mb-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
              <StatCard label="Heritage ID" value={story.heritageId || '—'} />
              <StatCard label="Generations" value={story.generations || '—'} />
              <StatCard label="Since" value={story.foundingYear ? String(story.foundingYear) : '—'} />
            </div>

            {story.chapters?.length > 0 && (
              <div className="space-y-6">
                {[...story.chapters]
                  .sort((a, b) => a.displayOrder - b.displayOrder)
                  .map((chapter) => (
                    <section key={chapter.heading}>
                      <h2 className="mb-2 text-sm font-semibold text-heading">{chapter.heading}</h2>
                      <p className="max-w-3xl text-sm leading-relaxed text-body/70">{chapter.body}</p>
                    </section>
                  ))}
              </div>
            )}
          </>
        ) : (
          <p className="py-10 text-center text-sm text-body/60">
            This producer has not published a profile yet.
          </p>
        )}
      </AsyncState>
    </div>
  );
}
