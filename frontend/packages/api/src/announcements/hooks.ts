import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { announcementsApi } from './api';
import { announcementQueryKeys } from './keys';

export const useAnnouncements = (params?: { courseId?: number; pageNumber?: number; pageSize?: number }) =>
  useQuery({
    queryKey: announcementQueryKeys.list(params),
    queryFn: () => announcementsApi.list(params),
  });

export const useCreateAnnouncement = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: announcementsApi.create,
    onSuccess: () => qc.invalidateQueries({ queryKey: announcementQueryKeys.all }),
  });
};

export const useDeleteAnnouncement = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: announcementsApi.delete,
    onSuccess: () => qc.invalidateQueries({ queryKey: announcementQueryKeys.all }),
  });
};
