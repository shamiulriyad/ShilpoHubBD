import RoleOverview from '../../components/shared/RoleOverview';

export default function TrainerMasterArtisanPage() {
  return (
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
  );
}
