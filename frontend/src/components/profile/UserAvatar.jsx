import { useAuth } from '../../hooks/useAuth';
import { useMyProfile } from '../../hooks/useProfile';
import { resolveMediaUrl } from '../media/CardMedia';

// The signed-in member's photo, or their initial until they add one. `className` supplies the size and shape
// (profile-avatar in the navbar, sb-avatar in the sidebar, ...).
export default function UserAvatar({ className = '', name: nameOverride, photoUrl: photoOverride }) {
  const { user } = useAuth();
  const { data } = useMyProfile();
  const name = nameOverride ?? user?.name ?? 'U';
  const src = resolveMediaUrl(photoOverride ?? data?.photoUrl);
  return (
    <span className={className} aria-hidden="true">
      {src ? <img src={src} alt="" className="h-full w-full object-cover" style={{ borderRadius: 'inherit' }} /> : (name || 'U').trim().slice(0, 1).toUpperCase()}
    </span>
  );
}
