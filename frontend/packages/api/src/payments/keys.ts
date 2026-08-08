export const paymentsQueryKeys = {
  all: ['payments'] as const,
  plans: () => [...paymentsQueryKeys.all, 'plans'] as const,
  subscription: () => [...paymentsQueryKeys.all, 'subscription'] as const,
  payments: (params?: Record<string, any>) => [...paymentsQueryKeys.all, 'history', params] as const,
};
