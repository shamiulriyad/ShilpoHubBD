import { useState } from 'react';

/**
 * A list of short words typed one at a time (Enter or comma adds, × removes).
 * Values are trimmed and de-duplicated (case-insensitive); `max` and `maxLength` mirror the server's limits.
 */
export default function ChipInput({ id, label, hint, values, onChange, placeholder, max = 30, maxLength = 60 }) {
  const [draft, setDraft] = useState('');

  const add = (raw) => {
    const parts = raw.split(',').map((p) => p.trim()).filter(Boolean);
    if (!parts.length) return;
    const next = [...values];
    for (const part of parts) {
      const word = part.slice(0, maxLength);
      if (next.length < max && !next.some((v) => v.toLowerCase() === word.toLowerCase())) next.push(word);
    }
    onChange(next);
    setDraft('');
  };

  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-sm font-medium text-heading">{label}</label>
      <div className="flex flex-wrap items-center gap-2 rounded-md border border-border bg-background px-2.5 py-2 focus-within:border-primary focus-within:ring-2 focus-within:ring-primary/25">
        {values.map((value) => (
          <span key={value} className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-1 text-xs font-medium text-primary">
            {value}
            <button type="button" onClick={() => onChange(values.filter((v) => v !== value))} aria-label={`Remove ${value}`} className="rounded-full px-1 leading-none hover:bg-primary/20">×</button>
          </span>
        ))}
        <input
          id={id}
          value={draft}
          disabled={values.length >= max}
          onChange={(e) => (e.target.value.includes(',') ? add(e.target.value) : setDraft(e.target.value))}
          onKeyDown={(e) => {
            if (e.key === 'Enter') { e.preventDefault(); add(draft); }
            else if (e.key === 'Backspace' && !draft && values.length) onChange(values.slice(0, -1));
          }}
          onBlur={() => add(draft)}
          placeholder={values.length ? '' : placeholder}
          className="min-w-[8rem] flex-1 bg-transparent py-1 text-sm outline-none"
        />
      </div>
      {hint && <p className="mt-1 text-xs text-body/60">{hint}</p>}
    </div>
  );
}
