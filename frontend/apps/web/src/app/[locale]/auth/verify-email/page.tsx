import React from 'react';
import Link from 'next/link';
import { CheckCircle2, ArrowRight } from 'lucide-react';

export default function VerifyEmailPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4 py-12 text-center">
      <div className="w-full max-w-md space-y-6 rounded-3xl border border-border/60 bg-card p-8 shadow-2xl">
        <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-500/10 text-emerald-500 shadow-inner">
          <CheckCircle2 className="h-10 w-10" />
        </div>

        <h2 className="text-2xl font-bold tracking-tight">Email Verified! 🎉</h2>
        <p className="text-sm text-muted-foreground">
          Your email address has been successfully verified. You now have full access to the Platform.
        </p>

        <Link
          href="/dashboard"
          className="inline-flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95"
        >
          Continue to Dashboard
          <ArrowRight className="h-4 w-4" />
        </Link>
      </div>
    </div>
  );
}
