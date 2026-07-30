import { useParams } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Button, Badge } from '../../components/ui';
import { courses } from '../../data/mockData';

export default function CourseDetails() {
  const { courseId } = useParams();
  const course = courses.find((c) => c.id === courseId) || courses[0];

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Academy', path: routePaths.academy },
          { label: course.title },
        ]}
        title={course.title}
        action={<Button variant="primary">Enroll Now</Button>}
      />

      <div className="mb-6 flex aspect-video items-center justify-center rounded-2xl border border-border bg-surface text-sm text-body/40">
        Course Preview Video Placeholder
      </div>

      <div className="mb-6 flex flex-wrap gap-2">
        <Badge tone="primary">{course.level}</Badge>
        <Badge tone="neutral">{course.duration}</Badge>
        <Badge tone="secondary">{course.enrolled} enrolled</Badge>
      </div>

      <div className="grid gap-8 lg:grid-cols-[2fr_1fr]">
        <div className="space-y-4">
          <p className="text-sm font-semibold text-heading">About this course</p>
          <p className="text-sm text-body/70">
            Placeholder course description covering what learners will practice, the tools required, and the
            outcome of completing the course.
          </p>
          <p className="text-sm font-semibold text-heading">Curriculum</p>
          <ul className="space-y-2">
            {['Introduction & Materials', 'Core Technique', 'Guided Practice', 'Final Project & Review'].map((m, i) => (
              <li key={m} className="flex items-center gap-3 rounded-lg border border-border bg-surface p-3 text-sm">
                <span className="flex h-6 w-6 items-center justify-center rounded-full bg-background text-xs">{i + 1}</span>
                {m}
              </li>
            ))}
          </ul>
        </div>
        <div className="h-fit rounded-xl border border-border bg-surface p-5">
          <p className="text-sm font-semibold text-heading">Mentor</p>
          <div className="mt-3 flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
              {course.mentor.slice(0, 1)}
            </span>
            <p className="text-sm text-body">{course.mentor}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
