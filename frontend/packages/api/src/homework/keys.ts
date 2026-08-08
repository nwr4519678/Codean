export const homeworkQueryKeys = {
  all: ['homework'] as const,
  list: (params?: Record<string, any>) => [...homeworkQueryKeys.all, 'list', params] as const,
  detail: (id: number | string) => [...homeworkQueryKeys.all, 'detail', id] as const,
  submissions: (id: number | string) => [...homeworkQueryKeys.all, 'submissions', id] as const,
};
