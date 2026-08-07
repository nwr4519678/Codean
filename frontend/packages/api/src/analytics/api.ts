import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import {
  PlatformOverviewResponse,
  AuditLogResponse,
  PagedList,
} from '@platform/contracts';

export const analyticsApi = {
  getOverview: async (): Promise<PlatformOverviewResponse> => {
    const res = await apiClient.get<PlatformOverviewResponse>(API_URLS.ANALYTICS.OVERVIEW);
    return res.data;
  },

  getAuditLogs: async (params?: { pageNumber?: number; pageSize?: number }): Promise<PagedList<AuditLogResponse>> => {
    const res = await apiClient.get<PagedList<AuditLogResponse>>(API_URLS.ANALYTICS.AUDIT_LOGS, { params });
    return res.data;
  },
};
