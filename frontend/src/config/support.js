// Support contact shown on every dashboard, the footer and the About page.
// Set VITE_HELPLINE_NUMBER in .env to the real 24/7 number; the value below is only a placeholder.
export const HELPLINE_NUMBER = import.meta.env.VITE_HELPLINE_NUMBER?.trim() || '+880 1700-000000';
export const HELPLINE_TEL = `tel:${HELPLINE_NUMBER.replace(/[^+\d]/g, '')}`;
