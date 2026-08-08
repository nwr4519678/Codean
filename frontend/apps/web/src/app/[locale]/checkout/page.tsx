'use client';

import React from 'react';
import { useSearchParams } from 'next/navigation';
import { ShieldCheck, ArrowRight, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { useInitCheckout, useSubscriptionPlans } from '@platform/api';

export default function CheckoutPage() {
  const searchParams = useSearchParams();
  const planId = Number(searchParams.get('planId'));
  const { data: plans, isLoading: isLoadingPlans } = useSubscriptionPlans();
  const checkout = useInitCheckout();
  const plan = plans?.find((candidate) => candidate.id === planId);

  const handlePayment = async () => {
    if (!plan) return;
    try {
      const session = await checkout.mutateAsync({ planId: plan.id });
      const checkoutUrl = session.checkoutUrl;
      if (!checkoutUrl || !/^https?:\/\//i.test(checkoutUrl)) throw new Error('Invalid checkout URL');
      window.location.assign(checkoutUrl);
    } catch {
      toast.error('Unable to start secure checkout. Please try again.');
    }
  };

  return (
    <div className="container mx-auto max-w-4xl space-y-8 px-4 py-12 sm:px-6 lg:px-8">
      <div className="text-center space-y-2">
        <h1 className="text-3xl font-extrabold tracking-tight">Complete Your Subscription</h1>
        <p className="text-sm text-muted-foreground">Secure payment powered by Paymob</p>
      </div>

      {isLoadingPlans ? <div className="flex min-h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div> : !plan ? <p role="alert" className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">Select a valid plan before starting checkout.</p> : <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
        {/* Order Summary */}
        <div className="rounded-3xl border border-border/80 bg-card p-6 space-y-6">
          <h2 className="text-lg font-bold">Order Summary</h2>

          <div className="flex items-center justify-between border-b border-border/60 pb-4">
            <div>
              <h3 className="font-semibold">{plan.name}</h3>
              <p className="text-xs text-muted-foreground">{plan.description}</p>
            </div>
            <span className="text-xl font-bold">{new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EGP' }).format(plan.price)}</span>
          </div>

          <div className="space-y-2 text-sm text-muted-foreground">
            <div className="flex justify-between">
              <span>Subtotal</span>
              <span>{new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EGP' }).format(plan.price)}</span>
            </div>
            <div className="flex justify-between">
              <span>Tax</span>
              <span>Calculated by payment provider</span>
            </div>
            <div className="flex justify-between font-bold text-foreground pt-2 border-t border-border/40">
              <span>Total Due</span>
              <span>{new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EGP' }).format(plan.price)}</span>
            </div>
          </div>

          <div className="flex items-center gap-2 text-xs text-muted-foreground pt-2">
            <ShieldCheck className="h-4 w-4 text-emerald-500" />
            <span>30-day money-back guarantee</span>
          </div>
        </div>

        {/* Paymob hosts payment-method selection on its secure checkout page. */}
        <div className="rounded-3xl border border-border/80 bg-card p-6 space-y-6">
          <h2 className="text-lg font-bold">Secure payment</h2>
          <p className="text-sm text-muted-foreground">You will choose your payment method securely on Paymob. We never collect card or wallet details on this site.</p>

          <button
            onClick={handlePayment}
            disabled={checkout.isPending}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-base font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95 disabled:opacity-50"
          >
            {checkout.isPending ? (
              <Loader2 className="h-5 w-5 animate-spin" />
            ) : (
              <>
                Continue to Paymob
                <ArrowRight className="h-4 w-4" />
              </>
            )}
          </button>
        </div>
      </div>}
    </div>
  );
}
