import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';
import { productsService } from '../../services/productsService';
import { Button, QueryStatusBanner } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';

export default function NewProductForm({ onCreated, onCancel }) {
  const categories = useCategories();
  const districts = useDistricts();
  const queryClient = useQueryClient();
  const [form, setForm] = useState({ name: '', description: '', price: '', stock: '0', categoryId: '', districtId: '', imageUrl: '', lowStockThreshold: '5' });
  const create = useMutation({ mutationFn: productsService.create, onSuccess: (product) => {
    queryClient.invalidateQueries({ queryKey: ['products'] });
    queryClient.invalidateQueries({ queryKey: ['inventory'] });
    onCreated(product);
  } });
  const field = (key) => ({ value: form[key], onChange: (event) => setForm((previous) => ({ ...previous, [key]: event.target.value })) });
  const inputClass = 'mt-1 block w-full rounded-lg border border-border bg-background px-3 py-2 text-sm';
  return <form className="mb-6 space-y-4 rounded-xl border border-border bg-surface p-5" onSubmit={(event) => {
    event.preventDefault();
    if (create.isPending) return;
    const { imageUrl, ...payload } = form;
    create.mutate({ ...payload, name: form.name.trim(), description: form.description.trim(), price: Number(form.price), stock: Number(form.stock), lowStockThreshold: Number(form.lowStockThreshold), imageUrls: [imageUrl.trim()] });
  }}>
    <h2 className="text-lg font-semibold">Add a product</h2>
    <p className="text-sm text-muted">Publish a product to the marketplace and manage its stock here.</p>
    <QueryStatusBanner queries={[categories, districts]} />
    <div className="grid gap-4 sm:grid-cols-2">
      <label className="text-sm">Product name<input required maxLength={200} {...field('name')} className={inputClass} /></label>
      <label className="text-sm">Price (৳)<input required type="number" min="0.01" step="0.01" {...field('price')} className={inputClass} /></label>
      <label className="text-sm">Category<select required {...field('categoryId')} className={inputClass}><option value="">Choose category</option>{(categories.data || []).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label className="text-sm">District<select required {...field('districtId')} className={inputClass}><option value="">Choose district</option>{(districts.data || []).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label className="text-sm">Opening stock<input required type="number" min="0" step="1" {...field('stock')} className={inputClass} /></label>
      <label className="text-sm">Low stock alert at<input required type="number" min="0" step="1" {...field('lowStockThreshold')} className={inputClass} /></label>
      <label className="text-sm sm:col-span-2">Product photo URL<input required type="url" maxLength={2000} placeholder="https://…" {...field('imageUrl')} className={inputClass} /></label>
      <label className="text-sm sm:col-span-2">Description<textarea required maxLength={4000} rows={3} {...field('description')} className={inputClass} /></label>
    </div>
    <MutationFeedback mutation={create} />
    <div className="flex gap-3"><Button type="submit" disabled={create.isPending || !categories.data?.length || !districts.data?.length}>{create.isPending ? 'Saving…' : 'Publish product'}</Button><Button type="button" variant="secondary" onClick={onCancel} disabled={create.isPending}>Cancel</Button></div>
  </form>;
}
