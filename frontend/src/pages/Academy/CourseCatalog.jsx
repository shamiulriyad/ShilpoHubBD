import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, SearchBar } from '../../components/ui';
import { CourseCard } from '../../components/cards';
import { courses } from '../../data/mockData';

const filterGroups = [
  { label: 'Level', options: ['Beginner', 'Intermediate', 'Advanced'] },
  { label: 'Craft', options: ['Weaving', 'Embroidery', 'Pottery', 'Business'] },
];

export default function CourseCatalog() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Academy' }]}
        title="Academy"
        description="Learn traditional crafts and heritage business skills from master artisans."
      />
      <div className="mb-6 max-w-xl">
        <SearchBar placeholder="Search courses…" />
      </div>
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {courses.map((course) => (
            <CourseCard key={course.id} course={course} to={routePaths.academyCourseDetails.replace(':courseId', course.id)} />
          ))}
        </div>
      </div>
    </div>
  );
}
