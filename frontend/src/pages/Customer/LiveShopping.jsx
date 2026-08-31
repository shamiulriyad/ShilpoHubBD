import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Badge, Button, ChatBox, AsyncState } from '../../components/ui';
import { LiveShoppingPlayer } from '../../components/media';
import { useLiveEvent, useLiveEventInteractions } from '../../hooks/useLiveEvents';
import { useAuth } from '../../hooks/useAuth';

export default function LiveShopping() {
  const { workshopId } = useParams();
  const { isAuthenticated } = useAuth();
  const { data: event, isLoading, isError, error } = useLiveEvent(workshopId);
  const { addComment } = useLiveEventInteractions(workshopId);

  const status = (event?.status || '').toLowerCase();
  const products = event
    ? [
        {
          id: event.productId,
          name: event.productName,
          price: event.productPrice,
          image: event.productImageUrl,
          category: 'Featured',
        },
      ]
    : [];
  const chat = (event?.comments || []).map((c) => ({ id: c.id, from: c.authorName, text: c.body }));

  return (
    <div>
      <AsyncState isLoading={isLoading} isError={isError} error={error} loadingText="Loading live event…">
        {event && (
          <>
            <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
              <div>
                <Badge tone={status === 'live' ? 'success' : 'secondary'}>
                  {status === 'live' ? 'Live now' : status === 'ended' ? 'Replay' : 'Scheduled'}
                </Badge>
                <h1 className="mt-2 text-2xl font-semibold text-heading">{event.title}</h1>
                <Link
                  to={routePaths.customerProducerProfile.replace(':producerId', event.producerId)}
                  className="text-sm text-link hover:underline"
                >
                  {event.producerName}
                </Link>
              </div>
              <Link to={routePaths.customerWorkshops}>
                <Button variant="secondary">Back to Gallery</Button>
              </Link>
            </div>

            {event.description && <p className="mb-6 max-w-3xl text-sm text-body/70">{event.description}</p>}

            <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
              <LiveShoppingPlayer
                workshop={{ title: event.title, status, viewers: event.purchaseCount }}
                products={products}
                getProductLink={(product) =>
                  routePaths.customerProductDetails.replace(':productId', product.id)
                }
              />

              <ChatBox
                title={`Live Chat · ${event.commentCount} comments`}
                messages={chat}
                onSend={
                  isAuthenticated ? (text) => addComment.mutate(text) : undefined
                }
                className="h-[520px]"
              />
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
