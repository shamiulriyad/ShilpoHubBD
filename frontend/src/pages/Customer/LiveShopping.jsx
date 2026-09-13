import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Badge, ChatBox, AsyncState } from '../../components/ui';
import { LiveShoppingPlayer } from '../../components/media';
import { useLiveEvent, useLiveEventInteractions } from '../../hooks/useLiveEvents';
import { getApiErrorMessage } from '../../utils/apiError';

export default function LiveShopping() {
  const { workshopId } = useParams();
  const { data: event, isLoading, isError, error } = useLiveEvent(workshopId);
  const { addComment, buyDuringLive } = useLiveEventInteractions(workshopId);
  const [purchaseMessage, setPurchaseMessage] = useState('');

  const status = (event?.status || '').toLowerCase();
  const products = event
    ? [{
        id: event.productId,
        name: event.productName,
        price: event.productPrice,
        image: event.productImageUrl,
        category: 'Live event product',
      }]
    : [];
  const chat = (event?.comments || []).map((comment) => ({
    id: comment.id,
    from: comment.authorName,
    text: comment.body,
  }));

  const handleBuy = () => {
    if (buyDuringLive.isPending) return;
    setPurchaseMessage('');
    buyDuringLive.mutate(
      { quantity: 1 },
      {
        onSuccess: () => setPurchaseMessage('Added to your cart from this live event.'),
        onError: (mutationError) => setPurchaseMessage(getApiErrorMessage(mutationError, 'Unable to add this product to your cart.')),
      },
    );
  };

  return (
    <div>
      <AsyncState isLoading={isLoading} isError={isError} error={error} loadingText="Loading live event…">
        {event && (
          <>
            <div className="mb-6 flex flex-wrap items-start justify-between gap-3">
              <div className="min-w-0">
                <Badge tone={status === 'live' ? 'success' : status === 'ended' ? 'neutral' : 'secondary'}>
                  {status === 'live' ? 'Live now' : status === 'ended' ? 'Ended' : status === 'cancelled' ? 'Cancelled' : 'Scheduled'}
                </Badge>
                <h1 className="mt-2 break-words text-2xl font-semibold text-heading">{event.title}</h1>
                <Link
                  to={routePaths.customerProducerProfile.replace(':producerId', event.producerId)}
                  className="text-sm text-link hover:underline"
                >
                  {event.producerName}
                </Link>
                <p className="mt-1 text-xs text-body/55">Scheduled {new Date(event.scheduledStartAt).toLocaleString()}</p>
              </div>
              <Link
                to={routePaths.customerWorkshops}
                className="inline-flex rounded-full border border-border bg-surface px-4 py-2 text-sm font-semibold text-title transition hover:bg-background"
              >
                Back to events
              </Link>
            </div>

            {event.description && <p className="mb-6 max-w-3xl text-sm leading-6 text-body/70">{event.description}</p>}

            {purchaseMessage && (
              <p
                role="status"
                className={`mb-4 rounded-lg border px-3 py-2 text-sm ${buyDuringLive.isError ? 'border-error/30 bg-error/10 text-error' : 'border-success/30 bg-success/10 text-success'}`}
              >
                {purchaseMessage}
              </p>
            )}

            <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
              <LiveShoppingPlayer
                event={{ title: event.title, status }}
                products={products}
                getProductLink={(product) => routePaths.customerProductDetails.replace(':productId', product.id)}
                onBuy={status === 'live' ? handleBuy : undefined}
                isBuying={buyDuringLive.isPending}
              />

              <ChatBox
                title={`Event chat · ${event.commentCount} comments`}
                messages={chat}
                onSend={status === 'live' ? (text) => addComment.mutate(text) : undefined}
                className="min-h-[28rem] max-h-[36rem]"
              />
            </div>
            {addComment.isError && (
              <p role="alert" className="mt-3 text-sm text-error">{getApiErrorMessage(addComment.error, 'Unable to send your message.')}</p>
            )}
          </>
        )}
      </AsyncState>
    </div>
  );
}
