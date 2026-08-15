import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import { LiveSessionResponse } from '@platform/contracts';

export const liveApi = {
  getUpcoming: async (): Promise<LiveSessionResponse[]> => {
    const res = await apiClient.get<LiveSessionResponse[]>(API_URLS.LIVE_SESSIONS.UPCOMING);
    return res.data;
  },
  getMySessions: async (): Promise<LiveSessionResponse[]> => {
    const res = await apiClient.get<LiveSessionResponse[]>(`${API_URLS.LIVE_SESSIONS.LIST}/me`);
    return res.data;
  },

  getById: async (id: number | string): Promise<LiveSessionResponse> => {
    const res = await apiClient.get<LiveSessionResponse>(API_URLS.LIVE_SESSIONS.DETAIL(id));
    return res.data;
  },

  schedule: async (payload: {
    courseId?: number;
    title: string;
    startTime: string;
    endTime?: string;
    providerId: number;
  }): Promise<LiveSessionResponse> => {
    const res = await apiClient.post<LiveSessionResponse>(API_URLS.LIVE_SESSIONS.LIST, payload);
    return res.data;
  },

  cancel: async (id: number | string): Promise<void> => {
    await apiClient.post(`${API_URLS.LIVE_SESSIONS.DETAIL(id)}/cancel`);
  },

  attend: async (id: number | string): Promise<void> => {
    await apiClient.post(`${API_URLS.LIVE_SESSIONS.DETAIL(id)}/attend`);
  },
};
