import { useState } from 'react';
import Badge from '../ui/Badge';

export default function VideoPlayer({ title, live = false, viewers, bordered = true, className = '' }) {
  const [playing, setPlaying] = useState(false);

  return (
    <div
      className={`relative flex aspect-video items-center justify-center bg-background ${
        bordered ? 'rounded-2xl border border-border' : ''
      } ${className}`}
    >
      {live && (
        <Badge tone="success" className="absolute left-3 top-3">
          Live{viewers ? ` · ${viewers} watching` : ''}
        </Badge>
      )}
      <button
        type="button"
        onClick={() => setPlaying((prev) => !prev)}
        className="flex h-14 w-14 items-center justify-center rounded-full bg-primary text-xl text-surface transition hover:bg-primary/90"
        aria-label={playing ? 'Pause video' : 'Play video'}
      >
        {playing ? '❚❚' : '▶'}
      </button>
      {title && (
        <p className="absolute bottom-3 left-3 right-3 truncate text-xs text-body/60">
          {playing ? `Playing: ${title}` : title}
        </p>
      )}
    </div>
  );
}
