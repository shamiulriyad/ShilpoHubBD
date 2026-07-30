import RoleOverview from '../../components/shared/RoleOverview';

export default function IndustryGarmentsPage() {
  return (
    <RoleOverview
      title="Industry & Garments Dashboard"
      description="Partner with heritage textile producers for sourcing and collaboration."
      stats={[
        { label: 'Active Partnerships', value: '11' },
        { label: 'Pipeline Orders', value: '5' },
        { label: 'Certified Suppliers', value: '18' },
        { label: 'Compliance Score', value: '96%' },
      ]}
      highlights={[
        { title: 'Supplier Network', description: 'Certified heritage textile producers' },
        { title: 'Compliance Reports', description: 'Sourcing and sustainability reports' },
      ]}
    />
  );
}
