import { Link } from 'react-router-dom';

export default function OptionalCardLink({ to, className, children }) {
  return to ? (
    <Link to={to} className={className}>
      {children}
    </Link>
  ) : (
    <div className={className}>{children}</div>
  );
}