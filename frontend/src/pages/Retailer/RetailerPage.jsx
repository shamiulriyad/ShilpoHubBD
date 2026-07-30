import RoleOverview from '../../components/shared/RoleOverview';

export default function RetailerPage() {
  return (
    <RoleOverview
      title="Retailer Dashboard"
      description="Source heritage products for your storefront and manage bulk orders."
      stats={[
        { label: 'Bulk Orders', value: '16' },
        { label: 'Linked Producers', value: '22' },
        { label: 'Monthly Volume', value: '৳ 4.2L' },
        { label: 'Active Contracts', value: '9' },
      ]}
      highlights={[
        { title: 'Sourcing', description: 'Find producers for bulk sourcing' },
        { title: 'Order History', description: 'Track bulk purchase orders' },
      ]}
    />
  );
}
