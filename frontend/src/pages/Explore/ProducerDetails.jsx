import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, QueryState } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { useProducerStory } from '../../hooks/queries/useCatalog';

// NOTE: no producer-profile endpoint yet — this view is built from the
// producer's heritage story (`GET /api/producer-stories/{producerId}`).
export default function ProducerDetails() {
  const { producerId } = useParams();
  const query = useProducerStory(producerId);

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <QueryState
        query={query}
        loadingLabel="Loading producer…"
        emptyLabel="This producer has not published a profile yet."
      >
        {(story) => (
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
        )}
      </QueryState>
    </div>
  );
}
