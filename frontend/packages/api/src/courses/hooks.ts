import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from './api';
import { coursesQueryKeys } from './keys';

export const useCourses = (params?: { pageNumber?: number; pageSize?: number; search?: string; category?: string; teacherId?: number; isPublished?: boolean }) => {
  return useQuery({
    queryKey: coursesQueryKeys.list(params),
    queryFn: () => coursesApi.getCourses(params),
  });
};

export const useCourseDetail = (id: number | string) => {
  return useQuery({
    queryKey: coursesQueryKeys.detail(id),
    queryFn: () => coursesApi.getCourseById(id),
    enabled: !!id,
  });
};

export const useTrackProgress = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: coursesApi.trackProgress,
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: coursesQueryKeys.all });
    },
  });
};

export const useCreateCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: coursesApi.createCourse,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: coursesQueryKeys.all }),
  });
};

export const useUpdateCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: number | string; payload: Parameters<typeof coursesApi.updateCourse>[1] }) => coursesApi.updateCourse(id, payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: coursesQueryKeys.all });
      queryClient.invalidateQueries({ queryKey: coursesQueryKeys.detail(variables.id) });
    },
  });
};

export const usePublishCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number | string) => coursesApi.publishCourse(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: coursesQueryKeys.all });
    },
  });
};

export const useArchiveCourse = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: number | string) => coursesApi.archiveCourse(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: coursesQueryKeys.all });
    },
  });
};
