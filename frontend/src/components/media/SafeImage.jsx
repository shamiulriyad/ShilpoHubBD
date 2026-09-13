import { useEffect, useState } from 'react';

export default function SafeImage({ src, alt = '', className = '', fallbackLabel = 'Image unavailable', onError, ...props }) {
  const [failed, setFailed] = useState(!src);

  useEffect(() => {
    setFailed(!src);
  }, [src]);

  if (failed) {
    return (
      <div
        className={`flex items-center justify-center bg-background text-center text-xs text-body/50 ${className}`}
        role={alt ? 'img' : undefined}
        aria-label={alt || undefined}
        aria-hidden={alt ? undefined : 'true'}
      >
        {fallbackLabel}
      </div>
    );
  }

  return (
    <img
      src={src}
      alt={alt}
      className={className}
      onError={(event) => {
        setFailed(true);
        onError?.(event);
      }}
      {...props}
    />
  );
}