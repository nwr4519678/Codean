import React from 'react';
import Link from 'next/link';
import { CheckCircle2, ArrowRight } from 'lucide-react';

export default function CheckoutSuccessPage() {
  return (
    <div className="flex min-h-[500px] flex-col items-center justify-center p-4 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-500/10 text-emerald-500 mb-6 shadow-inner">
        <CheckCircle2 className="h-10 w-10" />
      </div>

      <h1 className="text-3xl font-extrabold tracking-tight">Payment Successful! 🎉</h1>
      <p className="mt-2 text-muted-foreground max-w-md text-sm">
        Thank you for your subscription. Your Pro account features are now unlocked.
      </p>

      <Link
        href="/dashboard"
        className="mt-8 inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95"
      >
        Go to Student Dashboard
        <ArrowRight className="h-4 w-4" />
      </Link>
    </div>
  );
}
