import { apiClient } from '../client';
import { API_URLS } from '@platform/config';

export const notificationsApi = {
  getAll: async (params?: { isRead?: boolean; page?: number; pageSize?: number }) => {
    const res = await apiClient.get(API_URLS.COMMUNICATION.NOTIFICATIONS, { params });
    return res.data;
  },

  markAsRead: async (notificationId: number) => {
    const res = await apiClient.patch(API_URLS.COMMUNICATION.MARK_NOTIFICATION_READ(notificationId));
    return res.data;
  },

  markAllAsRead: async () => {
    const res = await apiClient.patch(`${API_URLS.COMMUNICATION.NOTIFICATIONS}/read-all`);
    return res.data;
  },

  getUnreadCount: async (): Promise<number> => {
    const res = await apiClient.get(`${API_URLS.COMMUNICATION.NOTIFICATIONS}/unread-count`);
    return res.data;
  },
};
