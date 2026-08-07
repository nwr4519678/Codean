export const coursesQueryKeys = {
  all: ['courses'] as const,
  list: (params?: Record<string, any>) => [...coursesQueryKeys.all, 'list', params] as const,
  detail: (id: number | string) => [...coursesQueryKeys.all, 'detail', id] as const,
  progress: (courseId: number | string) => [...coursesQueryKeys.all, 'progress', courseId] as const,
};
