'use client';

import React from 'react';
import { CreditCard, DollarSign } from 'lucide-react';

export default function AdminSubscriptionsPage() {
  return (
    <div className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-7xl space-y-8">
        <div className="flex items-center justify-between border-b border-border pb-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-500 text-white shadow-lg">
              <CreditCard className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Subscriptions & Revenue</h1>
              <p className="text-sm text-muted-foreground">Monitor platform revenue, active subscriptions, and Paymob transactions</p>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
          <div className="rounded-2xl border border-border bg-card p-6">
            <span className="text-xs font-semibold text-muted-foreground uppercase">Monthly Recurring Revenue (MRR)</span>
            <p className="mt-2 text-3xl font-extrabold text-emerald-500">$34,800</p>
          </div>
          <div className="rounded-2xl border border-border bg-card p-6">
            <span className="text-xs font-semibold text-muted-foreground uppercase">Active Pro Subscribers</span>
            <p className="mt-2 text-3xl font-extrabold">1,200</p>
          </div>
          <div className="rounded-2xl border border-border bg-card p-6">
            <span className="text-xs font-semibold text-muted-foreground uppercase">Churn Rate</span>
            <p className="mt-2 text-3xl font-extrabold text-primary">1.2%</p>
          </div>
        </div>
      </div>
    </div>
  );
}
