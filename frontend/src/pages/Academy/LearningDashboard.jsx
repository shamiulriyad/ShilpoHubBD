import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { CourseCard, StatCard } from '../../components/cards';
import { courses } from '../../data/mockData';

export default function LearningDashboard() {
  return (
    <div>
      <PageHeader title="Learning Dashboard" description="Track your enrolled courses and progress." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Enrolled Courses" value="4" />
        <StatCard label="Completed" value="2" />
        <StatCard label="Certificates Earned" value="2" />
        <StatCard label="Hours Learned" value="36" />
      </div>
      <p className="mb-3 text-sm font-semibold text-heading">Continue Learning</p>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {courses.slice(0, 3).map((course) => (
          <CourseCard key={course.id} course={course} to={routePaths.academyCourseDetails.replace(':courseId', course.id)} />
        ))}
      </div>
    </div>
  );
}
