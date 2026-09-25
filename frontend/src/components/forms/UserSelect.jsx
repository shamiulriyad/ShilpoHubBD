import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import apiClient from '../../services/apiClient';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';

// Pick a person by name: type two letters, choose from the matches. Hands the user id back.
export default function UserSelect({ value, onChange, placeholder = 'Search a person by name or email…', className = '' }) {
  const [term, setTerm] = useState('');
  const [debounced, setDebounced] = useState('');

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(term.trim()), 300);
    return () => clearTimeout(timer);
  }, [term]);

  const query = useQuery({
    queryKey: ['user-lookup', debounced],
    queryFn: () => apiClient.get('/users/lookup', { params: { search: debounced } }).then((res) => res.data),
    enabled: debounced.length >= 2,
    staleTime: 60_000,
  });
  const people = query.data || [];

  return (
    <div className={`flex min-w-0 flex-1 flex-wrap gap-2 ${className}`}>
      <input
        aria-label="Search people"
        placeholder={placeholder}
        value={term}
        onChange={(e) => { setTerm(e.target.value); onChange(''); }}
        className={`${inputClass} min-w-[12rem] flex-1`}
      />
      <select aria-label="Choose person" value={value} onChange={(e) => onChange(e.target.value)} className={`${inputClass} min-w-[12rem] flex-1`}>
        <option value="">
          {debounced.length < 2 ? 'Type at least 2 letters' : query.isLoading ? 'Searching…' : people.length ? 'Choose a person…' : 'No one found'}
        </option>
        {people.map((p) => <option key={p.id} value={p.id}>{p.fullName}{p.roles?.length ? ` (${p.roles.join(', ')})` : ''}</option>)}
      </select>
    </div>
  );
}
