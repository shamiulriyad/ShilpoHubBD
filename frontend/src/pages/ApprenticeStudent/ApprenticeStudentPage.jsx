import RoleOverview from '../../components/shared/RoleOverview';

export default function ApprenticeStudentPage() {
  return (
    <RoleOverview
      title="Apprentice / Student Dashboard"
      description="Track your enrolled courses, mentors and certifications."
      stats={[
        { label: 'Enrolled Courses', value: '3' },
        { label: 'Completed', value: '1' },
        { label: 'Certificates', value: '1' },
        { label: 'Mentor Sessions', value: '6' },
      ]}
      highlights={[
        { title: 'Learning Dashboard', description: 'Continue your active courses' },
        { title: 'Portfolio', description: 'Showcase your craft projects' },
      ]}
    />
  );
}
