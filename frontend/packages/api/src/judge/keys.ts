export const judgeQueryKeys = {
  all: ['judge'] as const,
  challenges: (params?: Record<string, any>) => [...judgeQueryKeys.all, 'challenges', params] as const,
  challengeDetail: (id: number | string) => [...judgeQueryKeys.all, 'challenge', id] as const,
  submissionStatus: (id: number | string) => [...judgeQueryKeys.all, 'submission', id] as const,
};
