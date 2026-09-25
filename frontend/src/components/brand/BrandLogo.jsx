import SafeImage from '../media/SafeImage';
export default function BrandLogo({ size = 'md', showName = true, inverse = false, className = '' }) {
  return <span className={`brand-lockup brand-${size} ${inverse ? 'brand-inverse' : ''} ${className}`}>
    <SafeImage src="/images/shilpohub-logo.png" alt="ShilpoHub heritage emblem" className="brand-emblem" width="355" height="447" />
    {showName && <span className="brand-wordmark">ShilpoHub<span className="brand-tagline">Heritage. People. Possibility.</span></span>}
  </span>;
}
