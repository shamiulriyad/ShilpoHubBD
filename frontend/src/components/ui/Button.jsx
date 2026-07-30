export default function Button({ children, variant = 'primary', className = '', ...props }) {
  const baseClasses = 'inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition';
  const variants = {
    primary: 'bg-primary text-surface hover:bg-primary/90',
    secondary: 'border border-border bg-surface text-title hover:bg-background',
  };

  return (
    <button className={`${baseClasses} ${variants[variant] || variants.primary} ${className}`} {...props}>
      {children}
    </button>
  );
}
