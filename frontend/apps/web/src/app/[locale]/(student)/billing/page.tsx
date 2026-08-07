'use client';

import React from 'react';
import Link from 'next/link';
import { PageHeader } from '@/components/shared/PageHeader';
import { CreditCard, CheckCircle, Zap, ExternalLink } from 'lucide-react';

export default function StudentBillingPage() {
  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8 max-w-4xl">
      <PageHeader title="Billing & Subscriptions" description="Manage your current plan, payment methods, and invoice history." />

      <div className="rounded-3xl border border-border/70 bg-card p-8 shadow-xl space-y-6">
        <div className="flex items-center justify-between border-b border-border/60 pb-6">
          <div>
            <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/10 px-3 py-1 text-xs font-bold text-emerald-500 mb-2">
              <CheckCircle className="h-3.5 w-3.5" /> Active Plan
            </span>
            <h2 className="text-2xl font-bold">Pro Student Subscription</h2>
            <p className="text-xs text-muted-foreground">$29 / month • Renews on Sep 1, 2026</p>
          </div>
          <Link
            href="/checkout"
            className="rounded-xl border border-border bg-muted px-4 py-2 text-xs font-semibold text-foreground hover:bg-background"
          >
            Upgrade Plan
          </Link>
        </div>

        <div className="space-y-3">
          <h3 className="text-sm font-bold">Payment Methods</h3>
          <div className="flex items-center justify-between rounded-2xl border border-border/60 bg-muted/20 p-4">
            <div className="flex items-center gap-3">
              <CreditCard className="h-5 w-5 text-primary" />
              <div>
                <p className="text-xs font-semibold">Visa ending in 4242</p>
                <p className="text-[10px] text-muted-foreground">Expires 12/28</p>
              </div>
            </div>
            <span className="text-xs font-semibold text-primary">Default</span>
          </div>
        </div>
      </div>
    </div>
  );
}
