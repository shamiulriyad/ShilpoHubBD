export default function Button({ children, variant = 'primary', size = 'md', className = '', ...props }) {
  const baseClasses = 'inline-flex items-center justify-center rounded-md font-medium transition';
  const variants = {
    primary: 'bg-primary text-surface hover:bg-primary/90',
    secondary: 'border border-border bg-surface text-title hover:bg-background',
  };
  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-sm',
    lg: 'px-5 py-2.5 text-base',
  };

  return (
    <button
      className={`${baseClasses} ${sizes[size] || sizes.md} ${variants[variant] || variants.primary} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
}
