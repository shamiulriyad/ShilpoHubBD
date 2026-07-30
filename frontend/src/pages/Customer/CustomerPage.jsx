import RoleOverview from '../../components/shared/RoleOverview';

export default function CustomerPage() {
  return (
    <RoleOverview
      title="Customer Dashboard"
      description="Track your orders, wishlist and favorite producers."
      stats={[
        { label: 'Orders Placed', value: '9' },
        { label: 'Wishlist Items', value: '14' },
        { label: 'Favorite Producers', value: '5' },
        { label: 'Reward Points', value: '320' },
      ]}
      highlights={[
        { title: 'My Orders', description: 'View order status and history' },
        { title: 'Wishlist', description: 'Products you have saved for later' },
      ]}
    />
  );
}
