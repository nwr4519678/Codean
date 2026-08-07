export const learningKeys = {
  all: ['learning'] as const,
  progress: (courseId: number) => [...learningKeys.all, 'progress', courseId] as const,
  enrolled: () => [...learningKeys.all, 'enrolled'] as const,
  exam: (examId: number) => [...learningKeys.all, 'exam', examId] as const,
};
