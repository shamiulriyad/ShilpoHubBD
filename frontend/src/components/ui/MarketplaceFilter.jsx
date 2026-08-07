import FilterPanel from './FilterPanel';
import { categories, districts, crafts } from '../../data/mockData';

const priceRanges = ['Under ৳1,000', '৳1,000 - ৳3,000', '৳3,000 - ৳6,000', 'Above ৳6,000'];

export default function MarketplaceFilter({ className = '' }) {
  const groups = [
    { label: 'Category', options: categories.map((c) => c.name) },
    { label: 'Price Range', options: priceRanges },
    { label: 'District', options: districts.map((d) => d.name) },
    { label: 'Craft', options: crafts.map((c) => c.name) },
  ];

  return <FilterPanel groups={groups} className={className} />;
}
