'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { CreditCard, ShieldCheck, Lock, ArrowRight, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

export default function CheckoutPage() {
  const router = useRouter();
  const [isProcessing, setIsProcessing] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState<'card' | 'wallet'>('card');

  const handlePayment = async () => {
    setIsProcessing(true);
    toast.info('Connecting to Paymob payment gateway...');

    // Simulate Paymob integration token generation & redirect
    setTimeout(() => {
      setIsProcessing(false);
      router.push('/checkout/success');
    }, 2000);
  };

  return (
    <div className="container mx-auto max-w-4xl space-y-8 px-4 py-12 sm:px-6 lg:px-8">
      <div className="text-center space-y-2">
        <h1 className="text-3xl font-extrabold tracking-tight">Complete Your Subscription</h1>
        <p className="text-sm text-muted-foreground">Secure payment powered by Paymob</p>
      </div>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
        {/* Order Summary */}
        <div className="rounded-3xl border border-border/80 bg-card p-6 space-y-6">
          <h2 className="text-lg font-bold">Order Summary</h2>

          <div className="flex items-center justify-between border-b border-border/60 pb-4">
            <div>
              <h3 className="font-semibold">Pro Student Subscription</h3>
              <p className="text-xs text-muted-foreground">Billed monthly • Unlimited courses & judge</p>
            </div>
            <span className="text-xl font-bold">$29/mo</span>
          </div>

          <div className="space-y-2 text-sm text-muted-foreground">
            <div className="flex justify-between">
              <span>Subtotal</span>
              <span>$29.00</span>
            </div>
            <div className="flex justify-between">
              <span>Tax</span>
              <span>$0.00</span>
            </div>
            <div className="flex justify-between font-bold text-foreground pt-2 border-t border-border/40">
              <span>Total Due</span>
              <span>$29.00</span>
            </div>
          </div>

          <div className="flex items-center gap-2 text-xs text-muted-foreground pt-2">
            <ShieldCheck className="h-4 w-4 text-emerald-500" />
            <span>30-day money-back guarantee</span>
          </div>
        </div>

        {/* Payment Method Selector & Submit */}
        <div className="rounded-3xl border border-border/80 bg-card p-6 space-y-6">
          <h2 className="text-lg font-bold">Select Payment Method</h2>

          <div className="space-y-3">
            <label
              onClick={() => setPaymentMethod('card')}
              className={`flex items-center justify-between rounded-xl border p-4 cursor-pointer transition-all ${
                paymentMethod === 'card'
                  ? 'border-primary bg-primary/5 ring-2 ring-primary/20'
                  : 'border-border bg-background'
              }`}
            >
              <div className="flex items-center gap-3">
                <CreditCard className="h-5 w-5 text-primary" />
                <span className="text-sm font-semibold">Credit / Debit Card (Visa/MasterCard)</span>
              </div>
              <input type="radio" checked={paymentMethod === 'card'} readOnly />
            </label>

            <label
              onClick={() => setPaymentMethod('wallet')}
              className={`flex items-center justify-between rounded-xl border p-4 cursor-pointer transition-all ${
                paymentMethod === 'wallet'
                  ? 'border-primary bg-primary/5 ring-2 ring-primary/20'
                  : 'border-border bg-background'
              }`}
            >
              <div className="flex items-center gap-3">
                <Lock className="h-5 w-5 text-accent" />
                <span className="text-sm font-semibold">Mobile Wallet (Vodafone / Orange / Etisalat Cash)</span>
              </div>
              <input type="radio" checked={paymentMethod === 'wallet'} readOnly />
            </label>
          </div>

          <button
            onClick={handlePayment}
            disabled={isProcessing}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-base font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95 disabled:opacity-50"
          >
            {isProcessing ? (
              <Loader2 className="h-5 w-5 animate-spin" />
            ) : (
              <>
                Pay $29.00 Now
                <ArrowRight className="h-4 w-4" />
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
