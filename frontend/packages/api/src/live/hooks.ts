import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { liveApi } from './api';
import { liveQueryKeys } from './keys';

export const useMyLiveSessions = () =>
  useQuery({
    queryKey: liveQueryKeys.list(),
    queryFn: liveApi.getMySessions,
  });

export const useLiveSession = (id: number | string) =>
  useQuery({
    queryKey: liveQueryKeys.detail(id),
    queryFn: () => liveApi.getById(id),
    enabled: !!id,
  });

export const useScheduleLiveSession = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: liveApi.schedule,
    onSuccess: () => qc.invalidateQueries({ queryKey: liveQueryKeys.all }),
  });
};

export const useCancelLiveSession = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: liveApi.cancel,
    onSuccess: () => qc.invalidateQueries({ queryKey: liveQueryKeys.all }),
  });
};

export const useAttendLiveSession = () =>
  useMutation({
    mutationFn: liveApi.attend,
  });
