import { routePaths } from '../../routes/routePaths';
import { PageHeader, AsyncState } from '../../components/ui';
import { ImpactCard } from '../../components/cards';
import { useMyImpact } from '../../hooks/useImpact';

export default function ImpactDashboard() {
  const { data, isLoading, isError, error } = useMyImpact();

  const stats = data
    ? [
        { label: 'Heritage Score', value: data.heritageScore, description: 'Your overall contribution score' },
        { label: 'Families Supported', value: data.familiesSupported, description: 'Producer households you\'ve bought from' },
        { label: 'Districts Reached', value: data.distinctDistrictsSupported, description: 'Districts your purchases touched' },
        { label: 'Categories Explored', value: data.distinctCategoriesSupported, description: 'Different craft categories purchased' },
        { label: 'Items Purchased', value: data.totalItemsPurchased, description: 'Total heritage items bought' },
        {
          label: 'Est. CO₂ Savings',
          value: `${data.estimatedCo2SavingsKg.toLocaleString()} kg`,
          description: 'Rough estimate vs. mass-produced equivalents',
        },
      ]
    : [];

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Impact Dashboard' }]}
        title="Your Impact"
        description="See how your purchases directly support Bangladesh's heritage artisans and villages."
      />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
          {stats.map((stat) => (
            <ImpactCard key={stat.label} label={stat.label} value={stat.value} description={stat.description} />
          ))}
        </div>
      </AsyncState>
    </div>
  );
}
