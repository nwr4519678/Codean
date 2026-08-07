import { useQuery, useMutation } from '@tanstack/react-query';
import { paymentsApi } from './api';
import { paymentsQueryKeys } from './keys';

export const useSubscriptionPlans = () => {
  return useQuery({
    queryKey: paymentsQueryKeys.plans(),
    queryFn: paymentsApi.getSubscriptionPlans,
  });
};

export const useMySubscription = () => {
  return useQuery({
    queryKey: paymentsQueryKeys.subscription(),
    queryFn: paymentsApi.getMySubscription,
  });
};

export const useInitCheckout = () => {
  return useMutation({
    mutationFn: ({ planId, isYearly }: { planId: number; isYearly: boolean }) =>
      paymentsApi.initCheckout(planId, isYearly),
  });
};
