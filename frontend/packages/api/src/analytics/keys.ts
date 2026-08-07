export const analyticsQueryKeys = {
  all: ['analytics'] as const,
  overview: () => [...analyticsQueryKeys.all, 'overview'] as const,
  auditLogs: (params?: Record<string, any>) => [...analyticsQueryKeys.all, 'auditLogs', params] as const,
};
