import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { researchWorkspaceService } from '../services/researchWorkspaceService';

export function useResearchProjects(params = {}) {
  return useQuery({ queryKey: ['research-projects', 'list', params], queryFn: () => researchWorkspaceService.listProjects(params) });
}

export function useResearchProject(id) {
  return useQuery({ queryKey: ['research-projects', id], queryFn: () => researchWorkspaceService.getProject(id), enabled: Boolean(id) });
}

export function useResearchActivity(id, take = 50) {
  return useQuery({
    queryKey: ['research-projects', id, 'activity', take],
    queryFn: () => researchWorkspaceService.getActivity(id, take),
    enabled: Boolean(id),
  });
}

export function useResearchProjectMutations() {
  const queryClient = useQueryClient();
  const invalidate = (id) => {
    queryClient.invalidateQueries({ queryKey: ['research-projects'] });
    if (id) queryClient.invalidateQueries({ queryKey: ['research-projects', id] });
  };

  return {
    create: useMutation({ mutationFn: (payload) => researchWorkspaceService.createProject(payload), onSuccess: () => invalidate() }),
    update: useMutation({
      mutationFn: ({ id, payload }) => researchWorkspaceService.updateProject(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    updateStatus: useMutation({
      mutationFn: ({ id, payload }) => researchWorkspaceService.updateProjectStatus(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    remove: useMutation({ mutationFn: (id) => researchWorkspaceService.removeProject(id), onSuccess: () => invalidate() }),
    addMember: useMutation({
      mutationFn: ({ id, payload }) => researchWorkspaceService.addMember(id, payload),
      onSuccess: (_, { id }) => invalidate(id),
    }),
    removeMember: useMutation({
      mutationFn: ({ id, memberId }) => researchWorkspaceService.removeMember(id, memberId),
      onSuccess: (_, { id }) => invalidate(id),
    }),
  };
}

export function useResearchPapers(projectId) {
  return useQuery({ queryKey: ['research-papers', projectId], queryFn: () => researchWorkspaceService.listPapers(projectId), enabled: Boolean(projectId) });
}

export function useResearchNotes(projectId) {
  return useQuery({ queryKey: ['research-notes', projectId], queryFn: () => researchWorkspaceService.listNotes(projectId), enabled: Boolean(projectId) });
}

export function useResearchTasks(projectId, params = {}) {
  return useQuery({
    queryKey: ['research-tasks', projectId, params],
    queryFn: () => researchWorkspaceService.listTasks(projectId, params),
    enabled: Boolean(projectId),
  });
}

export function useResearchMilestones(projectId) {
  return useQuery({
    queryKey: ['research-milestones', projectId],
    queryFn: () => researchWorkspaceService.listMilestones(projectId),
    enabled: Boolean(projectId),
  });
}

export function useResearchWorkItemMutations(projectId) {
  const queryClient = useQueryClient();
  const invalidate = (key) => queryClient.invalidateQueries({ queryKey: [key, projectId] });

  return {
    createPaper: useMutation({ mutationFn: (payload) => researchWorkspaceService.createPaper(projectId, payload), onSuccess: () => invalidate('research-papers') }),
    updatePaper: useMutation({
      mutationFn: ({ paperId, payload }) => researchWorkspaceService.updatePaper(projectId, paperId, payload),
      onSuccess: () => invalidate('research-papers'),
    }),
    removePaper: useMutation({ mutationFn: (paperId) => researchWorkspaceService.removePaper(projectId, paperId), onSuccess: () => invalidate('research-papers') }),

    createNote: useMutation({ mutationFn: (payload) => researchWorkspaceService.createNote(projectId, payload), onSuccess: () => invalidate('research-notes') }),
    removeNote: useMutation({ mutationFn: (noteId) => researchWorkspaceService.removeNote(projectId, noteId), onSuccess: () => invalidate('research-notes') }),

    createTask: useMutation({ mutationFn: (payload) => researchWorkspaceService.createTask(projectId, payload), onSuccess: () => invalidate('research-tasks') }),
    updateTaskStatus: useMutation({
      mutationFn: ({ taskId, payload }) => researchWorkspaceService.updateTaskStatus(projectId, taskId, payload),
      onSuccess: () => invalidate('research-tasks'),
    }),
    removeTask: useMutation({ mutationFn: (taskId) => researchWorkspaceService.removeTask(projectId, taskId), onSuccess: () => invalidate('research-tasks') }),

    createMilestone: useMutation({ mutationFn: (payload) => researchWorkspaceService.createMilestone(projectId, payload), onSuccess: () => invalidate('research-milestones') }),
    updateMilestone: useMutation({
      mutationFn: ({ milestoneId, payload }) => researchWorkspaceService.updateMilestone(projectId, milestoneId, payload),
      onSuccess: () => invalidate('research-milestones'),
    }),
    removeMilestone: useMutation({
      mutationFn: (milestoneId) => researchWorkspaceService.removeMilestone(projectId, milestoneId),
      onSuccess: () => invalidate('research-milestones'),
    }),
  };
}
