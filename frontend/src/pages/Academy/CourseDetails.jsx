import { Link, useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge, AsyncState } from '../../components/ui';
import { useCourse } from '../../hooks/useCourses';
import { useEnrollmentMutations, useMyEnrollments } from '../../hooks/useEnrollments';
import { useCourseExams } from '../../hooks/useExams';
import { useCourseQuizzes } from '../../hooks/useQuizzes';
import { useCourseAssignments } from '../../hooks/useAssignments';
import { useAuth } from '../../hooks/useAuth';

export default function CourseDetails() {
  const { courseId } = useParams();
  const { isAuthenticated } = useAuth();
  const courseQuery = useCourse(courseId);
  const enrollmentsQuery = useMyEnrollments(isAuthenticated);
  const { enroll } = useEnrollmentMutations();
  const examsQuery = useCourseExams(courseId);
  const quizzesQuery = useCourseQuizzes(courseId);
  const assignmentsQuery = useCourseAssignments(courseId);

  const course = courseQuery.data;
  const enrollment = (enrollmentsQuery.data || []).find((e) => e.courseId === courseId);

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <AsyncState isLoading={courseQuery.isLoading} isError={courseQuery.isError} error={courseQuery.error}>
        {course && (
          <>
            <PageHeader
              breadcrumbs={[
                { label: 'Home', path: routePaths.home },
                { label: 'Academy', path: routePaths.academy },
                { label: course.title },
              ]}
              title={course.title}
              action={
                isAuthenticated ? (
                  enrollment ? (
                    <Link to={routePaths.academyLearning}>
                      <Button variant="secondary">Continue Learning ({Math.round(enrollment.progressPercent)}%)</Button>
                    </Link>
                  ) : (
                    <Button variant="primary" onClick={() => enroll.mutate(courseId)} disabled={enroll.isPending}>
                      {enroll.isPending ? 'Enrolling…' : 'Enroll Now'}
                    </Button>
                  )
                ) : (
                  <Link to={routePaths.login}>
                    <Button variant="primary">Log in to Enroll</Button>
                  </Link>
                )
              }
            />

            <div className="mb-6 flex aspect-video items-center justify-center rounded-2xl border border-border bg-surface text-sm text-body/40">
              Course Preview Video Placeholder
            </div>

            <div className="mb-6 flex flex-wrap gap-2">
              <Badge tone="primary">{course.categoryName || course.category}</Badge>
              <Badge tone="neutral">{course.lessons.length} lessons</Badge>
              <Badge tone="secondary">{course.activeEnrollmentCount} enrolled</Badge>
            </div>

            <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
              <div className="space-y-4">
                <p className="text-sm font-semibold text-heading">About this course</p>
                <p className="text-sm text-body/70">{course.description}</p>

                <p className="text-sm font-semibold text-heading">Curriculum</p>
                <ul className="space-y-2">
                  {course.lessons.map((lesson, i) => (
                    <li key={lesson.id} className="flex items-center gap-3 rounded-lg border border-border bg-surface p-3 text-sm">
                      <span className="flex h-6 w-6 items-center justify-center rounded-full bg-background text-xs">{i + 1}</span>
                      {lesson.title}
                    </li>
                  ))}
                  {course.lessons.length === 0 && <p className="text-sm text-body/60">No lessons published yet.</p>}
                </ul>

                {enrollment && (
                  <>
                    <p className="text-sm font-semibold text-heading">Assessments</p>
                    <div className="grid gap-2 sm:grid-cols-3">
                      {(examsQuery.data || []).map((exam) => (
                        <Link
                          key={exam.id}
                          to={routePaths.academyExamDetails.replace(':examId', exam.id)}
                          className="rounded-lg border border-border bg-surface p-3 text-sm hover:border-primary"
                        >
                          📝 {exam.title}
                        </Link>
                      ))}
                      {(quizzesQuery.data || []).map((quiz) => (
                        <Link
                          key={quiz.id}
                          to={routePaths.academyQuizDetails.replace(':quizId', quiz.id)}
                          className="rounded-lg border border-border bg-surface p-3 text-sm hover:border-primary"
                        >
                          ❓ {quiz.title}
                        </Link>
                      ))}
                      {(assignmentsQuery.data || []).map((assignment) => (
                        <Link
                          key={assignment.id}
                          to={routePaths.academyAssignmentDetails.replace(':assignmentId', assignment.id)}
                          className="rounded-lg border border-border bg-surface p-3 text-sm hover:border-primary"
                        >
                          📎 {assignment.title}
                        </Link>
                      ))}
                    </div>
                  </>
                )}
              </div>
              <div className="h-fit rounded-xl border border-border bg-surface p-5">
                <p className="text-sm font-semibold text-heading">Mentor</p>
                <div className="mt-3 flex items-center gap-3">
                  <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                    {course.authorName.slice(0, 1)}
                  </span>
                  <p className="text-sm text-body">{course.authorName}</p>
                </div>
              </div>
            </div>
          </>
        )}
      </AsyncState>
    </div>
  );
}
