import FilterPanel from './FilterPanel';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';

const priceRanges = ['Under ৳1,000', '৳1,000 - ৳3,000', '৳3,000 - ৳6,000', 'Above ৳6,000'];

export default function MarketplaceFilter({ className = '' }) {
  const { data: categories } = useCategories();
  const { data: districts } = useDistricts();

  const groups = [
    { label: 'Category', options: (categories || []).map((c) => c.name) },
    { label: 'Price Range', options: priceRanges },
    { label: 'District', options: (districts || []).map((d) => d.name) },
  ];

  return <FilterPanel groups={groups} className={className} />;
}
