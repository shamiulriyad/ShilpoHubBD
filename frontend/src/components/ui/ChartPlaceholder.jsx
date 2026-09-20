import AnalyticsChart from './AnalyticsChart';

export default function ChartPlaceholder({ title, type = 'bar' }) {
  return <AnalyticsChart title={title} type={type === 'donut' ? 'bar' : type} data={[]} />;
}
