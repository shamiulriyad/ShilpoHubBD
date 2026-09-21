import { Link, Outlet, useLocation } from 'react-router-dom';
import { logisticsPartnerSidebarNav } from '../../data/navigation';
import { useAuth } from '../../hooks/useAuth';
import { useMyLogisticsPartnerProfile } from '../../hooks/useLogisticsPartners';
import { routePaths } from '../../routes/routePaths';
import { getApiErrorMessage } from '../../utils/apiError';

export default function LogisticsWorkspaceGuard() {
  const { activeRole } = useAuth();
  const location = useLocation();
  const profile = useMyLogisticsPartnerProfile();
  const isProfilePage = location.pathname === routePaths.logisticsPartnerProfile;
  const section = logisticsPartnerSidebarNav.flatMap(group => group.items).find(item => item.path === location.pathname)?.label || 'Logistics workspace';

  // Super administrators can inspect operational screens without owning a partner profile.
  if (activeRole === 'SuperAdmin') return <Outlet />;

  // Onboarding must always remain usable, even while the profile lookup is slow or offline.
  if (isProfilePage) return <Outlet />;

  if (profile.isLoading) {
    return (
      <div>
        <div role="status" className="mb-4 rounded-xl border border-border bg-surface px-4 py-3 text-sm text-body/65">Checking your logistics profile…</div>
        <h1 className="text-3xl font-semibold text-heading">{section}</h1>
      </div>
    );
  }

  if (profile.isError && profile.error?.response?.status === 404) {
    return (
      <section>
        <h1 className="mb-6 text-3xl font-semibold text-heading">{section}</h1>
        <div className="rounded-2xl border border-border bg-surface p-6">
          <h2 className="text-lg font-semibold text-heading">Set up your company to get started</h2>
          <p className="mt-2 text-sm text-body/70">Your company details are needed before you can manage {section === 'Logistics Dashboard' ? 'logistics operations' : section.toLowerCase()}. You can browse the sidebar and explore heritage at any time.</p>
          <Link to={routePaths.logisticsPartnerProfile} state={{ from: location.pathname }} className="mt-5 inline-flex rounded-full bg-primary px-5 py-3 text-sm font-semibold text-white">Complete company profile →</Link>
        </div>
      </section>
    );
  }

  if (profile.isError) {
    return (
      <div role="alert" className="rounded-2xl border border-red-200 bg-red-50 p-6 text-sm text-red-800">
        <h1 className="mb-3 text-2xl font-semibold">{section}</h1>
        <p className="font-semibold">This section could not be loaded.</p>
        <p className="mt-1">{getApiErrorMessage(profile.error, 'Please try again shortly.')}</p>
        <button type="button" onClick={() => profile.refetch()} className="mt-4 rounded-lg bg-title px-4 py-2 font-semibold text-white">Try again</button>
      </div>
    );
  }

  return <Outlet />;
}
