export const announcementQueryKeys = {
  all: ['announcements'] as const,
  list: (params?: Record<string, any>) => [...announcementQueryKeys.all, 'list', params] as const,
};
