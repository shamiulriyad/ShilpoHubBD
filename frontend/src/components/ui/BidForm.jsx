import { useState } from 'react';
import Button from './Button';

export default function BidForm({ currentBid, onSubmit, step = 200 }) {
  const [amount, setAmount] = useState('');

  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit?.(Number(amount) || currentBid + step);
        setAmount('');
      }}
      className="flex gap-2"
    >
      <input
        value={amount}
        onChange={(event) => setAmount(event.target.value)}
        placeholder={`৳ ${(currentBid + step).toLocaleString()} or more`}
        className="flex-1 rounded-md border border-border bg-background px-3 py-2 text-sm"
      />
      <Button type="submit" variant="primary">
        Place Bid
      </Button>
    </form>
  );
}
