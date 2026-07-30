import RoleOverview from '../../components/shared/RoleOverview';

export default function FarmerPage() {
  return (
    <RoleOverview
      title="Farmer Dashboard"
      description="Manage produce listings and track seasonal demand."
      stats={[
        { label: 'Active Listings', value: '7' },
        { label: 'Orders This Month', value: '21' },
        { label: 'Cooperative Members', value: '48' },
        { label: 'Rating', value: '4.6 / 5' },
      ]}
      highlights={[
        { title: 'Produce Listings', description: 'Manage seasonal produce and goods' },
        { title: 'Cooperative Network', description: 'Connect with nearby farmer cooperatives' },
      ]}
    />
  );
}
