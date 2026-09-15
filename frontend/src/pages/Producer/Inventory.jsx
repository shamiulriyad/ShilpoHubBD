import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useLowStockProducts, useInventoryHistory, useAdjustStock } from '../../hooks/useInventory';
import { useMyProducts } from '../../hooks/useProducts';
import MutationFeedback from '../../components/ui/MutationFeedback';
import NewProductForm from './NewProductForm';

export default function Inventory() {
  const lowStockQuery = useLowStockProducts();
  const productsQuery = useMyProducts();
  const [selectedProductId, setSelectedProductId] = useState('');
  const [changeAmount, setChangeAmount] = useState('');
  const [reason, setReason] = useState('');
  const adjustStock = useAdjustStock();
  const historyQuery = useInventoryHistory(selectedProductId);
  const [showCreate, setShowCreate] = useState(false);
  const [createdMessage, setCreatedMessage] = useState('');
  const selectedProduct = productsQuery.data?.find((product) => product.id === selectedProductId);
  const amount = Number(changeAmount);
  const validAdjustment = Boolean(selectedProduct) && Number.isInteger(amount) && amount !== 0 && selectedProduct.stock + amount >= 0 && reason.trim().length > 0;

  const handleAdjust = (event) => {
    event.preventDefault();
    if (!validAdjustment || adjustStock.isPending) return;
    adjustStock.mutate(
      { productId: selectedProductId, payload: { changeAmount: Number(changeAmount), reason } },
      { onSuccess: () => { setChangeAmount(''); setReason(''); } },
    );
  };

  return (
    <div>
      <PageHeader title="Inventory" description="Manage your products, stock levels and transaction history." action={<Button type="button" onClick={() => setShowCreate(true)}>Add product</Button>} />
      {createdMessage && <p role="status" className="mb-4 text-sm text-success">{createdMessage}</p>}
      {showCreate && <NewProductForm onCancel={() => setShowCreate(false)} onCreated={(product) => { setShowCreate(false); setSelectedProductId(product.id); setCreatedMessage(`${product.name} was added to your inventory.`); }} />}

      <div className="mb-8 rounded-xl border border-border bg-surface p-5">
        <p className="mb-3 text-sm font-semibold text-heading">Low Stock Alerts</p>
        <AsyncState isLoading={lowStockQuery.isLoading} isError={lowStockQuery.isError} error={lowStockQuery.error}>
          <div className="divide-y divide-border">
            {(lowStockQuery.data || []).map((product) => (
              <div key={product.id} className="flex items-center justify-between py-2 text-sm">
                <span>{product.name}</span>
                <Badge tone="secondary">{product.stock} left (threshold {product.lowStockThreshold})</Badge>
              </div>
            ))}
            {(lowStockQuery.data || []).length === 0 && <p className="text-sm text-body/60">No low-stock items right now.</p>}
          </div>
        </AsyncState>
      </div>

      <AsyncState isLoading={productsQuery.isLoading} isError={productsQuery.isError} error={productsQuery.error}>
      {productsQuery.data?.length === 0 ? <div className="mb-8 rounded-xl border border-dashed border-border bg-surface p-8 text-center"><h2 className="text-lg font-semibold">Your inventory is ready for its first product</h2><p className="mt-2 text-sm text-muted">Add a product to set its opening stock and start selling.</p><Button type="button" className="mt-4" onClick={() => setShowCreate(true)}>Add your first product</Button></div> : <form onSubmit={handleAdjust} className="mb-8 space-y-3 rounded-xl border border-border bg-surface p-5">
        <p className="text-sm font-semibold text-heading">Adjust Stock</p>
        <select aria-label="Selected Product Id"
          required
          value={selectedProductId}
          onChange={(event) => setSelectedProductId(event.target.value)}
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Select a product…</option>
          {(productsQuery.data || []).map((product) => (
            <option key={product.id} value={product.id}>{product.name} (stock: {product.stock})</option>
          ))}
        </select>
        <div className="grid gap-3 sm:grid-cols-2">
          <input aria-label="Change amount"
            required
            type="number"
            step="1"
            placeholder="Change amount (+/-)"
            value={changeAmount}
            onChange={(event) => setChangeAmount(event.target.value)}
            className="rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <input aria-label="Reason"
            required
            maxLength={500}
            placeholder="Reason"
            value={reason}
            onChange={(event) => setReason(event.target.value)}
            className="rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <MutationFeedback mutation={adjustStock} successMessage="Stock updated." />
        {selectedProduct && <p className="text-sm text-muted">Current stock: {selectedProduct.stock}. {changeAmount && `After adjustment: ${selectedProduct.stock + amount}.`}</p>}
        <Button type="submit" variant="primary" disabled={adjustStock.isPending || !validAdjustment}>
          {adjustStock.isPending ? 'Adjusting…' : 'Adjust Stock'}
        </Button>
      </form>}
      </AsyncState>

      {selectedProductId && (
        <div>
          <p className="mb-3 text-sm font-semibold text-heading">Transaction History</p>
          <AsyncState isLoading={historyQuery.isLoading} isError={historyQuery.isError} error={historyQuery.error}>
            <div className="divide-y divide-border rounded-xl border border-border bg-surface">
              {(historyQuery.data || []).map((tx) => (
                <div key={tx.id} className="flex items-center justify-between p-3 text-sm">
                  <span>{tx.reason}</span>
                  <span className={tx.changeAmount >= 0 ? 'text-success' : 'text-red-600'}>
                    {tx.changeAmount >= 0 ? '+' : ''}{tx.changeAmount} ({tx.previousStock} → {tx.newStock})
                  </span>
                </div>
              ))}
              {(historyQuery.data || []).length === 0 && <p className="p-3 text-sm text-body/60">No history yet.</p>}
            </div>
          </AsyncState>
        </div>
      )}
    </div>
  );
}
