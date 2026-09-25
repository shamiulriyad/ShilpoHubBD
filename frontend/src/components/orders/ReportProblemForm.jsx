import { useState } from 'react';
import { Button } from '../ui';
import MutationFeedback from '../ui/MutationFeedback';
import ImageAttachButton, { resolveUploadUrl } from '../messaging/ImageAttachButton';
import { useComplaintMutations } from '../../hooks/useOrderComplaints';

// "Report a problem" for one delivered order item.
export default function ReportProblemForm({ orderId, productId, productName }) {
  const { create } = useComplaintMutations();
  const [open, setOpen] = useState(false);
  const [subject, setSubject] = useState('');
  const [description, setDescription] = useState('');
  const [image, setImage] = useState('');

  const submit = (e) => {
    e.preventDefault();
    if (!subject.trim() || !description.trim()) return;
    create.mutate({ orderId, productId, subject: subject.trim(), description: description.trim(), imageUrl: image || undefined }, {
      onSuccess: () => { setOpen(false); setSubject(''); setDescription(''); setImage(''); },
    });
  };

  if (!open) {
    return (
      <div>
        <button type="button" onClick={() => setOpen(true)} className="text-xs font-medium text-danger hover:underline">Report a problem</button>
        <MutationFeedback mutation={create} successMessage="Complaint sent to the producer. Follow it under My Complaints." />
      </div>
    );
  }

  return (
    <form onSubmit={submit} className="mt-2 w-full space-y-2 rounded-lg border border-border bg-background p-3">
      <p className="text-xs font-semibold text-heading">Problem with {productName}</p>
      <input aria-label="Subject" required maxLength={200} placeholder="What is wrong? (short)" value={subject} onChange={(e) => setSubject(e.target.value)} className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm" />
      <textarea aria-label="Details" required rows={3} maxLength={2000} placeholder="Describe the problem" value={description} onChange={(e) => setDescription(e.target.value)} className="w-full rounded-md border border-border bg-surface px-3 py-2 text-sm" />
      {image && (
        <div className="flex items-center gap-2">
          <img src={resolveUploadUrl(image)} alt="Attached" className="h-14 w-14 rounded-md object-cover" />
          <button type="button" onClick={() => setImage('')} className="text-xs text-danger hover:underline">Remove picture</button>
        </div>
      )}
      <div className="flex flex-wrap items-center gap-2">
        <ImageAttachButton onUploaded={setImage} label="Attach a photo of the problem" />
        <Button type="submit" variant="primary" size="sm" disabled={create.isPending || !subject.trim() || !description.trim()}>{create.isPending ? 'Sending…' : 'Send complaint'}</Button>
        <Button type="button" variant="secondary" size="sm" onClick={() => setOpen(false)}>Cancel</Button>
      </div>
      <MutationFeedback mutation={create} successMessage="Sent." />
    </form>
  );
}
