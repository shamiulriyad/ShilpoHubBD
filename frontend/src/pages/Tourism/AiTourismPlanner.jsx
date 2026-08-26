import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader } from '../../components/ui';
import { useDistricts } from '../../hooks/useDistricts';
import { useTourPlan } from '../../hooks/useAITourism';

export default function AiTourismPlanner() {
  const districtsQuery = useDistricts();
  const tourPlan = useTourPlan();
  const [form, setForm] = useState({ districtId: '', durationDays: 2, partySize: 1 });

  const handleSubmit = (event) => {
    event.preventDefault();
    tourPlan.mutate({
      districtId: form.districtId || undefined,
      durationDays: Number(form.durationDays),
      partySize: Number(form.partySize),
    });
  };

  const plan = tourPlan.data;

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <PageHeader
        breadcrumbs={[
          { label: 'Home', path: routePaths.home },
          { label: 'Tourism', path: routePaths.tourism },
          { label: 'AI Trip Planner' },
        ]}
        title="AI Trip Planner"
        description="Get a day-by-day heritage itinerary tailored to your district, duration and group size."
        action={<Badge tone="primary">AI Powered</Badge>}
      />

      <form onSubmit={handleSubmit} className="mb-10 grid gap-4 rounded-xl border border-border bg-surface p-6 sm:grid-cols-3">
        <select
          value={form.districtId}
          onChange={(event) => setForm((prev) => ({ ...prev, districtId: event.target.value }))}
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        >
          <option value="">Any district</option>
          {(districtsQuery.data || []).map((district) => (
            <option key={district.id} value={district.id}>
              {district.name}
            </option>
          ))}
        </select>
        <input
          type="number"
          min={1}
          max={30}
          value={form.durationDays}
          onChange={(event) => setForm((prev) => ({ ...prev, durationDays: event.target.value }))}
          placeholder="Duration (days)"
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <input
          type="number"
          min={1}
          max={100}
          value={form.partySize}
          onChange={(event) => setForm((prev) => ({ ...prev, partySize: event.target.value }))}
          placeholder="Party size"
          className="rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <Button type="submit" variant="primary" className="sm:col-span-3" disabled={tourPlan.isPending}>
          {tourPlan.isPending ? 'Planning…' : 'Generate Itinerary'}
        </Button>
      </form>

      {plan && (
        <div>
          <SectionHeader eyebrow="Your Trip" title="Suggested Itinerary" description={plan.summary} />
          <div className="space-y-4">
            {plan.days.map((day) => (
              <div key={day.dayNumber} className="rounded-xl border border-border bg-surface p-5">
                <p className="text-sm font-semibold text-heading">
                  Day {day.dayNumber}
                  {day.date && ` · ${new Date(day.date).toLocaleDateString()}`}
                </p>
                <ul className="mt-3 space-y-2">
                  {day.stops.map((stop, i) => (
                    <li key={i} className="text-sm text-body/70">
                      <span className="font-medium text-heading">{stop.name}</span> ({stop.type})
                      {stop.notes && <span className="text-body/50"> — {stop.notes}</span>}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
          {plan.highlightedFestivals.length > 0 && (
            <p className="mt-4 text-sm text-body/70">
              <span className="font-medium text-heading">Festivals to watch for: </span>
              {plan.highlightedFestivals.join(', ')}
            </p>
          )}
        </div>
      )}
    </div>
  );
}
