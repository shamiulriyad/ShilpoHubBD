import RoleOverview from '../../components/shared/RoleOverview';

export default function ArtisanPage() {
  return (
    <RoleOverview
      title="Artisan Dashboard"
      description="Manage your craft listings, orders and learning progress."
      stats={[
        { label: 'Active Listings', value: '12' },
        { label: 'Orders This Month', value: '34' },
        { label: 'Course Progress', value: '2 / 5' },
        { label: 'Rating', value: '4.8 / 5' },
      ]}
      highlights={[
        { title: 'Marketplace Listings', description: 'Manage the products you sell' },
        { title: 'Academy Courses', description: 'Continue your craft certification path' },
      ]}
    />
  );
}
