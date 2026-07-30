import { routePaths } from '../../routes/routePaths';
import { PageHeader, Table, Button } from '../../components/ui';

const projects = [
  { name: 'Regional Craft Mapping', lead: 'Dr. Rahima Begum', status: 'In Progress', updated: '2 days ago' },
  { name: 'Digitizing Oral Heritage', lead: 'Dr. Abdul Karim', status: 'Review', updated: '5 days ago' },
  { name: 'Craft Tourism Impact Study', lead: 'Dr. Shefali Rani', status: 'Planning', updated: '1 week ago' },
];

export default function ResearchWorkspace() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Innovation Hub', path: routePaths.research },
          { label: 'Research Workspace' },
        ]}
        title="Research Workspace"
        description="Ongoing and proposed heritage research projects."
        action={<Button variant="primary">New Research Project</Button>}
      />
      <Table columns={['name', 'lead', 'status', 'updated']} rows={projects} />
    </div>
  );
}
