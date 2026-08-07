import { useQuery } from '@tanstack/react-query';
import { analyticsApi } from './api';
import { analyticsQueryKeys } from './keys';

export const usePlatformOverview = () => {
  return useQuery({
    queryKey: analyticsQueryKeys.overview(),
    queryFn: analyticsApi.getOverview,
  });
};

export const useAuditLogs = (params?: { pageNumber?: number; pageSize?: number }) => {
  return useQuery({
    queryKey: analyticsQueryKeys.auditLogs(params),
    queryFn: () => analyticsApi.getAuditLogs(params),
  });
};
