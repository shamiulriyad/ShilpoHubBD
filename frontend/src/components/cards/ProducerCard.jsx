import OptionalCardLink from './OptionalCardLink';
import Badge from '../ui/Badge';
import CardMedia from '../media/CardMedia';

export default function ProducerCard({ producer, to }) {
  return (
    <OptionalCardLink
      to={to}
      className="group flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
    >
      <CardMedia src={producer.image || producer.imageUrl || producer.profileImageUrl} name={producer.name} kind="producer" />
      <div className="flex flex-1 flex-col gap-1.5 p-4">
        <div className="flex items-center justify-between gap-2">
          <h3 className="truncate text-sm font-semibold text-heading group-hover:text-primary">{producer.name}</h3>
          {producer.rating && <Badge tone="secondary">★ {producer.rating}</Badge>}
        </div>
        <p className="text-xs text-body/60">{producer.craft}</p>
        <p className="text-xs text-body/50">{producer.district}</p>
      </div>
    </OptionalCardLink>
  );
}
