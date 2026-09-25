import { HELPLINE_NUMBER, HELPLINE_TEL } from '../../config/support';

// 24/7 helpline shown in the dashboard top bar so every member can reach the admin team.
export default function HelplineChip({ compact = false }) {
  return (
    <a
      href={HELPLINE_TEL}
      className="inline-flex items-center gap-2 rounded-full border border-border bg-surface px-3 py-1.5 text-xs font-medium text-body transition hover:border-primary/40 hover:text-primary"
      aria-label={`24/7 helpline ${HELPLINE_NUMBER}`}
      title="Call the 24/7 helpline"
    >
      <span aria-hidden="true">📞</span>
      {!compact && <span className="hidden whitespace-nowrap md:inline">24/7 Helpline <span className="font-semibold text-heading">{HELPLINE_NUMBER}</span></span>}
      {compact && <span>{HELPLINE_NUMBER}</span>}
    </a>
  );
}
