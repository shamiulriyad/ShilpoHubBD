import RoleOverview from '../../components/shared/RoleOverview';

export default function ResearcherPage() {
  return (
    <RoleOverview
      title="Researcher Dashboard"
      description="Manage research projects, publications and heritage datasets."
      stats={[
        { label: 'Active Projects', value: '3' },
        { label: 'Publications', value: '7' },
        { label: 'Datasets Accessed', value: '12' },
        { label: 'Citations', value: '148' },
      ]}
      highlights={[
        { title: 'Research Workspace', description: 'Continue your active research projects' },
        { title: 'Heritage Database', description: 'Explore open heritage datasets' },
      ]}
    />
  );
}
