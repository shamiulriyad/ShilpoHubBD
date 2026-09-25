import { Link } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useMyProfile } from '../../hooks/useProfile';
import { routePaths } from '../../routes/routePaths';

// Nudges every member (except admins) to finish and get their profile approved.
export default function ProfileStatusBanner() {
  const { activeRole } = useAuth();
  const { data } = useMyProfile();
  if (!data || activeRole === 'SuperAdmin' || data.status === 'Approved') return null;

  const messages = {
    NotSubmitted: 'Complete your profile with your NID number so an admin can approve it.',
    Pending: 'Your profile is waiting for admin approval.',
    Rejected: 'Your profile needs changes' + (data.reviewNotes ? ': ' + data.reviewNotes : '.'),
  };

  return (
    <div role="status" className="mb-4 flex flex-wrap items-center justify-between gap-2 rounded-lg border border-secondary/40 bg-secondary/10 px-4 py-2 text-sm text-body">
      <span>{messages[data.status] || messages.NotSubmitted}</span>
      {data.status !== 'Pending' && <Link to={routePaths.dashboardProfile} className="font-semibold text-primary hover:underline">Open profile</Link>}
    </div>
  );
}
