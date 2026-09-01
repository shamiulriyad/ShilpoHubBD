import { Link } from 'react-router-dom';
import RoleOverview from '../../components/shared/RoleOverview';
import { routePaths } from '../../routes/routePaths';

export default function TrainerMasterArtisanPage() {
  return (
    <div>
      <RoleOverview
        title="Trainer / Master Artisan Dashboard"
        description="Manage the courses and apprentices you mentor."
        stats={[
          { label: 'Active Courses', value: '4' },
          { label: 'Students', value: '96' },
          { label: 'Certificates Issued', value: '58' },
          { label: 'Rating', value: '4.9 / 5' },
        ]}
        highlights={[
          { title: 'My Courses', description: 'Manage course content and cohorts' },
          { title: 'Apprentices', description: 'Track apprentice progress' },
        ]}
      />
      <Link
        to={routePaths.trainerMasterArtisanPrograms}
        className="mt-4 inline-block rounded-xl border border-border bg-surface p-4 text-sm font-medium text-heading transition hover:shadow-md"
      >
        Apprenticeship Programs →
      </Link>
    </div>
  );
}
