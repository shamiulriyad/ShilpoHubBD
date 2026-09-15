import { AnalyticsChart, AsyncState } from '../../components/ui';
import { useProducerSales, useProducerRevenue } from '../../hooks/useProducerOrders';

export default function ProducerInsights() {
  const sales = useProducerSales();
  const revenue = useProducerRevenue();
  const daily = [...(sales.data?.dailySales || [])].sort((a,b)=>new Date(a.date)-new Date(b.date)).slice(-30).map(day=>({label:new Date(day.date).toLocaleDateString(undefined,{month:'short',day:'numeric'}),value:day.revenue}));
  const statuses = ['Pending','Accepted','Processing','Shipped','Delivered','Rejected','Cancelled'].map(label=>({label,value:revenue.data?.[`${label[0].toLowerCase()}${label.slice(1)}Count`]}));
  return <div className="my-8 grid items-start gap-5 xl:grid-cols-[1.5fr_1fr]">
    <AsyncState isLoading={sales.isLoading} isError={sales.isError} error={sales.error}><AnalyticsChart title="Revenue · recent sales days" type="line" data={daily} valueFormatter={value=>`৳ ${value.toLocaleString()}`} /></AsyncState>
    <AsyncState isLoading={revenue.isLoading} isError={revenue.isError} error={revenue.error}><AnalyticsChart title="Order fulfillment" data={statuses} /></AsyncState>
  </div>;
}
