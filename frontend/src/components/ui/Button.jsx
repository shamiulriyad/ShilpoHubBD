export default function Button({ children, variant = 'primary', size = 'md', className = '', ...props }) {
  const baseClasses = 'inline-flex items-center justify-center rounded-full font-semibold tracking-[0.01em] transition duration-200 focus:outline-none focus:ring-4 focus:ring-primary/15 disabled:pointer-events-none disabled:opacity-50';
  const variants = {
    primary: 'bg-primary text-surface shadow-[0_8px_20px_rgba(168,79,45,0.24)] hover:-translate-y-0.5 hover:bg-primary-dark hover:shadow-[0_12px_24px_rgba(168,79,45,0.28)]',
    secondary: 'border border-border bg-surface text-title shadow-sm hover:-translate-y-0.5 hover:border-primary/30 hover:bg-primary-soft',
  };
  const sizes = {
    sm: 'px-3.5 py-2 text-sm',
    md: 'px-5 py-2.5 text-sm',
    lg: 'px-6 py-3 text-base',
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