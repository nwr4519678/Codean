import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { modulesApi } from './api';
import { modulesQueryKeys } from './keys';

export const useCourseModules = (courseId: number | string) =>
  useQuery({
    queryKey: modulesQueryKeys.byCourse(courseId),
    queryFn: () => modulesApi.getByCourse(courseId),
    enabled: !!courseId,
  });

export const useCreateModule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: modulesApi.create,
    onSuccess: (_, vars) => qc.invalidateQueries({ queryKey: modulesQueryKeys.byCourse(vars.courseId) }),
  });
};

export const useUpdateModule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number | string; payload: Parameters<typeof modulesApi.update>[1] }) =>
      modulesApi.update(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: modulesQueryKeys.all }),
  });
};

export const useDeleteModule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: modulesApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: modulesQueryKeys.all }),
  });
};

export const useCreateLesson = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: modulesApi.createLesson,
    onSuccess: () => qc.invalidateQueries({ queryKey: modulesQueryKeys.all }),
  });
};

export const useUpdateLesson = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number | string; payload: Parameters<typeof modulesApi.updateLesson>[1] }) =>
      modulesApi.updateLesson(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: modulesQueryKeys.all }),
  });
};

export const useDeleteLesson = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: modulesApi.deleteLesson,
    onSuccess: () => qc.invalidateQueries({ queryKey: modulesQueryKeys.all }),
  });
};
