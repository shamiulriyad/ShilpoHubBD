import SafeImage from '../../components/media/SafeImage';
import { resolveMediaUrl } from '../../components/media/CardMedia';
import { useState } from 'react';
import { Action, DataTable, Editor, ErrorNotice, Modal, Panel, RecordDetails, useAdminAction, useAdminQuery, inputClass } from './AdminUI';
export default function AdminMarketplace({
  view
}) {
  const refunds = view === 'refunds',
    approval = view === 'approval';
  const [page, setPage] = useState(1),
    [status, setStatus] = useState(''),
    [term, setTerm] = useState(''),
    [search, setSearch] = useState(''),
    [selected, setSelected] = useState(null),
    [decision, setDecision] = useState(null);
  const path = refunds ? '/payments' : approval ? '/products/pending-approval' : '/products';
  const query = useAdminQuery(path, {
    page,
    pageSize: 20,
    ...(!approval ? {
      search: term || undefined
    } : {}),
    ...(refunds ? {
      status: status || undefined
    } : {})
  });
  const detail = useAdminQuery(`${refunds ? '/payments' : '/products'}/${selected?.id}`, {}, Boolean(selected));
  const mutation = useAdminAction();
  const start = (label, method, path, fields, payload) => {
    mutation.reset();
    setDecision({
      label,
      method,
      path,
      fields,
      payload
    });
  };
  return <Panel><div className="mb-5 flex flex-wrap justify-between gap-3">{!approval && <form className="flex flex-wrap gap-2" onSubmit={e => {
        e.preventDefault();
        setTerm(search);
        setPage(1);
      }}><input aria-label="Search records" className={`${inputClass} max-w-sm`} placeholder={refunds ? 'Order number or recipient' : 'Search products'} value={search} onChange={e => setSearch(e.target.value)} /><button className="rounded-lg bg-primary px-4 py-2 text-sm text-white">Search</button>{refunds && <select aria-label="Payment status" className={`${inputClass} max-w-xs`} value={status} onChange={e => {
          setStatus(e.target.value);
          setPage(1);
        }}><option value="">All payment statuses</option>{['Pending', 'Awaiting', 'Paid', 'Failed', 'Refunded', 'PartiallyRefunded'].map(s => <option key={s}>{s}</option>)}</select>}</form>}<Action disabled={query.isFetching} onClick={() => query.refetch()}>Refresh</Action></div>
    <DataTable query={query} page={page} onPage={setPage} columns={refunds ? ['orderNumber', 'recipientName', 'amount', 'refundedAmount', 'status'] : ['name', 'producerName', 'categoryName', 'price', 'approvalStatus', 'isFeatured']} actions={row => <Action onClick={() => setSelected(row)}>Review</Action>} />
    {selected && <Modal title={refunds ? 'Payment review' : 'Product review'} onClose={() => setSelected(null)}><ErrorNotice error={detail.error} />{detail.isPending ? <p>Loading details…</p> : detail.isSuccess && <><RecordDetails record={detail.data} />{detail.data.imageUrls?.length > 0 && <div className="mt-4 flex flex-wrap gap-3">{detail.data.imageUrls.map(url => <SafeImage key={url} src={resolveMediaUrl(url)} alt={detail.data.name} className="h-36 w-36 rounded-lg object-cover" />)}</div>}<div className="mt-6 flex flex-wrap gap-3">
    {refunds ? ['Paid', 'PartiallyRefunded'].includes(detail.data.status) && <Action danger onClick={() => start('Issue refund', 'post', `/payments/${selected.id}/refund`, [{
            key: 'amount',
            label: 'Refund amount (BDT)',
            type: 'number',
            required: true,
            min: 0.01,
            max: detail.data.amount - detail.data.refundedAmount,
            default: detail.data.amount - detail.data.refundedAmount
          }, {
            key: 'reason',
            label: 'Refund reason',
            type: 'textarea',
            required: true,
            maxLength: 1000
          }])}>Refund payment</Action> : <>
      <Action onClick={() => start('Approve product', 'patch', `/products/${selected.id}/approval`, [], {
              status: 'Approved'
            })}>Approve product</Action>
      <Action danger onClick={() => start('Reject product', 'patch', `/products/${selected.id}/approval`, [{
              key: 'rejectionReason',
              label: 'Rejection reason',
              type: 'textarea',
              required: true,
              maxLength: 1000
            }], {
              status: 'Rejected'
            })}>Reject product</Action>
      <Action onClick={() => start(detail.data.isFeatured ? 'Remove feature' : 'Feature product', 'patch', `/products/${selected.id}/featured`, [], {
              isFeatured: !detail.data.isFeatured
            })}>{detail.data.isFeatured ? 'Unfeature' : 'Feature'}</Action>
      <Action onClick={() => start('Handmade verification', 'patch', `/products/${selected.id}/handmade-verification`, [{
              key: 'status',
              label: 'Verification decision',
              required: true,
              options: ['Verified', 'Rejected']
            }, {
              key: 'notes',
              label: 'Notes',
              type: 'textarea',
              maxLength: 2000
            }])}>Verify handmade</Action>
    </>}</div></>}</Modal>}
    {decision && <Modal title={decision.label} onClose={() => !mutation.isPending && setDecision(null)}><p className="mb-4 text-sm">Review the details before confirming. {refunds ? 'The refund will be submitted to the payment provider.' : ''}</p><Editor fields={decision.fields} pending={mutation.isPending} error={mutation.error} submitLabel={`Confirm: ${decision.label}`} onSubmit={body => mutation.mutate({
        method: decision.method,
        path: decision.path,
        payload: {
          ...decision.payload,
          ...body
        }
      }, {
        onSuccess: () => {
          setDecision(null);
          setSelected(null);
        }
      })} /></Modal>}
  </Panel>;
}
