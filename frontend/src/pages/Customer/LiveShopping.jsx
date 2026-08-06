import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Badge, Button } from '../../components/ui';
import { ProductCard } from '../../components/cards';
import { workshops, products } from '../../data/mockData';

const seedChat = [
  { id: 1, from: 'Rashed', text: 'The colors are gorgeous!' },
  { id: 2, from: 'Mitu', text: 'Is this available in size medium?' },
  { id: 3, from: 'Producer', text: 'Yes! Taking orders live right now.' },
];

export default function LiveShopping() {
  const { workshopId } = useParams();
  const workshop = workshops.find((w) => w.id === workshopId) || workshops[0];
  const featured = products.slice(0, 4);
  const [chat, setChat] = useState(seedChat);
  const [message, setMessage] = useState('');

  function sendMessage(event) {
    event.preventDefault();
    if (!message.trim()) return;
    setChat((prev) => [...prev, { id: prev.length + 1, from: 'You', text: message.trim() }]);
    setMessage('');
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
        <div className="space-y-4">
          <div className="flex aspect-video items-center justify-center rounded-2xl border border-border bg-background text-sm text-body/40">
            Live Stream Player
          </div>

          <p className="text-sm font-semibold text-heading">Shop this stream</p>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            {featured.map((product) => (
              <div key={product.id} className="relative">
                <ProductCard product={product} to={routePaths.customerProductDetails.replace(':productId', product.id)} />
                <Button variant="primary" className="mt-2 w-full">
                  Buy Now
                </Button>
              </div>
            ))}
          </div>
        </div>

        <div className="flex h-[520px] flex-col rounded-xl border border-border bg-surface">
          <p className="border-b border-border p-4 text-sm font-semibold text-heading">Live Chat</p>
          <div className="flex-1 space-y-3 overflow-y-auto p-4">
            {chat.map((entry) => (
              <p key={entry.id} className="text-sm">
                <span className="font-medium text-heading">{entry.from}: </span>
                <span className="text-body/70">{entry.text}</span>
              </p>
            ))}
          </div>
          <form onSubmit={sendMessage} className="flex gap-2 border-t border-border p-3">
            <input
              value={message}
              onChange={(event) => setMessage(event.target.value)}
              placeholder="Say something…"
              className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
            />
            <Button type="submit" variant="primary">
              Send
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
