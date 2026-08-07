import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import {
  SubscriptionPlanResponse,
  SubscriptionResponse,
  CheckoutInitResponse,
} from '@platform/contracts';

export const paymentsApi = {
  getSubscriptionPlans: async (): Promise<SubscriptionPlanResponse[]> => {
    const res = await apiClient.get<SubscriptionPlanResponse[]>(API_URLS.COMMERCE.PLANS);
    return res.data;
  },

  getMySubscription: async (): Promise<SubscriptionResponse> => {
    const res = await apiClient.get<SubscriptionResponse>(API_URLS.COMMERCE.MY_SUBSCRIPTION);
    return res.data;
  },

  initCheckout: async (planId: number, isYearly: boolean): Promise<CheckoutInitResponse> => {
    const res = await apiClient.post<CheckoutInitResponse>(API_URLS.COMMERCE.CHECKOUT, { planId, isYearly });
    return res.data;
  },
};
