import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, AsyncState } from '../../components/ui';
import { useLiveClasses } from '../../hooks/useLiveClasses';

const statusTone = { Scheduled: 'secondary', Live: 'success', Ended: 'neutral', Cancelled: 'neutral' };

export default function LiveClasses() {
  const { data, isLoading, isError, error } = useLiveClasses({ pageSize: 24 });
  const classes = data?.items || [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Academy', path: routePaths.academy },
          { label: 'Live Classes' },
        ]}
        title="Live Classes"
        description="Join live sessions with mentors, ask questions and learn in real time."
      />
      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {classes.map((liveClass) => (
            <Link
              key={liveClass.id}
              to={routePaths.academyLiveClassDetails.replace(':liveClassId', liveClass.id)}
              className="rounded-xl border border-border bg-surface p-4 transition hover:shadow-md"
            >
              <Badge tone={statusTone[liveClass.status] || 'neutral'}>{liveClass.status}</Badge>
              <p className="mt-3 text-sm font-semibold text-heading">{liveClass.title}</p>
              <p className="mt-1 text-xs text-body/60">By {liveClass.instructorName}</p>
              <p className="mt-2 text-xs text-body/50">{new Date(liveClass.scheduledStartAt).toLocaleString()}</p>
              <p className="mt-1 text-xs text-body/50">
                {liveClass.participantCount}{liveClass.maxParticipants ? `/${liveClass.maxParticipants}` : ''} registered
              </p>
            </Link>
          ))}
          {classes.length === 0 && <p className="col-span-full text-sm text-body/60">No live classes scheduled yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}
