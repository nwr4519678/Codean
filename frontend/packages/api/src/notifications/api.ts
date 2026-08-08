import { apiClient } from '../client';
import { API_URLS } from '@platform/config';

export interface NotificationItem {
  id: number;
  title: string;
  body: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

export interface PagedNotifications {
  items: NotificationItem[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasNextPage: boolean;
}

export const notificationsApi = {
  getAll: async (params?: { unreadOnly?: boolean; pageNumber?: number; pageSize?: number }): Promise<PagedNotifications> => {
    const res = await apiClient.get<PagedNotifications>(API_URLS.COMMUNICATION.NOTIFICATIONS, { params });
    return res.data;
  },

  markAsRead: async (notificationId: number) => {
    const res = await apiClient.post(API_URLS.COMMUNICATION.MARK_NOTIFICATION_READ(notificationId));
    return res.data;
  },

  markAllAsRead: async () => {
    const res = await apiClient.post('/api/notifications/me/read-all');
    return res.data;
  },

  getUnreadCount: async (): Promise<number> => {
    const res = await apiClient.get<{ unreadCount: number }>('/api/notifications/me/unread-count');
    return res.data.unreadCount;
  },
};
