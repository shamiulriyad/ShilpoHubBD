import FilterPanel from './FilterPanel';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';

const priceRanges = [
  { label: 'Under ৳1,000', value: 'under-1000' },
  { label: '৳1,000 - ৳3,000', value: '1000-3000' },
  { label: '৳3,000 - ৳6,000', value: '3000-6000' },
  { label: 'Above ৳6,000', value: 'above-6000' },
];

export function priceRangeToQuery(value) {
  switch (value) {
    case 'under-1000':
      return { maxPrice: 999.99 };
    case '1000-3000':
      return { minPrice: 1000, maxPrice: 3000 };
    case '3000-6000':
      return { minPrice: 3000, maxPrice: 6000 };
    case 'above-6000':
      return { minPrice: 6000.01 };
    default:
      return {};
  }
}

export default function MarketplaceFilter({ values = {}, onChange, onClear, className = '' }) {
  const { data: categories } = useCategories();
  const { data: districts } = useDistricts();

  const groups = [
    {
      key: 'categoryId',
      label: 'Category',
      options: (categories || []).map((category) => ({ label: category.name, value: category.id })),
    },
    { key: 'priceRange', label: 'Price Range', options: priceRanges },
    {
      key: 'districtId',
      label: 'District',
      options: (districts || []).map((district) => ({ label: district.name, value: district.id })),
    },
  ];

  return <FilterPanel groups={groups} values={values} onChange={onChange} onClear={onClear} className={className} />;
}
