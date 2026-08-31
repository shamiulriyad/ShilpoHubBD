import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';

// Editorial content — Bangladesh's elements on UNESCO's Representative List of the
// Intangible Cultural Heritage of Humanity. There is no backend resource for this;
// update here when the inscription list changes.
const unescoElements = [
  {
    name: 'Traditional art of Jamdani weaving',
    year: 2013,
    summary:
      'The fine muslin textile of Dhaka, hand-woven on the loom with a supplementary weft technique passed down through generations of weavers.',
  },
  {
    name: 'Baul songs',
    year: 2008,
    summary:
      'The mystical folk songs of the Baul community of Bengal, blending devotion, philosophy and everyday life into an oral musical tradition.',
  },
  {
    name: 'Mangal Shobhajatra on Pahela Baishakh',
    year: 2016,
    summary:
      'The colourful new-year procession organised by the students and teachers of the Faculty of Fine Arts, University of Dhaka.',
  },
  {
    name: 'Traditional art of Shital Pati weaving of Sylhet',
    year: 2017,
    summary:
      'The craft of weaving a cool, smooth sitting mat from the slips of a cane known locally as murta.',
  },
  {
    name: 'Rickshaw and rickshaw painting in Dhaka',
    year: 2023,
    summary:
      'The decorative painting and craftsmanship of Dhaka’s cycle rickshaws, a living urban folk-art tradition.',
  },
];

export default function Unesco() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Explore', path: routePaths.explore },
          { label: 'UNESCO Heritage' },
        ]}
        title="UNESCO Heritage"
        description="Elements of Bangladesh's intangible cultural heritage recognized by UNESCO."
      />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {unescoElements.map((element) => (
          <div key={element.name} className="overflow-hidden rounded-xl border border-border bg-surface">
            <div className="flex aspect-video items-center justify-center bg-background text-xs text-body/40">
              Heritage Image
            </div>
            <div className="space-y-2 p-4">
              <Badge tone="success">Inscribed {element.year}</Badge>
              <p className="text-sm font-semibold text-heading">{element.name}</p>
              <p className="text-xs text-body/60">{element.summary}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
