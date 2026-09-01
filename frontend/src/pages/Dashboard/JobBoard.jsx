import { useState } from 'react';
import { PageHeader, Badge, Button, AsyncState } from '../../components/ui';
import {
  useJobListings, useMyJobListings, useMyJobApplications, useApplicationsForListing, useJobBoardMutations,
} from '../../hooks/useJobBoard';

const inputClass = 'rounded-md border border-border bg-background px-3 py-2 text-sm';
const employmentTypes = ['FullTime', 'PartTime', 'Contract', 'Apprenticeship', 'Freelance'];
const applicationStatusTone = { Applied: 'neutral', Shortlisted: 'secondary', Hired: 'success', Rejected: 'neutral', Withdrawn: 'neutral' };

function BrowseTab() {
  const { data, isLoading, isError, error } = useJobListings({ pageSize: 50 });
  const { apply } = useJobBoardMutations();
  const [coverMessage, setCoverMessage] = useState({});

  const listings = data?.items || [];

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="space-y-3">
        {listings.map((j) => (
          <div key={j.id} className="rounded-xl border border-border bg-surface p-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="text-sm font-semibold text-heading">{j.title}</p>
                <p className="text-xs text-body/60">
                  {j.employerName} · {j.employmentType}{j.location ? ` · ${j.location}` : ''}
                  {j.salaryMin ? ` · ৳${j.salaryMin.toLocaleString()}-${j.salaryMax?.toLocaleString() ?? ''}` : ''}
                </p>
              </div>
              <Badge>{j.applicationCount} applicant(s)</Badge>
            </div>
            <div className="mt-2 flex gap-2">
              <input
                placeholder="Short cover message"
                value={coverMessage[j.id] || ''}
                onChange={(e) => setCoverMessage((p) => ({ ...p, [j.id]: e.target.value }))}
                className={`${inputClass} flex-1`}
              />
              <Button
                size="sm"
                variant="primary"
                disabled={apply.isPending}
                onClick={() => apply.mutate({ jobListingId: j.id, coverMessage: coverMessage[j.id] || '' })}
              >
                Apply
              </Button>
            </div>
          </div>
        ))}
        {listings.length === 0 && <p className="text-sm text-body/60">No open job listings right now.</p>}
      </div>
    </AsyncState>
  );
}

function MyApplicationsTab() {
  const { data, isLoading, isError, error } = useMyJobApplications();
  const { withdrawApplication } = useJobBoardMutations();

  const applications = data || [];

  return (
    <AsyncState isLoading={isLoading} isError={isError} error={error}>
      <div className="space-y-2">
        {applications.map((a) => (
          <div key={a.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-4">
            <div>
              <p className="text-sm font-semibold text-heading">{a.jobTitle}</p>
              <p className="text-xs text-body/60">Applied {new Date(a.appliedAt).toLocaleDateString()}</p>
            </div>
            <div className="flex items-center gap-2">
              <Badge tone={applicationStatusTone[a.status] || 'neutral'}>{a.status}</Badge>
              {a.status === 'Applied' && (
                <button type="button" onClick={() => withdrawApplication.mutate(a.id)} className="text-xs text-danger hover:underline">Withdraw</button>
              )}
            </div>
          </div>
        ))}
        {applications.length === 0 && <p className="text-sm text-body/60">You haven't applied to any jobs yet.</p>}
      </div>
    </AsyncState>
  );
}

function ListingApplicants({ listingId }) {
  const applicantsQuery = useApplicationsForListing(listingId);
  const { shortlistApplication, rejectApplication, hireApplication } = useJobBoardMutations();
  const applicants = applicantsQuery.data || [];

  return (
    <div className="mt-3 space-y-1 border-t border-border pt-3">
      {applicants.map((a) => (
        <div key={a.id} className="flex items-center justify-between rounded-lg border border-border px-3 py-2 text-xs">
          <span>{a.applicantName} — {a.status}</span>
          {a.status === 'Applied' && (
            <div className="flex gap-2">
              <button type="button" onClick={() => shortlistApplication.mutate({ id: a.id, payload: {} })} className="text-primary hover:underline">Shortlist</button>
              <button type="button" onClick={() => rejectApplication.mutate({ id: a.id, payload: {} })} className="text-danger hover:underline">Reject</button>
            </div>
          )}
          {a.status === 'Shortlisted' && (
            <button type="button" onClick={() => hireApplication.mutate({ id: a.id, payload: {} })} className="text-success hover:underline">Hire</button>
          )}
        </div>
      ))}
      {applicants.length === 0 && <p className="text-xs text-body/50">No applicants yet.</p>}
    </div>
  );
}

function PostingsTab() {
  const { data, isLoading, isError, error } = useMyJobListings();
  const { createListing, publishListing, closeListing } = useJobBoardMutations();
  const [showForm, setShowForm] = useState(false);
  const [expandedId, setExpandedId] = useState(null);
  const [form, setForm] = useState({ title: '', description: '', employmentType: 'FullTime', location: '' });

  const listings = data || [];

  const handleCreate = (event) => {
    event.preventDefault();
    createListing.mutate(form, { onSuccess: () => { setShowForm(false); setForm({ title: '', description: '', employmentType: 'FullTime', location: '' }); } });
  };

  return (
    <div>
      <div className="mb-4 flex justify-end">
        <Button variant="primary" onClick={() => setShowForm((v) => !v)}>{showForm ? 'Cancel' : 'Post a Job'}</Button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="mb-6 grid gap-3 rounded-xl border border-border bg-surface p-4 sm:grid-cols-2">
          <input required placeholder="Job title" value={form.title} onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <select value={form.employmentType} onChange={(e) => setForm((p) => ({ ...p, employmentType: e.target.value }))} className={inputClass}>
            {employmentTypes.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          <input placeholder="Location" value={form.location} onChange={(e) => setForm((p) => ({ ...p, location: e.target.value }))} className={inputClass} />
          <textarea required rows={2} placeholder="Description" value={form.description} onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))} className={`${inputClass} sm:col-span-2`} />
          <Button type="submit" variant="primary" className="sm:col-span-2" disabled={createListing.isPending}>Create Listing</Button>
        </form>
      )}

      <AsyncState isLoading={isLoading} isError={isError} error={error}>
        <div className="space-y-2">
          {listings.map((j) => (
            <div key={j.id} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="text-sm font-semibold text-heading">{j.title}</p>
                  <p className="text-xs text-body/60">{j.employmentType}{j.location ? ` · ${j.location}` : ''} · {j.applicationCount} applicant(s)</p>
                </div>
                <div className="flex items-center gap-2">
                  <Badge tone={j.status === 'Published' ? 'success' : 'neutral'}>{j.status}</Badge>
                  {j.status !== 'Published' ? (
                    <button type="button" onClick={() => publishListing.mutate(j.id)} className="text-xs text-primary hover:underline">Publish</button>
                  ) : (
                    <button type="button" onClick={() => closeListing.mutate(j.id)} className="text-xs text-danger hover:underline">Close</button>
                  )}
                  <Button variant="secondary" onClick={() => setExpandedId(expandedId === j.id ? null : j.id)}>
                    {expandedId === j.id ? 'Hide' : 'Applicants'}
                  </Button>
                </div>
              </div>
              {expandedId === j.id && <ListingApplicants listingId={j.id} />}
            </div>
          ))}
          {listings.length === 0 && <p className="text-sm text-body/60">You haven't posted any jobs yet.</p>}
        </div>
      </AsyncState>
    </div>
  );
}

