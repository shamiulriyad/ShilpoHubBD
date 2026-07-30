import RoleOverview from '../../components/shared/RoleOverview';

export default function ExporterPage() {
  return (
    <RoleOverview
      title="Exporter Dashboard"
      description="Manage export orders, compliance and international partners."
      stats={[
        { label: 'Active Export Orders', value: '8' },
        { label: 'Partner Countries', value: '14' },
        { label: 'Compliance Score', value: '94%' },
        { label: 'Monthly Volume', value: '৳ 9.8L' },
      ]}
      highlights={[
        { title: 'Export Orders', description: 'Track shipments and documentation' },
        { title: 'Compliance Center', description: 'Certifications and trade compliance' },
      ]}
    />
  );
}
