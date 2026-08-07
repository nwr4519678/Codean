export const notificationKeys = {
  all: ['notifications'] as const,
  list: (filters?: object) => [...notificationKeys.all, 'list', filters] as const,
  unreadCount: () => [...notificationKeys.all, 'unread-count'] as const,
};
