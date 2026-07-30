import RoleOverview from '../../components/shared/RoleOverview';

export default function LogisticsPartnerPage() {
  return (
    <RoleOverview
      title="Logistics Partner Dashboard"
      description="Manage deliveries, routes and fulfillment for ShilpoHub orders."
      stats={[
        { label: 'Active Deliveries', value: '42' },
        { label: 'On-Time Rate', value: '97%' },
        { label: 'Coverage Districts', value: '28' },
        { label: 'Fleet Size', value: '15' },
      ]}
      highlights={[
        { title: 'Delivery Queue', description: 'Manage pending and active deliveries' },
        { title: 'Route Planning', description: 'Optimize delivery routes by district' },
      ]}
    />
  );
}
