import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import CardMedia from '../../components/media/CardMedia';
import { useHeritagePlace } from '../../hooks/useHeritagePlaces';

export default function HeritagePlaceDetails() {
  const { placeId } = useParams();
  const query = useHeritagePlace(placeId);
  const place = query.data;
  return <main className="mx-auto max-w-6xl px-4 py-10 lg:px-8"><AsyncState isLoading={query.isLoading} isError={query.isError} error={query.error}>{place && <><PageHeader title={place.name} description={`${place.placeType} · ${place.districtName}`} breadcrumbs={[{label:'Tourism',path:routePaths.tourism},{label:'Heritage places',path:routePaths.tourismMap},{label:place.name}]}/><div className="grid gap-8 lg:grid-cols-[1.4fr_1fr]"><div className="overflow-hidden rounded-2xl border border-border"><CardMedia src={place.imageUrl} name={place.name}/></div><article className="rounded-2xl border border-border bg-surface p-6"><div className="flex flex-wrap gap-2"><Badge tone="secondary">{place.placeType}</Badge>{place.isFeatured && <Badge tone="success">Featured</Badge>}</div><h2 className="mt-5 text-xl font-semibold">About this place</h2><p className="mt-3 whitespace-pre-line leading-7 text-body/80">{place.description || 'Details will be published by ShilpoHub administrators.'}</p><dl className="mt-6 space-y-4 border-t border-border pt-5 text-sm"><div><dt className="font-semibold">District</dt><dd>{place.districtName}</dd></div>{place.address && <div><dt className="font-semibold">Address</dt><dd>{place.address}</dd></div>}<div><dt className="font-semibold">Location</dt><dd>{place.latitude.toFixed(5)}, {place.longitude.toFixed(5)}</dd></div>{place.reviewCount > 0 && <div><dt className="font-semibold">Visitor rating</dt><dd>★ {place.averageRating.toFixed(1)} from {place.reviewCount} reviews</dd></div>}</dl></article></div></>}</AsyncState></main>;
}
