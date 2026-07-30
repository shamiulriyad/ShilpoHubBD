const tones = {
  neutral: 'bg-background text-body border-border',
  primary: 'bg-primary/10 text-primary border-primary/20',
  success: 'bg-success/10 text-success border-success/20',
  secondary: 'bg-secondary/10 text-secondary border-secondary/20',
};

export default function Badge({ children, tone = 'neutral', className = '' }) {
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${tones[tone] || tones.neutral} ${className}`}
    >
      {children}
    </span>
  );
}
