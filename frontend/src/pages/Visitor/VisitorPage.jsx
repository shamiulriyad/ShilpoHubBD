import RoleOverview from '../../components/shared/RoleOverview';

export default function VisitorPage() {
  return (
    <RoleOverview
      title="Visitor"
      description="Browse ShilpoHub's public heritage content — no login required."
      highlights={[
        { title: 'Explore Heritage', description: 'Discover districts, villages and crafts' },
        { title: 'Marketplace', description: 'Browse authentic heritage products' },
      ]}
    />
  );
}
