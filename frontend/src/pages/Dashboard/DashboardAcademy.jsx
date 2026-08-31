import { Link } from 'react-router-dom';
import { PageHeader, AsyncState } from '../../components/ui';
import { CourseCard } from '../../components/cards';
import { routePaths } from '../../routes/routePaths';
import { useCourses } from '../../hooks/useCourses';

const listOf = (data) => data?.items || data || [];

const toCourseCardItem = (c) => ({
  level: c.status || 'Course',
  title: c.title,
  mentor: c.authorName,
  duration: `${c.lessonCount ?? 0} lessons`,
  enrolled: c.activeEnrollmentCount ?? 0,
});

export default function DashboardAcademy() {
  const { data, isLoading, isError, error } = useCourses({ pageSize: 3 });
  const courses = listOf(data);

  return (
    <div>
      <PageHeader
        title="Academy"
        description="Your enrolled courses and learning progress."
        action={
          <div className="flex gap-3 text-sm font-medium text-link">
            <Link to={routePaths.academyCertificates} className="hover:underline">
              Certificates
            </Link>
            <Link to={routePaths.academyPortfolio} className="hover:underline">
              Portfolio
            </Link>
          </div>
        }
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {courses.map((course) => (
            <CourseCard
              key={course.id}
              course={toCourseCardItem(course)}
              to={routePaths.academyCourseDetails.replace(':courseId', course.id)}
            />
          ))}
          {courses.length === 0 && (
            <p className="text-sm text-body/60">No courses available yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
