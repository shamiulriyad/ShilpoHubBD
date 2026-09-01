import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { jobBoardService } from '../services/jobBoardService';

export function useJobListings(params = {}) {
  return useQuery({ queryKey: ['job-listings', 'list', params], queryFn: () => jobBoardService.listListings(params) });
}

export function useMyJobListings() {
  return useQuery({ queryKey: ['job-listings', 'mine'], queryFn: () => jobBoardService.getMyListings() });
}

export function useMyJobApplications() {
  return useQuery({ queryKey: ['job-applications', 'mine'], queryFn: () => jobBoardService.getMyApplications() });
}

export function useApplicationsForListing(jobListingId) {
  return useQuery({
    queryKey: ['job-applications', 'listing', jobListingId],
    queryFn: () => jobBoardService.getApplicationsForListing(jobListingId),
    enabled: Boolean(jobListingId),
  });
}

export function useJobBoardMutations() {
  const queryClient = useQueryClient();
  const invalidateListings = () => queryClient.invalidateQueries({ queryKey: ['job-listings'] });
  const invalidateApplications = () => queryClient.invalidateQueries({ queryKey: ['job-applications'] });

  return {
    createListing: useMutation({ mutationFn: (payload) => jobBoardService.createListing(payload), onSuccess: invalidateListings }),
    publishListing: useMutation({ mutationFn: (id) => jobBoardService.publishListing(id), onSuccess: invalidateListings }),
    closeListing: useMutation({ mutationFn: (id) => jobBoardService.closeListing(id), onSuccess: invalidateListings }),
    apply: useMutation({ mutationFn: (payload) => jobBoardService.apply(payload), onSuccess: invalidateApplications }),
    shortlistApplication: useMutation({
      mutationFn: ({ id, payload }) => jobBoardService.shortlistApplication(id, payload),
      onSuccess: invalidateApplications,
    }),
    rejectApplication: useMutation({
      mutationFn: ({ id, payload }) => jobBoardService.rejectApplication(id, payload),
      onSuccess: invalidateApplications,
    }),
    hireApplication: useMutation({
      mutationFn: ({ id, payload }) => jobBoardService.hireApplication(id, payload),
      onSuccess: invalidateApplications,
    }),
    withdrawApplication: useMutation({ mutationFn: (id) => jobBoardService.withdrawApplication(id), onSuccess: invalidateApplications }),
    getRecommendedJobs: useMutation({ mutationFn: (payload) => jobBoardService.getRecommendedJobs(payload) }),
  };
}
