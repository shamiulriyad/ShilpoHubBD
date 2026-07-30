import RoleOverview from '../../components/shared/RoleOverview';

export default function NGOPage() {
  return (
    <RoleOverview
      title="NGO Dashboard"
      description="Track community programs and producer support initiatives."
      stats={[
        { label: 'Active Projects', value: '6' },
        { label: 'Communities Supported', value: '32' },
        { label: 'Beneficiaries', value: '2,150' },
        { label: 'Funding Utilized', value: '78%' },
      ]}
      highlights={[
        { title: 'Community Programs', description: 'Manage ongoing support initiatives' },
        { title: 'Impact Reports', description: 'Track outcomes across districts' },
      ]}
    />
  );
}
