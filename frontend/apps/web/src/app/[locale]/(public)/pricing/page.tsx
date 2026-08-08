'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { Check, Loader2 } from 'lucide-react';
import { useSubscriptionPlans } from '@platform/api';

export default function PricingPage() {
  const { data: plans, isLoading, isError } = useSubscriptionPlans();

  return (
    <div className="container mx-auto space-y-12 px-4 py-12 sm:px-6 lg:px-8">
      <PageHeader
        title="Flexible Pricing Plans"
        description="Choose the perfect tier for your learning goals. Upgrade or cancel anytime."
      />

      {isLoading ? <div className="flex min-h-64 items-center justify-center"><Loader2 className="h-6 w-6 animate-spin text-primary" /></div> : isError ? (
        <p role="alert" className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">Plans are unavailable right now. Please try again later.</p>
      ) : plans?.length ? <div className="mx-auto grid max-w-6xl grid-cols-1 gap-8 md:grid-cols-3">
        {plans.map((plan) => (
          <div
            key={plan.name}
            className={`flex flex-col justify-between rounded-3xl border p-8 transition-all ${
              'border-border/70 bg-card shadow-sm hover:border-primary/50 hover:shadow-md'
            }`}
          >
            <div className="space-y-6">
              <div>
                <h3 className="text-2xl font-bold">{plan.name}</h3>
                <p className="mt-1 text-xs text-muted-foreground">{plan.description}</p>
              </div>

              <div className="flex items-baseline gap-1">
                <span className="text-5xl font-extrabold">{new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EGP', maximumFractionDigits: 0 }).format(plan.price)}</span>
                <span className="text-sm text-muted-foreground">/ {plan.durationMonths} month{plan.durationMonths === 1 ? '' : 's'}</span>
              </div>

              <ul className="space-y-3 text-xs text-muted-foreground border-t border-border/40 pt-6">
                <li className="flex items-center gap-2 font-medium text-foreground">
                  <Check className="h-4 w-4 text-emerald-500 shrink-0" />
                  <span>Access for {plan.durationMonths} month{plan.durationMonths === 1 ? '' : 's'}</span>
                </li>
              </ul>
            </div>

            <Link
              href={`/checkout?planId=${plan.id}`}
              className="mt-8 flex w-full items-center justify-center rounded-xl bg-primary px-6 py-3.5 text-sm font-semibold text-white shadow-lg shadow-primary/25 transition-all hover:bg-primary/90 active:scale-95"
            >
              Choose plan
            </Link>
          </div>
        ))}
      </div> : <div className="rounded-2xl border border-dashed border-border p-10 text-center text-sm text-muted-foreground">No plans are currently available.</div>}
    </div>
  );
}
