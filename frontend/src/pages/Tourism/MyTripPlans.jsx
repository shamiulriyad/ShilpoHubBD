import { useState } from 'react';
import { Link } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import TravelEmptyState from '../../components/ui/TravelEmptyState';
import MutationFeedback from '../../components/ui/MutationFeedback';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import { useSavedTourPlans, useDeleteSavedTourPlan } from '../../hooks/useAITourism';

import { confirmAction } from '../../lib/confirm';
export default function MyTripPlans() {
  const [deleteId, setDeleteId] = useState(null);
  const { data, isLoading, isError, error } = useSavedTourPlans({ pageSize: 50 });
  const remove = useDeleteSavedTourPlan();
  const plans = data?.items || [];

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'My Trip Plans' },
        ]}
        title="My Trip Plans"
        description="Every itinerary you generate with the AI Trip Planner is saved here, so you can reopen it any time."
        action={
          <Link
            to={routePaths.tourismAiPlanner}
            className="rounded-full bg-primary px-4 py-2 text-sm font-semibold text-white hover:opacity-90"
          >
            Plan a new trip
          </Link>
        }
      />

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="divide-y divide-border rounded-xl border border-border bg-surface">
          {plans.map((plan) => (
            <div key={plan.id} className="flex flex-wrap items-center justify-between gap-3 p-4">
              <div>
                <p className="text-sm font-medium text-heading">{plan.title}</p>
                <p className="text-xs text-body/60">
                  {plan.originText ? `From ${plan.originText} · ` : ''}
                  {plan.durationDays} day{plan.durationDays === 1 ? '' : 's'} · Party of {plan.partySize} · {plan.transportMode}
                  {plan.startDate ? ` · Starts ${new Date(plan.startDate).toLocaleDateString(undefined, { timeZone: 'UTC' })}` : ''}
                </p>
                <p className="mt-0.5 text-xs text-body/50">Saved {new Date(plan.createdAt).toLocaleString()}</p>
              </div>
              <div className="flex flex-wrap items-center gap-3">
                {plan.totalEstimatedCost != null && (
                  <p className="text-sm font-semibold text-primary">
                    ৳ {Number(plan.totalEstimatedCost).toLocaleString('en-BD')}
                  </p>
                )}
                <Badge tone={plan.isAiGenerated ? 'primary' : 'secondary'}>{plan.isAiGenerated ? 'AI' : 'Rule-based'}</Badge>
                <Link
                  to={`${routePaths.tourismAiPlanner}?plan=${plan.id}`}
                  className="rounded-full border border-border px-3 py-1.5 text-xs font-semibold text-heading hover:border-primary"
                >
                  Open plan →
                </Link>
                <Button variant="secondary" onClick={() => setDeleteId(plan.id)} disabled={remove.isPending}>
                  Delete
                </Button>
              </div>
            </div>
          ))}
          {plans.length === 0 && (
            <TravelEmptyState
              title="No saved trip plans yet"
              description="Generate an itinerary with the AI Trip Planner and it will be saved here automatically."
            />
          )}
        </div>
      </AsyncState>

      {deleteId && (
        <section aria-label="Confirm trip plan deletion" className="mt-4 rounded-xl border border-border bg-surface p-5">
          <h2 className="font-semibold">Delete {plans.find((p) => p.id === deleteId)?.title || 'this trip plan'}?</h2>
          <p className="mt-2 text-sm text-body/70">This removes the saved itinerary permanently.</p>
          <div className="mt-4 flex gap-3">
            <Button variant="secondary" disabled={remove.isPending} onClick={() => setDeleteId(null)}>
              Keep plan
            </Button>
            <Button disabled={remove.isPending} onClick={async () => { if (await confirmAction('Remove this? This cannot be undone.', { confirmLabel: 'Yes, remove' })) remove.mutate(deleteId, { onSuccess: () => setDeleteId(null) }); }}>
              {remove.isPending ? 'Deleting…' : 'Delete plan'}
            </Button>
          </div>
        </section>
      )}
      <div className="mt-4">
        <MutationFeedback mutation={remove} />
      </div>
    </div>
  );
}
