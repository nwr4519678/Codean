'use client';

import React from 'react';
import { Users, BookOpen, CreditCard, ShieldAlert, Activity, FileText } from 'lucide-react';

export default function AdminOverviewPage() {
  return (
    <div className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-7xl space-y-8">
        <div className="flex items-center justify-between border-b border-border pb-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-destructive text-white shadow-lg">
              <ShieldAlert className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Platform Admin Console</h1>
              <p className="text-sm text-muted-foreground">System telemetry, user management, and security audit trail</p>
            </div>
          </div>
          <span className="inline-flex items-center gap-2 rounded-full bg-emerald-500/10 px-3 py-1 text-xs font-semibold text-emerald-500">
            <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" />
            System Healthy
          </span>
        </div>

        {/* Metrics Grid */}
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-muted-foreground">Total Users</span>
              <Users className="h-5 w-5 text-primary" />
            </div>
            <p className="mt-4 text-3xl font-bold">12,450</p>
            <span className="mt-1 text-xs text-emerald-500">+120 today</span>
          </div>

          <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-muted-foreground">Active Courses</span>
              <BookOpen className="h-5 w-5 text-accent" />
            </div>
            <p className="mt-4 text-3xl font-bold">54</p>
            <span className="mt-1 text-xs text-muted-foreground">4 draft submissions</span>
          </div>

          <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-muted-foreground">Monthly Revenue</span>
              <CreditCard className="h-5 w-5 text-emerald-500" />
            </div>
            <p className="mt-4 text-3xl font-bold">$42,800</p>
            <span className="mt-1 text-xs text-emerald-500">+15.4% vs last month</span>
          </div>

          <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium text-muted-foreground">Audit Logs Today</span>
              <FileText className="h-5 w-5 text-purple-500" />
            </div>
            <p className="mt-4 text-3xl font-bold">1,820</p>
            <span className="mt-1 text-xs text-muted-foreground">0 critical warnings</span>
          </div>
        </div>
      </div>
    </div>
  );
}
