import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useSpendingAnalytics, useProcurementAnalytics } from '../../hooks/useBusinessPartnerAnalytics';
import { useMyContracts } from '../../hooks/useContracts';
import { useMyQuotations } from '../../hooks/useQuotations';

export default function BusinessPartnerDashboard() {
  const spendingQuery = useSpendingAnalytics();
  const procurementQuery = useProcurementAnalytics();
  const contractsQuery = useMyContracts({ status: 'Active', pageSize: 1 });
  const quotationsQuery = useMyQuotations({ pageSize: 1 });

  return (
    <div>
      <PageHeader title="Business Partner Dashboard" description="Manage supply relationships, procurement and analytics." />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <DashboardCard title="Total Spent" description="All time">
          <p className="text-2xl font-semibold text-primary">৳ {spendingQuery.data?.totalSpent?.toLocaleString() ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Active Contracts" description="Currently in force">
          <p className="text-2xl font-semibold text-primary">{contractsQuery.data?.totalCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Open Quotations" description="Awaiting responses">
          <p className="text-2xl font-semibold text-primary">{quotationsQuery.data?.totalCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Procurement Requests" description="All time">
          <p className="text-2xl font-semibold text-primary">{procurementQuery.data?.totalRequests ?? '—'}</p>
        </DashboardCard>
      </div>

      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {[
          { title: 'Company Profile', to: routePaths.businessPartnerProfile },
          { title: 'Contracts', to: routePaths.businessPartnerContracts },
          { title: 'Quotations', to: routePaths.businessPartnerQuotations },
          { title: 'Procurement', to: routePaths.businessPartnerProcurements },
          { title: 'Manufacturing Partnerships', to: routePaths.businessPartnerPartnerships },
          { title: 'Design Collaborations', to: routePaths.businessPartnerDesignCollaborations },
          { title: 'Product Development', to: routePaths.businessPartnerProductDevelopment },
          { title: 'Sponsorship Marketplace', to: routePaths.businessPartnerCsr },
          { title: 'Investment Marketplace', to: routePaths.businessPartnerInvestments },
          { title: 'Supplier Discovery', to: routePaths.businessPartnerSupplierDiscovery },
          { title: 'Supplier Matching (AI)', to: routePaths.businessPartnerSupplierMatching },
          { title: 'Compare Producers', to: routePaths.businessPartnerProducerComparison },
          { title: 'Analytics', to: routePaths.businessPartnerAnalytics },
          { title: 'AI Intelligence', to: routePaths.businessPartnerAiIntelligence },
        ].map((item) => (
          <Link key={item.to} to={item.to} className="rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md">
            {item.title} →
          </Link>
        ))}
      </div>
    </div>
  );
}
