export const modulesQueryKeys = {
  all: ['modules'] as const,
  byCourse: (courseId: number | string) => [...modulesQueryKeys.all, 'course', courseId] as const,
};
