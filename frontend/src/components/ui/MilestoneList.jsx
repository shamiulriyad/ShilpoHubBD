import Badge from './Badge';
import Button from './Button';

const toneByStatus = { Completed: 'success', InProgress: 'primary', Delayed: 'neutral', Pending: 'secondary' };

export default function MilestoneList({ milestones = [], onAdvance }) {
  if (milestones.length === 0) return <p className="text-sm text-body/60">No milestones yet.</p>;

  return (
    <div className="space-y-2">
      {milestones.map((milestone) => (
        <div key={milestone.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-border bg-surface p-3">
          <div>
            <p className="text-sm font-medium text-heading">{milestone.title}</p>
            <p className="text-xs text-body/60">
              Due {new Date(milestone.dueDate).toLocaleDateString()}
              {milestone.description ? ` · ${milestone.description}` : ''}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <Badge tone={toneByStatus[milestone.status] || 'neutral'}>{milestone.status}</Badge>
            {onAdvance && milestone.status !== 'Completed' && (
              <Button variant="secondary" onClick={() => onAdvance(milestone)}>
                Mark Complete
              </Button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
