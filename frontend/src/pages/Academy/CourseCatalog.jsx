import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, FilterPanel, SearchBar, AsyncState } from '../../components/ui';
import { CourseCard } from '../../components/cards';
import { useCourses } from '../../hooks/useCourses';
import { useCourseCategories } from '../../hooks/useCourseCategories';

export default function CourseCatalog() {
  const [search, setSearch] = useState('');
  const categoriesQuery = useCourseCategories();
  const coursesQuery = useCourses({ pageSize: 24 });

  const courses = (coursesQuery.data?.items || []).filter((c) =>
    search ? c.title.toLowerCase().includes(search.toLowerCase()) : true,
  );

  const filterGroups = [{ label: 'Category', options: (categoriesQuery.data || []).map((c) => c.name) }];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[{ label: 'Home', path: routePaths.home }, { label: 'Academy' }]}
        title="Academy"
        description="Learn traditional crafts and heritage business skills from master artisans."
      />
      <div className="mb-6 max-w-xl">
        <SearchBar placeholder="Search courses…" value={search} onChange={(event) => setSearch(event.target.value)} />
      </div>
      <div className="grid gap-6 lg:grid-cols-[260px_1fr]">
        <FilterPanel groups={filterGroups} />
        <AsyncState isLoading={coursesQuery.isLoading} isError={coursesQuery.isError} error={coursesQuery.error}>
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {courses.map((course) => (
              <CourseCard
                key={course.id}
                course={{
                  title: course.title,
                  mentor: course.authorName,
                  level: course.categoryName || course.category,
                  duration: `${course.lessonCount} lesson${course.lessonCount === 1 ? '' : 's'}`,
                  enrolled: course.activeEnrollmentCount,
                }}
                to={routePaths.academyCourseDetails.replace(':courseId', course.id)}
              />
            ))}
            {courses.length === 0 && <p className="col-span-full text-sm text-body/60">No published courses match your search.</p>}
          </div>
        </AsyncState>
      </div>
    </div>
  );
}
