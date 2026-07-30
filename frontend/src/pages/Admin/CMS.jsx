import { PageHeader, Table, Button, Badge } from '../../components/ui';
import { newsItems } from '../../data/mockData';

export default function CMS() {
  return (
    <div>
      <PageHeader
        title="Content Management"
        description="Manage news, homepage sections and featured content."
        action={<Button variant="primary">New Article</Button>}
      />
      <Table
        columns={['title', 'category', 'date', 'status']}
        rows={newsItems.map((item) => ({
          title: item.title,
          category: item.category,
          date: item.date,
          status: <Badge tone="success">Published</Badge>,
        }))}
      />
    </div>
  );
}
