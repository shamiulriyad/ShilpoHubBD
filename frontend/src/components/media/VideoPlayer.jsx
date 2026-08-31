import { useState } from 'react';
import Badge from '../ui/Badge';

// Recognises bare YouTube / Vimeo links and returns an embeddable URL, else null.
function toEmbedUrl(src) {
  if (!src) return null;
  try {
    const url = new URL(src);
    const host = url.hostname.replace(/^www\./, '');
    if (host === 'youtube.com' && url.searchParams.get('v')) {
      return `https://www.youtube.com/embed/${url.searchParams.get('v')}`;
    }
    if (host === 'youtu.be') {
      return `https://www.youtube.com/embed${url.pathname}`;
    }
    if (host === 'vimeo.com') {
      return `https://player.vimeo.com/video${url.pathname}`;
    }
  } catch {
    return null;
  }
  return null;
}

export default function VideoPlayer({ title, src, live = false, viewers, bordered = true, className = '' }) {
  const [playing, setPlaying] = useState(false);
  const embedUrl = toEmbedUrl(src);
  const isFile = src && !embedUrl;

  const frameClass = `relative flex aspect-video items-center justify-center overflow-hidden bg-background ${
    bordered ? 'rounded-2xl border border-border' : ''
  } ${className}`;

  if (playing && embedUrl) {
    return (
      <div className={frameClass}>
        <iframe
          src={`${embedUrl}?autoplay=1`}
          title={title || 'Video'}
          allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture"
          allowFullScreen
          className="h-full w-full border-0"
        />
      </div>
    );
  }

  if (playing && isFile) {
    return (
      <div className={frameClass}>
        {/* eslint-disable-next-line jsx-a11y/media-has-caption */}
        <video src={src} controls autoPlay className="h-full w-full bg-black object-contain">
          <track kind="captions" />
        </video>
      </div>
    );
  }

  return (
    <div className={frameClass}>
      {live && (
        <Badge tone="success" className="absolute left-3 top-3">
          Live{viewers ? ` · ${viewers} watching` : ''}
        </Badge>
      )}
      <button
        type="button"
        onClick={() => (src ? setPlaying(true) : setPlaying((prev) => !prev))}
        className="flex h-14 w-14 items-center justify-center rounded-full bg-primary text-xl text-surface transition hover:bg-primary/90"
        aria-label="Play video"
      >
        ▶
      </button>
      {title && (
        <p className="absolute bottom-3 left-3 right-3 truncate text-xs text-body/60">{title}</p>
      )}
    </div>
  );
}
