import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { CourseCard, StatCard } from '../../components/cards';
import { useMyEnrollments } from '../../hooks/useEnrollments';
import { useMyTrainingCertificates } from '../../hooks/useTrainingCertificates';

export default function LearningDashboard() {
  const enrollmentsQuery = useMyEnrollments();
  const certificatesQuery = useMyTrainingCertificates();

  const enrollments = enrollmentsQuery.data || [];
  const completed = enrollments.filter((e) => e.status === 'Completed').length;
  const inProgress = enrollments.filter((e) => e.status !== 'Completed');

  return (
    <div>
      <PageHeader title="Learning Dashboard" description="Track your enrolled courses and progress." />
      <div className="mb-6 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Enrolled Courses" value={enrollments.length} />
        <StatCard label="Completed" value={completed} />
        <StatCard label="Certificates Earned" value={certificatesQuery.data?.length ?? '—'} />
        <StatCard label="Avg. Progress" value={`${Math.round(enrollments.reduce((sum, e) => sum + e.progressPercent, 0) / (enrollments.length || 1))}%`} />
      </div>
      <p className="mb-3 text-sm font-semibold text-heading">Continue Learning</p>
      <AsyncState isLoading={enrollmentsQuery.isLoading} isError={enrollmentsQuery.isError} error={enrollmentsQuery.error}>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {inProgress.map((enrollment) => (
            <CourseCard
              key={enrollment.id}
              course={{
                title: enrollment.courseTitle,
                mentor: '',
                level: enrollment.status,
                duration: `${enrollment.completedLessons}/${enrollment.totalLessons} lessons`,
                enrolled: `${Math.round(enrollment.progressPercent)}%`,
              }}
              to={routePaths.academyCourseDetails.replace(':courseId', enrollment.courseId)}
            />
          ))}
          {inProgress.length === 0 && (
            <p className="col-span-full text-sm text-body/60">You're not enrolled in any courses yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
