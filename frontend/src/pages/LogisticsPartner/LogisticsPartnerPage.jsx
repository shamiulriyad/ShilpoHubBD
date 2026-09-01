import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useLogisticsDashboardStats } from '../../hooks/useLogisticsDashboardStats';

const verificationTone = { Verified: 'success', Pending: 'secondary', Rejected: 'neutral', Suspended: 'neutral' };

export default function LogisticsPartnerPage() {
  const { profile, stats } = useLogisticsDashboardStats();

  return (
    <div>
      <PageHeader
        title="Logistics Partner Dashboard"
        description="Manage deliveries, routes and fulfillment for ShilpoHub orders."
        action={profile.data && (
          <Badge tone={verificationTone[profile.data.verificationStatus] || 'neutral'}>
            {profile.data.verificationStatus}
          </Badge>
        )}
      />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-5">
        <DashboardCard title="Warehouses" description="Storage facilities">
          <p className="text-2xl font-semibold text-primary">{stats.warehouseCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="In Transit" description="Active shipments">
          <p className="text-2xl font-semibold text-primary">{stats.activeShipmentCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Pending Pickups" description="Awaiting scheduling">
          <p className="text-2xl font-semibold text-primary">{stats.pendingPickupCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Open Returns" description="Awaiting approval">
          <p className="text-2xl font-semibold text-primary">{stats.openReturnCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Active Routes" description="Currently in progress">
          <p className="text-2xl font-semibold text-primary">{stats.activeRouteCount ?? '—'}</p>
        </DashboardCard>
      </div>

      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {[
          { title: 'Company Profile', to: routePaths.logisticsPartnerProfile },
          { title: 'Warehouses', to: routePaths.logisticsPartnerWarehouses },
          { title: 'Warehouse Stock', to: routePaths.logisticsPartnerStock },
          { title: 'Pickup Requests', to: routePaths.logisticsPartnerPickups },
          { title: 'Shipments', to: routePaths.logisticsPartnerShipments },
          { title: 'Returns', to: routePaths.logisticsPartnerReturns },
          { title: 'Delivery Routes', to: routePaths.logisticsPartnerRoutes },
          { title: 'AI Logistics Tools', to: routePaths.logisticsPartnerAiTools },
        ].map((item) => (
          <Link key={item.to} to={item.to} className="rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md">
            {item.title} →
          </Link>
        ))}
      </div>
    </div>
  );
}
