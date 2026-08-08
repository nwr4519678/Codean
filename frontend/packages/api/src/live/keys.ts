export const liveQueryKeys = {
  all: ['live'] as const,
  list: (params?: Record<string, any>) => [...liveQueryKeys.all, 'list', params] as const,
  detail: (id: number | string) => [...liveQueryKeys.all, 'detail', id] as const,
};
