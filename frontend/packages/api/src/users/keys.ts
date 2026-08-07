export const usersQueryKeys = {
  all: ['users'] as const,
  studentProfile: () => [...usersQueryKeys.all, 'studentProfile'] as const,
  teacherProfile: () => [...usersQueryKeys.all, 'teacherProfile'] as const,
  list: (params?: Record<string, any>) => [...usersQueryKeys.all, 'list', params] as const,
};
