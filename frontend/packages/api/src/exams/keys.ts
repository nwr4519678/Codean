export const examsQueryKeys = {
  all: ['exams'] as const,
  list: (params?: Record<string, any>) => [...examsQueryKeys.all, 'list', params] as const,
  detail: (id: number | string) => [...examsQueryKeys.all, 'detail', id] as const,
  attempt: (id: number | string) => [...examsQueryKeys.all, 'attempt', id] as const,
  result: (id: number | string) => [...examsQueryKeys.all, 'result', id] as const,
};
