import { apiClient } from '../client';
import { API_URLS } from '@platform/config';
import {
  CheckoutInitResponse,
  PaymentWithInvoiceResponse,
  PagedList,
} from '@platform/contracts';

/** Shapes returned by the existing Commerce controllers. */
export interface SubscriptionPlan {
  id: number;
  name: string;
  price: number;
  durationMonths: number;
  description: string;
  isActive: boolean;
}

export interface SubscriptionSummary {
  id: number;
  planName: string;
  status: string;
  startDate: string;
  endDate: string;
  accessExpiresAt: string;
}

export const paymentsApi = {
  getSubscriptionPlans: async (): Promise<SubscriptionPlan[]> => {
    const res = await apiClient.get<SubscriptionPlan[]>(API_URLS.COMMERCE.PLANS, { params: { isActive: true } });
    return res.data;
  },

  getMySubscription: async (): Promise<SubscriptionSummary[]> => {
    const res = await apiClient.get<SubscriptionSummary[]>(API_URLS.COMMERCE.MY_SUBSCRIPTION);
    return res.data;
  },

  initCheckout: async (planId: number): Promise<CheckoutInitResponse> => {
    const res = await apiClient.post<CheckoutInitResponse>(API_URLS.COMMERCE.CHECKOUT, { planId });
    return res.data;
  },

  getMyPayments: async (params?: { pageNumber?: number; pageSize?: number }): Promise<PagedList<PaymentWithInvoiceResponse>> => {
    const res = await apiClient.get<PagedList<PaymentWithInvoiceResponse>>('/api/payments/me', { params });
    return res.data;
  },
};
