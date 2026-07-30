import RoleOverview from '../../components/shared/RoleOverview';

export default function GovernmentPage() {
  return (
    <RoleOverview
      title="Government Dashboard"
      description="Monitor national heritage programs and producer registrations."
      stats={[
        { label: 'Registered Producers', value: '12,400' },
        { label: 'Active Programs', value: '9' },
        { label: 'Districts Covered', value: '64' },
        { label: 'Grants Disbursed', value: '৳ 2.1Cr' },
      ]}
      highlights={[
        { title: 'Heritage Registry', description: 'Oversee national heritage records' },
        { title: 'Program Reports', description: 'Track program impact by district' },
      ]}
    />
  );
}
