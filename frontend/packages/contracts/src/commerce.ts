import { PaymentStatus, SubscriptionTier } from './enums';

export interface SubscriptionPlanResponse {
  id: number;
  name: string;
  tier: SubscriptionTier;
  priceMonthly: number;
  priceYearly: number;
  features: string[];
  isPopular?: boolean;
}

export interface SubscriptionResponse {
  id: number;
  userId: number;
  planId: number;
  planName: string;
  status: 'Active' | 'Cancelled' | 'Expired';
  startDate: string;
  endDate: string;
  autoRenew: boolean;
}

export interface CheckoutInitResponse {
  orderId: string;
  paymentToken: string;
  checkoutUrl: string;
  iframeUrl?: string;
}

export interface PaymentResponse {
  id: number;
  orderId: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  provider: string;
  createdAt: string;
}
