import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { StatCard } from '../../components/cards';
import { useBusinessPartnersList } from '../../hooks/useBusinessPartners';
import { useProducts } from '../../hooks/useProducts';

export default function AdminDashboard() {
  const partnersQuery = useBusinessPartnersList({ verificationStatus: 'Pending', pageSize: 1 });
  const productsQuery = useProducts({ pageSize: 1 });

  return (
    <div>
      <PageHeader title="Admin Dashboard" description="Platform-wide overview and moderation queue." />
      <div className="mb-8 grid grid-cols-2 gap-4 lg:grid-cols-4">
        <StatCard label="Total Listings" value={productsQuery.data?.totalCount ?? '—'} />
        <StatCard label="Pending Partner Verifications" value={partnersQuery.data?.totalCount ?? '—'} />
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {[
          { title: 'User & Role Management', to: routePaths.adminUsers },
          { title: 'Heritage Management', to: routePaths.adminHeritage },
          { title: 'Marketplace Monitoring', to: routePaths.adminMarketplace },
          { title: 'CMS', to: routePaths.adminCms },
          { title: 'Security Center', to: routePaths.adminSecurity },
        ].map((item) => (
          <Link key={item.to} to={item.to} className="rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md">
            {item.title} →
          </Link>
        ))}
      </div>
    </div>
  );
}
