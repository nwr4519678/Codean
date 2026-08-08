import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import { AnnouncementResponse, PagedList } from '@platform/contracts';

export const announcementsApi = {
  list: async (params?: {
    courseId?: number;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedList<AnnouncementResponse>> => {
    const res = await apiClient.get<PagedList<AnnouncementResponse>>(
      API_URLS.COMMUNICATION.ANNOUNCEMENTS,
      { params }
    );
    return res.data;
  },

  create: async (payload: {
    courseId?: number;
    title: string;
    body: string;
    isPinned?: boolean;
  }): Promise<AnnouncementResponse> => {
    const res = await apiClient.post<AnnouncementResponse>(
      API_URLS.COMMUNICATION.ANNOUNCEMENTS,
      payload
    );
    return res.data;
  },

  delete: async (id: number | string): Promise<void> => {
    await apiClient.delete(`${API_URLS.COMMUNICATION.ANNOUNCEMENTS}/${id}`);
  },
};
