import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Badge, Button, ChatBox, AsyncState } from '../../components/ui';
import { LiveShoppingPlayer } from '../../components/media';
import { useFeaturedProducts } from '../../hooks/useProducts';
import { toProductCardItem } from '../../utils/productAdapters';

// TODO(backend): no live-workshop resource yet (only per-producer galleries).
// Placeholder stream + seed chat until a live-commerce API exists.
const workshop = {
  id: 'workshop-live',
  title: 'Live Jamdani Loom Session',
  producerId: 'producer-1',
  producer: 'Rahima Begum',
  craft: 'Jamdani Weaving',
  status: 'live',
  viewers: 128,
};

const seedChat = [
  { id: 1, from: 'Rashed', text: 'The colors are gorgeous!' },
  { id: 2, from: 'Mitu', text: 'Is this available in size medium?' },
  { id: 3, from: 'Producer', text: 'Yes! Taking orders live right now.' },
];

export default function LiveShopping() {
  const featuredQuery = useFeaturedProducts(4);
  const featured = (featuredQuery.data || []).map(toProductCardItem);
  const [chat, setChat] = useState(seedChat);

  function sendMessage(text) {
    setChat((prev) => [...prev, { id: prev.length + 1, from: 'You', text, self: true }]);
  }

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <Badge tone={workshop.status === 'live' ? 'success' : 'secondary'}>
            {workshop.status === 'live' ? `Live · ${workshop.viewers || 0} watching` : 'Replay'}
          </Badge>
          <h1 className="mt-2 text-2xl font-semibold text-heading">{workshop.title}</h1>
          <Link
            to={routePaths.customerProducerProfile.replace(':producerId', workshop.producerId)}
            className="text-sm text-link hover:underline"
          >
            {workshop.producer} · {workshop.craft}
          </Link>
        </div>
        <Link to={routePaths.customerWorkshops}>
          <Button variant="secondary">Back to Gallery</Button>
        </Link>
      </div>

      <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
        <AsyncState isLoading={featuredQuery.isLoading} isError={featuredQuery.isError} error={featuredQuery.error}>
          <LiveShoppingPlayer
            workshop={workshop}
            products={featured}
            getProductLink={(product) => routePaths.customerProductDetails.replace(':productId', product.id)}
          />
        </AsyncState>

        <ChatBox title="Live Chat" messages={chat} onSend={sendMessage} className="h-[520px]" />
      </div>
    </div>
  );
}
