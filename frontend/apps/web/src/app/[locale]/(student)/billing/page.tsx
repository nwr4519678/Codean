'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { CheckCircle, CreditCard, Loader2 } from 'lucide-react';
import { useMySubscription } from '@platform/api';

export default function StudentBillingPage() {
  const { data: subscriptions, isLoading, isError } = useMySubscription();
  const activeSubscription = subscriptions?.find((subscription) => subscription.status.toLowerCase() === 'active');

  return (
    <div className="container mx-auto max-w-4xl space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      <PageHeader title="Billing & Subscriptions" description="Review your subscription and manage access to learning content." />

      {isLoading ? <div className="flex min-h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div> : isError ? (
        <p role="alert" className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">Billing information could not be loaded. Please try again.</p>
      ) : activeSubscription ? (
        <section className="space-y-6 rounded-3xl border border-border/70 bg-card p-8 shadow-xl">
          <div className="flex flex-col justify-between gap-4 border-b border-border/60 pb-6 sm:flex-row sm:items-start">
            <div>
              <span className="mb-2 inline-flex items-center gap-1 rounded-full bg-emerald-500/10 px-3 py-1 text-xs font-bold text-emerald-600 dark:text-emerald-400"><CheckCircle className="h-3.5 w-3.5" /> Active subscription</span>
              <h2 className="text-2xl font-bold">{activeSubscription.planName}</h2>
              <p className="text-xs text-muted-foreground">Access expires {new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(activeSubscription.accessExpiresAt))}</p>
            </div>
            <Link href="/pricing" className="rounded-xl border border-border bg-muted px-4 py-2 text-xs font-semibold text-foreground transition-colors hover:bg-background">View plans</Link>
          </div>
          <div className="flex items-start gap-3 rounded-2xl bg-muted/40 p-4 text-sm text-muted-foreground"><CreditCard className="mt-0.5 h-5 w-5 shrink-0 text-primary" /><p>Payments are handled securely by Paymob. Payment-method and invoice details are not exposed by the current API.</p></div>
        </section>
      ) : (
        <section className="rounded-3xl border border-dashed border-border bg-card p-10 text-center"><CreditCard className="mx-auto mb-4 h-10 w-10 text-muted-foreground" /><h2 className="text-lg font-semibold">No active subscription</h2><p className="mt-2 text-sm text-muted-foreground">Choose a plan to unlock subscription-based learning access.</p><Link href="/pricing" className="mt-6 inline-flex rounded-xl bg-primary px-5 py-3 text-sm font-semibold text-white transition-colors hover:bg-primary/90">Browse plans</Link></section>
      )}
    </div>
  );
}
