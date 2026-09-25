import NavigationIcon from '../layout/NavigationIcon';
import OptionalCardLink from './OptionalCardLink';
import Badge from '../ui/Badge';

export default function CourseCard({ course, to }) {
  return (
    <OptionalCardLink
      to={to}
      className="group flex flex-col overflow-hidden rounded-xl border border-border bg-surface transition hover:shadow-md"
    >
      <div className="flex h-24 items-center justify-between border-b border-border bg-primary-soft px-6 text-primary">
        <NavigationIcon label="Learning Dashboard"/><span className="text-xs font-medium uppercase tracking-widest">Heritage Academy</span>
      </div>
      <div className="flex flex-1 flex-col gap-3 p-5">
        <Badge tone="primary">{course.level}</Badge>
        <h3 className="text-base font-semibold text-heading group-hover:text-primary">{course.title}</h3>
        <p className="text-xs text-body/60">Mentor: {course.mentor}</p>
        <div className="mt-auto flex items-center justify-between text-xs text-body/50">
          <span>{course.duration}</span>
          <span>{course.enrolled} enrolled</span>
        </div>
      </div>
    </OptionalCardLink>
  );
}
