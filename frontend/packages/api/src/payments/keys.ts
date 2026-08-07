export const paymentsQueryKeys = {
  all: ['payments'] as const,
  plans: () => [...paymentsQueryKeys.all, 'plans'] as const,
  subscription: () => [...paymentsQueryKeys.all, 'subscription'] as const,
};
