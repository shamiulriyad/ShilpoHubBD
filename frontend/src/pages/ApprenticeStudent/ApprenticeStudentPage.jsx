import { Link } from 'react-router-dom';
import RoleOverview from '../../components/shared/RoleOverview';
import { routePaths } from '../../routes/routePaths';

export default function ApprenticeStudentPage() {
  return (
    <div>
      <RoleOverview
        title="Apprentice / Student Dashboard"
        description="Track your enrolled courses, mentors and certifications."
        stats={[
          { label: 'Enrolled Courses', value: '3' },
          { label: 'Completed', value: '1' },
          { label: 'Certificates', value: '1' },
          { label: 'Mentor Sessions', value: '6' },
        ]}
        highlights={[
          { title: 'Learning Dashboard', description: 'Continue your active courses' },
          { title: 'Portfolio', description: 'Showcase your craft projects' },
        ]}
      />
      <div className="mt-4 flex flex-wrap gap-3">
        <Link
          to={routePaths.apprenticeStudentMyApprenticeships}
          className="inline-block rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md"
        >
          My Apprenticeships →
        </Link>
        <Link
          to={routePaths.apprenticeStudentBrowsePrograms}
          className="inline-block rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md"
        >
          Browse Programs →
        </Link>
      </div>
    </div>
  );
}
