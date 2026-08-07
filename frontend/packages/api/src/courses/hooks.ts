import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coursesApi } from './api';
import { coursesQueryKeys } from './keys';

export const useCourses = (params?: { pageNumber?: number; pageSize?: number; search?: string; category?: string }) => {
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
