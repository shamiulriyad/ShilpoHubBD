import { HELPLINE_NUMBER, HELPLINE_TEL } from '../../config/support';

// 24/7 helpline shown in the dashboard top bar so every member can reach the admin team.
export default function HelplineChip({ compact = false }) {
  return (
    <a
      href={HELPLINE_TEL}
      className="helpline-chip"
      aria-label={`24/7 helpline ${HELPLINE_NUMBER}`}
      title="Call the 24/7 helpline"
    >
      <svg viewBox="0 0 24 24" className="h-4 w-4 shrink-0" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1 1 .4 1.9.7 2.8a2 2 0 0 1-.5 2.1L8 9.9a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.8.7a2 2 0 0 1 1.7 2Z"/></svg>
      {!compact && <span className="hidden whitespace-nowrap xl:inline">24/7 Helpline <strong>{HELPLINE_NUMBER}</strong></span>}
      {compact && <span>{HELPLINE_NUMBER}</span>}
    </a>
  );
}
