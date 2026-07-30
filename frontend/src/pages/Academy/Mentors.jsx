import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { mentors } from '../../data/mockData';

export default function Mentors() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Academy', path: routePaths.academy },
          { label: 'Mentors' },
        ]}
        title="Mentors"
        description="Master artisans and trainers sharing their craft."
      />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {mentors.map((mentor) => (
          <EntityCard
            key={mentor.id}
            title={mentor.name}
            subtitle={mentor.expertise}
            meta={`${mentor.students} students`}
          />
        ))}
      </div>
    </div>
  );
}
