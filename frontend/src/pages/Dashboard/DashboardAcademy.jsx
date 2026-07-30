import { Link } from 'react-router-dom';
import { PageHeader } from '../../components/ui';
import { CourseCard } from '../../components/cards';
import { courses } from '../../data/mockData';
import { routePaths } from '../../routes/routePaths';

export default function DashboardAcademy() {
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
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {courses.slice(0, 3).map((course) => (
          <CourseCard key={course.id} course={course} to={routePaths.academyCourseDetails.replace(':courseId', course.id)} />
        ))}
      </div>
    </div>
  );
}
