import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { EntityCard } from '../../components/cards';
import { useMentors } from '../../hooks/useMentors';

export default function Mentors() {
  const { data, isLoading, isError, error } = useMentors({ pageSize: 24 });
  const mentors = data?.items || [];

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
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {mentors.map((mentor) => (
            <EntityCard
              key={mentor.id}
              title={mentor.fullName}
              subtitle={mentor.expertise}
              meta={`${mentor.publishedCourseCount} course${mentor.publishedCourseCount === 1 ? '' : 's'} · ${mentor.yearsOfExperience}y experience`}
            />
          ))}
          {mentors.length === 0 && <p className="col-span-full text-sm text-body/60">No mentors listed yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
