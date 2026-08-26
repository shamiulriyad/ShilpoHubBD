import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader } from '../../components/ui';
import { DashboardCard } from '../../components/cards';
import { useProducerOrderItems, useProducerRevenue } from '../../hooks/useProducerOrders';
import { useLowStockProducts } from '../../hooks/useInventory';
import { useReceivedContracts } from '../../hooks/useContracts';
import { useReceivedQuotations } from '../../hooks/useQuotations';

export default function ProducerDashboard() {
  const pendingOrdersQuery = useProducerOrderItems({ status: 'Pending', pageSize: 1 });
  const revenueQuery = useProducerRevenue();
  const lowStockQuery = useLowStockProducts();
  const contractsQuery = useReceivedContracts({ status: 'PendingApproval', pageSize: 1 });
  const quotationsQuery = useReceivedQuotations({ pageSize: 1 });

  return (
    <div>
      <PageHeader title="Producer Dashboard" description="Manage your business operations, orders and partnerships." />

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <DashboardCard title="Pending Orders" description="Awaiting your response">
          <p className="text-2xl font-semibold text-primary">{pendingOrdersQuery.data?.totalCount ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Revenue" description="All time">
          <p className="text-2xl font-semibold text-primary">৳ {revenueQuery.data?.totalRevenue?.toLocaleString() ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Low Stock Items" description="Need restocking">
          <p className="text-2xl font-semibold text-primary">{lowStockQuery.data?.length ?? '—'}</p>
        </DashboardCard>
        <DashboardCard title="Pending Contracts" description="Awaiting your decision">
          <p className="text-2xl font-semibold text-primary">{contractsQuery.data?.totalCount ?? '—'}</p>
        </DashboardCard>
      </div>

      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {[
          { title: 'Orders & Fulfillment', to: routePaths.producerOrders },
          { title: 'Inventory', to: routePaths.producerInventory },
          { title: 'Contracts', to: routePaths.producerContracts },
          { title: 'Quotation Requests', to: routePaths.producerQuotations },
          { title: 'Manufacturing Partnerships', to: routePaths.producerPartnerships },
          { title: 'Design Collaborations', to: routePaths.producerDesignCollaborations },
          { title: 'Product Development', to: routePaths.producerProductDevelopment },
          { title: 'CSR Sponsorship', to: routePaths.producerCsr },
          { title: 'Investment Opportunities', to: routePaths.producerInvestments },
          { title: 'Sustainability Profile', to: routePaths.producerSustainability },
          { title: 'AI Business Assistant', to: routePaths.producerAiAssistant },
        ].map((item) => (
          <Link key={item.to} to={item.to} className="rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md">
            {item.title} →
          </Link>
        ))}
      </div>
      {quotationsQuery.data?.totalCount > 0 && (
        <p className="mt-6 text-sm text-body/60">
          You have {quotationsQuery.data.totalCount} quotation request{quotationsQuery.data.totalCount > 1 ? 's' : ''} waiting for a response.
        </p>
      )}
    </div>
  );
}
