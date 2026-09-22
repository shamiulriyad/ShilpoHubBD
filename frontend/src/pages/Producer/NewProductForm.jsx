import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useCategories } from '../../hooks/useCategories';
import { useDistricts } from '../../hooks/useDistricts';
import { productsService } from '../../services/productsService';
import { Button, QueryStatusBanner } from '../../components/ui';
import MutationFeedback from '../../components/ui/MutationFeedback';
import SafeImage from '../../components/media/SafeImage';
import { resolveMediaUrl } from '../../components/media/CardMedia';

export default function NewProductForm({ initial, onCreated, onCancel }) {
  const categories = useCategories();
  const districts = useDistricts();
  const queryClient = useQueryClient();
  const [form, setForm] = useState({ name: initial?.name || '', description: initial?.description || '', price: initial?.price ?? '', discountPrice: initial?.discountPrice ?? '', stock: initial?.stock ?? '0', categoryId: initial?.categoryId || '', districtId: initial?.districtId || '', imageUrl: initial?.imageUrls?.[0] || '', lowStockThreshold: initial?.lowStockThreshold ?? '5', story: initial?.story || '', makingProcessVideoUrl: initial?.makingProcessVideoUrl || '' });
  const [uploadError, setUploadError] = useState('');
  const upload = useMutation({ mutationFn: productsService.uploadImage, onSuccess: ({ url }) => setForm(previous => ({ ...previous, imageUrl: url })) });
  const create = useMutation({ mutationFn: payload => initial?.id ? productsService.update(initial.id, payload) : productsService.create(payload), onSuccess: (product) => {
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
    create.mutate({ ...payload, name: form.name.trim(), description: form.description.trim(), price: Number(form.price), discountPrice: form.discountPrice === '' ? null : Number(form.discountPrice), stock: Number(form.stock), lowStockThreshold: Number(form.lowStockThreshold), imageUrls: [imageUrl.trim()], threeSixtyImageUrls: initial?.threeSixtyImageUrls || [], story: form.story.trim() || null, makingProcessVideoUrl: form.makingProcessVideoUrl.trim() || null, isActive: initial?.isActive ?? true });
  }}>
    <h2 className="text-lg font-semibold">{initial ? 'Edit product' : 'Add a product'}</h2>
    <p className="text-sm text-muted">Add complete product information. New products and producer edits remain pending until an admin approves them.</p>
    <QueryStatusBanner queries={[categories, districts]} />
    <div className="grid gap-4 sm:grid-cols-2">
      <label className="text-sm">Product name<input required maxLength={200} {...field('name')} className={inputClass} /></label>
      <label className="text-sm">Price (৳)<input required type="number" min="0.01" step="0.01" {...field('price')} className={inputClass} /></label>
      <label className="text-sm">Discount price (optional)<input type="number" min="0.01" step="0.01" {...field('discountPrice')} className={inputClass} /></label>
      <label className="text-sm">Category<select required {...field('categoryId')} className={inputClass}><option value="">Choose category</option>{(categories.data || []).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label className="text-sm">District<select required {...field('districtId')} className={inputClass}><option value="">Choose district</option>{(districts.data || []).map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label className="text-sm">Opening stock<input required type="number" min="0" step="1" {...field('stock')} className={inputClass} /></label>
      <label className="text-sm">Low stock alert at<input required type="number" min="0" step="1" {...field('lowStockThreshold')} className={inputClass} /></label>
      <div className="space-y-2 text-sm sm:col-span-2"><label htmlFor="product-photo" className="font-medium">Product photo *</label><input id="product-photo" required={!form.imageUrl} type="file" accept="image/jpeg,image/png,image/webp" className={inputClass} onChange={event => { const file = event.target.files?.[0]; setUploadError(''); if (!file) return; if (file.size > 20 * 1024 * 1024) { setUploadError('Choose an image smaller than 20 MB.'); return; } upload.mutate(file); }} /><p className="text-xs text-muted">JPG, PNG or WebP, up to 20 MB. {upload.isPending ? 'Uploading…' : form.imageUrl ? 'Photo uploaded.' : ''}</p>{(uploadError || upload.isError) && <p role="alert" className="text-red-700">{uploadError || upload.error?.response?.data?.message || 'The photo could not be uploaded. Try again.'}</p>}{form.imageUrl && <SafeImage src={resolveMediaUrl(form.imageUrl)} alt="Product preview" className="h-40 w-40 rounded-xl border border-border object-cover" />}</div>
      <label className="text-sm sm:col-span-2">Description<textarea required maxLength={4000} rows={3} {...field('description')} className={inputClass} /></label>
      <label className="text-sm sm:col-span-2">Product story (optional)<textarea maxLength={4000} rows={3} {...field('story')} className={inputClass} /></label>
      <label className="text-sm sm:col-span-2">Making process video URL (optional)<input type="url" maxLength={2000} {...field('makingProcessVideoUrl')} className={inputClass} /></label>
    </div>
    <MutationFeedback mutation={create} />
    <div className="flex gap-3"><Button type="submit" disabled={create.isPending || upload.isPending || !form.imageUrl || !categories.data?.length || !districts.data?.length}>{create.isPending ? 'Saving…' : initial ? 'Save and submit for review' : 'Submit product for approval'}</Button><Button type="button" variant="secondary" onClick={onCancel} disabled={create.isPending}>Cancel</Button></div>
  </form>;
}
