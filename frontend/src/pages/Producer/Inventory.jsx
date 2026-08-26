import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useLowStockProducts, useInventoryHistory, useAdjustStock } from '../../hooks/useInventory';
import { useMyProducts } from '../../hooks/useProducts';

export default function Inventory() {
  const lowStockQuery = useLowStockProducts();
  const productsQuery = useMyProducts();
  const [selectedProductId, setSelectedProductId] = useState('');
  const [changeAmount, setChangeAmount] = useState('');
  const [reason, setReason] = useState('');
  const adjustStock = useAdjustStock();
  const historyQuery = useInventoryHistory(selectedProductId);

  const handleAdjust = (event) => {
    event.preventDefault();
    adjustStock.mutate(
      { productId: selectedProductId, payload: { changeAmount: Number(changeAmount), reason } },
      { onSuccess: () => { setChangeAmount(''); setReason(''); } },
    );
  };

  return (
    <div>
      <PageHeader title="Inventory" description="Adjust stock levels and review transaction history." />

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

      <form onSubmit={handleAdjust} className="mb-8 space-y-3 rounded-xl border border-border bg-surface p-5">
        <p className="text-sm font-semibold text-heading">Adjust Stock</p>
        <select
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
          <input
            required
            type="number"
            placeholder="Change amount (+/-)"
            value={changeAmount}
            onChange={(event) => setChangeAmount(event.target.value)}
            className="rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
          <input
            required
            placeholder="Reason"
            value={reason}
            onChange={(event) => setReason(event.target.value)}
            className="rounded-md border border-border bg-background px-3 py-2 text-sm"
          />
        </div>
        <Button type="submit" variant="primary" disabled={adjustStock.isPending}>
          {adjustStock.isPending ? 'Adjusting…' : 'Adjust Stock'}
        </Button>
      </form>

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
