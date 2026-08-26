import { useState } from 'react';
import { routePaths } from '../../routes/routePaths';
import { PageHeader, Badge, Button, SectionHeader, AsyncState } from '../../components/ui';
import { useAllBadges, useMyBadges, useMyCheckIns, useMyJournal, usePassportMutations } from '../../hooks/usePassport';

export default function HeritagePassport() {
  const badgesQuery = useAllBadges();
  const myBadgesQuery = useMyBadges();
  const checkInsQuery = useMyCheckIns();
  const journalQuery = useMyJournal();
  const { claimDistrictBadge, addJournalEntry } = usePassportMutations();
  const [entry, setEntry] = useState({ title: '', content: '', photoUrl: '' });

  const earnedBadgeIds = new Set((myBadgesQuery.data || []).map((b) => b.badgeId));
  const badges = badgesQuery.data || [];
  const collected = earnedBadgeIds.size;

  const handleAddEntry = (event) => {
    event.preventDefault();
    addJournalEntry.mutate(
      { title: entry.title, content: entry.content, photoUrl: entry.photoUrl || undefined },
      { onSuccess: () => setEntry({ title: '', content: '', photoUrl: '' }) },
    );
  };

  return (
    <div>
      <PageHeader
        breadcrumbs={[{ label: 'Dashboard', path: routePaths.customer }, { label: 'Heritage Passport' }]}
        title="Heritage Passport"
        description={`${collected} of ${badges.length} badges collected.`}
      />

      <AsyncState isLoading={badgesQuery.isLoading} isError={badgesQuery.isError} error={badgesQuery.error}>
        <div className="mb-12 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {badges.map((badge) => {
            const earned = earnedBadgeIds.has(badge.id);
            return (
              <div
                key={badge.id}
                className={`flex flex-col items-center gap-2 rounded-xl border p-5 text-center ${
                  earned ? 'border-primary/30 bg-primary/5' : 'border-dashed border-border bg-surface opacity-70'
                }`}
              >
                <span
                  className={`flex h-14 w-14 items-center justify-center rounded-full border-2 text-lg font-semibold ${
                    earned ? 'border-primary text-primary' : 'border-border text-body/30'
                  }`}
                >
                  {earned ? '✓' : '?'}
                </span>
                <p className="text-sm font-semibold text-heading">{badge.name}</p>
                <p className="text-xs text-body/60">{badge.type}{badge.districtName ? ` · ${badge.districtName}` : ''}</p>
                {earned ? (
                  <Badge tone="success">Collected</Badge>
                ) : badge.type === 'District' && badge.districtId ? (
                  <Button variant="secondary" onClick={() => claimDistrictBadge.mutate(badge.districtId)} disabled={claimDistrictBadge.isPending}>
                    Claim
                  </Button>
                ) : (
                  <Badge tone="neutral">Not yet collected</Badge>
                )}
              </div>
            );
          })}
          {badges.length === 0 && <p className="col-span-full text-sm text-body/60">No passport badges defined yet.</p>}
        </div>
      </AsyncState>

      <SectionHeader eyebrow="Travel Log" title="My Check-Ins" />
      <AsyncState isLoading={checkInsQuery.isLoading} isError={checkInsQuery.isError} error={checkInsQuery.error}>
        <div className="mb-12 divide-y divide-border rounded-xl border border-border bg-surface">
          {(checkInsQuery.data || []).map((checkIn) => (
            <div key={checkIn.id} className="flex items-center justify-between p-4 text-sm">
              <span className="font-medium text-heading">{checkIn.heritagePlaceName}</span>
              <span className="text-body/50">{new Date(checkIn.checkedInAt).toLocaleDateString()}</span>
            </div>
          ))}
          {(checkInsQuery.data || []).length === 0 && (
            <p className="p-6 text-center text-sm text-body/60">No check-ins yet — visit a heritage place to check in.</p>
          )}
        </div>
      </AsyncState>

      <SectionHeader eyebrow="Memories" title="Travel Journal" />
      <form onSubmit={handleAddEntry} className="mb-6 space-y-3 rounded-xl border border-border bg-surface p-4">
        <input
          required
          placeholder="Entry title"
          value={entry.title}
          onChange={(event) => setEntry((prev) => ({ ...prev, title: event.target.value }))}
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <textarea
          required
          rows={3}
          placeholder="Write about your visit…"
          value={entry.content}
          onChange={(event) => setEntry((prev) => ({ ...prev, content: event.target.value }))}
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <input
          placeholder="Photo URL (optional)"
          value={entry.photoUrl}
          onChange={(event) => setEntry((prev) => ({ ...prev, photoUrl: event.target.value }))}
          className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm"
        />
        <Button type="submit" variant="primary" disabled={addJournalEntry.isPending}>
          {addJournalEntry.isPending ? 'Saving…' : 'Add Entry'}
        </Button>
      </form>

      <AsyncState isLoading={journalQuery.isLoading} isError={journalQuery.isError} error={journalQuery.error}>
        <div className="space-y-3">
          {(journalQuery.data || []).map((item) => (
            <div key={item.id} className="rounded-xl border border-border bg-surface p-4">
              <p className="text-sm font-semibold text-heading">{item.title}</p>
              <p className="mt-1 text-sm text-body/70">{item.content}</p>
              <p className="mt-2 text-xs text-body/50">{new Date(item.createdAt).toLocaleDateString()}</p>
            </div>
          ))}
          {(journalQuery.data || []).length === 0 && (
            <p className="text-center text-sm text-body/60">No journal entries yet.</p>
          )}
        </div>
      </AsyncState>
    </div>
  );
}
