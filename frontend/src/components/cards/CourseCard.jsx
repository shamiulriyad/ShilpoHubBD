import { Link } from 'react-router-dom';
import Badge from '../ui/Badge';

export default function CourseCard({ course, to }) {
  return (
    <Link
      to={to || '#'}
      className="group flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
    >
      <div className="flex aspect-video items-center justify-center bg-background text-xs text-body/40">
        Course Thumbnail
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <Badge tone="primary">{course.level}</Badge>
        <h3 className="text-sm font-semibold text-heading group-hover:text-primary">{course.title}</h3>
        <p className="text-xs text-body/60">Mentor: {course.mentor}</p>
        <div className="mt-auto flex items-center justify-between text-xs text-body/50">
          <span>{course.duration}</span>
          <span>{course.enrolled} enrolled</span>
        </div>
      </div>
    </Link>
  );
}
