import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { artisanSupportService as api } from '../services/artisanSupportService';

export const useSupportOrganization = (enabled = true) => useQuery({ queryKey: ['artisan-support', 'organization', 'me'], queryFn: api.myOrganization, enabled });
export const useSupportOrganizations = (enabled = true) => useQuery({ queryKey: ['artisan-support', 'organizations'], queryFn: api.organizations, enabled });
export const useSupportOrganizationOptions = (enabled = true) => useQuery({ queryKey: ['artisan-support', 'organization-options'], queryFn: api.organizationOptions, enabled });
export const useSupportArtisans = (enabled = true) => useQuery({ queryKey: ['artisan-support', 'artisans'], queryFn: api.artisans, enabled });
export const useSupportCases = () => useQuery({ queryKey: ['artisan-support', 'cases'], queryFn: api.cases });
export const useSupportDashboard = () => useQuery({ queryKey: ['artisan-support', 'dashboard'], queryFn: api.dashboard });
export const useCaseImpact = (caseId, enabled = true) => useQuery({
  queryKey: ['artisan-support', 'impact', caseId],
  queryFn: () => api.getImpact(caseId),
  enabled: enabled && !!caseId,
  retry: false,
});
export const useImpactReport = () => useQuery({ queryKey: ['artisan-support', 'impact-report'], queryFn: api.impactReport });

export function useArtisanSupportMutations() {
  const qc = useQueryClient();
  const refresh = () => { qc.invalidateQueries({ queryKey: ['artisan-support'] }); };
  const mutation = (fn) => useMutation({ mutationFn: fn, onSuccess: refresh });
  return {
    saveOrganization: mutation(api.saveOrganization), uploadDocument: mutation(api.uploadOrganizationDocument), reviewOrganization: mutation(({ id, payload }) => api.reviewOrganization(id, payload)),
    createCase: mutation(api.createCase), assign: mutation(({ id, payload }) => api.assign(id, payload)), accept: mutation(api.accept),
    inspect: mutation(({ id, payload }) => api.inspect(id, payload)), plan: mutation(({ id, payload }) => api.plan(id, payload)), provided: mutation(({ id, payload }) => api.provided(id, payload)),
    confirm: mutation(({ id, payload }) => api.confirm(id, payload)), monitor: mutation(({ id, payload }) => api.monitor(id, payload)), report: mutation(({ id, payload }) => api.report(id, payload)),
    reviewReport: mutation(({ id, payload }) => api.reviewReport(id, payload)), flag: mutation(({ id, payload }) => api.flag(id, payload)), evidence: mutation(({ id, payload }) => api.evidence(id, payload)),
    uploadEvidence: mutation(({ id, file, stage, caption }) => api.uploadEvidence(id, file, stage, caption)),
    generateImpact: useMutation({
      mutationFn: (id) => api.generateImpact(id),
      onSuccess: (_, id) => qc.invalidateQueries({ queryKey: ['artisan-support', 'impact', id] }),
    }),
  };
}
