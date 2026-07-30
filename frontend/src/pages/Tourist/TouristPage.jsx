import RoleOverview from '../../components/shared/RoleOverview';

export default function TouristPage() {
  return (
    <RoleOverview
      title="Tourist Dashboard"
      description="Plan heritage trips, track your travel passport and saved routes."
      stats={[
        { label: 'Sites Visited', value: '6' },
        { label: 'Saved Routes', value: '3' },
        { label: 'Upcoming Festivals', value: '2' },
        { label: 'Passport Badges', value: '4' },
      ]}
      highlights={[
        { title: 'Travel Passport', description: 'Track visited heritage sites' },
        { title: 'Tour Routes', description: 'Continue a saved travel itinerary' },
      ]}
    />
  );
}
