import VideoPlayer from './VideoPlayer';
import Button from '../ui/Button';
import ProductCard from '../cards/ProductCard';

export default function LiveShoppingPlayer({ workshop, products = [], getProductLink }) {
  const isLive = workshop.status === 'live';

  return (
    <div className="space-y-4">
      <VideoPlayer title={workshop.title} live={isLive} viewers={workshop.viewers} />

      <p className="text-sm font-semibold text-heading">Shop this stream</p>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        {products.map((product) => (
          <div key={product.id} className="relative">
            <ProductCard product={product} to={getProductLink?.(product)} />
            <Button variant="primary" className="mt-2 w-full">
              Buy Now
            </Button>
          </div>
        ))}
      </div>
    </div>
  );
}