function MatchesTab() {
  const { getRecommendedJobs } = useJobBoardMutations();
  const [form, setForm] = useState({ location: '', yearsOfExperience: '' });

  const results = getRecommendedJobs.data || [];

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-end gap-2">
        <input placeholder="Location" value={form.location} onChange={(e) => setForm((p) => ({ ...p, location: e.target.value }))} className={inputClass} />
        <input type="number" min="0" placeholder="Years of experience" value={form.yearsOfExperience} onChange={(e) => setForm((p) => ({ ...p, yearsOfExperience: e.target.value }))} className={inputClass} />
        <Button
          variant="primary"
          disabled={getRecommendedJobs.isPending}
          onClick={() => getRecommendedJobs.mutate({ ...form, yearsOfExperience: form.yearsOfExperience === '' ? null : Number(form.yearsOfExperience), maxResults: 10 })}
        >
          {getRecommendedJobs.isPending ? 'Matching…' : 'Get AI Job Matches'}
        </Button>
      </div>

      {getRecommendedJobs.isSuccess && (
        <div className="space-y-2">
          {results.map((r) => (
            <div key={r.jobListingId} className="rounded-xl border border-border bg-surface p-4">
              <div className="flex items-center justify-between">
                <p className="text-sm font-semibold text-heading">{r.title} — {r.employerName}</p>
                <Badge tone="success">{Math.round(r.matchScore)}% match</Badge>
              </div>
              <p className="text-xs text-body/60">{r.employmentType}{r.location ? ` · ${r.location}` : ''}</p>
              {r.matchReasons.length > 0 && <p className="mt-1 text-xs text-body/50">Why: {r.matchReasons.join(' · ')}</p>}
            </div>
          ))}
          {results.length === 0 && <p className="text-sm text-body/60">No matches found — try broadening your filters.</p>}
        </div>
      )}
    </div>
  );
}

const tabs = [
  { key: 'browse', label: 'Browse & Apply' },
  { key: 'applications', label: 'My Applications' },
  { key: 'postings', label: 'My Postings' },
  { key: 'matches', label: 'AI Matches' },
];

export default function JobBoard() {
  const [tab, setTab] = useState('browse');

  return (
    <div>
      <PageHeader title="Job Board" description="Browse and apply to jobs, manage your applications, post openings, and get AI-matched roles." />

      <div className="mb-4 flex flex-wrap gap-2 border-b border-border">
        {tabs.map((t) => (
          <button
            key={t.key}
            type="button"
            onClick={() => setTab(t.key)}
            className={`border-b-2 px-3 py-2 text-sm font-medium ${tab === t.key ? 'border-primary text-primary' : 'border-transparent text-body/60'}`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'browse' && <BrowseTab />}
      {tab === 'applications' && <MyApplicationsTab />}
      {tab === 'postings' && <PostingsTab />}
      {tab === 'matches' && <MatchesTab />}
    </div>
  );
}
