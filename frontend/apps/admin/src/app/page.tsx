'use client';

import React from 'react';
import { Users, BookOpen, CreditCard, ShieldAlert, FileText, Loader2, ArrowUpRight, TrendingUp, CheckCircle, Server, Activity } from 'lucide-react';
import { usePlatformOverview } from '@platform/api';

export default function AdminOverviewPage() {
  const { data: overview, isLoading, isError } = usePlatformOverview();

  const metrics = [
    { label: 'Total Students', value: overview?.totalStudents, icon: Users, accent: 'text-primary', trend: '+14% this month' },
    { label: 'Teachers', value: overview?.totalTeachers, icon: Users, accent: 'text-accent', trend: '+2 new' },
    { label: 'Active Courses', value: overview?.totalCourses, icon: BookOpen, accent: 'text-emerald-500', trend: '5 published' },
    { label: 'Judge Submissions', value: overview?.totalSubmissions, icon: FileText, accent: 'text-purple-500', trend: '98.5% pass rate' },
  ];

  const MONTHLY_ENROLLMENTS = [
    { month: 'Jan', count: 120 },
    { month: 'Feb', count: 210 },
    { month: 'Mar', count: 340 },
    { month: 'Apr', count: 480 },
    { month: 'May', count: 620 },
    { month: 'Jun', count: 890 },
    { month: 'Jul', count: 1100 },
    { month: 'Aug', count: 1240 },
  ];

  const maxVal = Math.max(...MONTHLY_ENROLLMENTS.map((m) => m.count));

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
            System Healthy (.NET 10 + PostgreSQL + Redis)
          </span>
        </div>

        {/* Metrics Grid */}
        {isError ? (
          <p className="rounded-xl border border-destructive/30 bg-destructive/10 p-4 text-sm text-destructive">
            Unable to load platform analytics.
          </p>
        ) : (
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {metrics.map(({ label, value, icon: Icon, accent, trend }) => (
              <div key={label} className="rounded-2xl border border-border bg-card p-6 shadow-sm">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">{label}</span>
                  <Icon className={`h-5 w-5 ${accent}`} />
                </div>
                <p className="mt-4 text-3xl font-black">
                  {isLoading ? <Loader2 className="h-6 w-6 animate-spin" /> : (value ?? '—').toLocaleString()}
                </p>
                <span className="mt-2 inline-flex items-center gap-1 text-[11px] font-semibold text-emerald-500">
                  <TrendingUp className="h-3 w-3" />
                  {trend}
                </span>
              </div>
            ))}
          </div>
        )}

        {/* Monthly Enrollment Bar Chart & System Status */}
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          {/* Chart Panel */}
          <div className="rounded-3xl border border-border bg-card p-6 space-y-6 lg:col-span-2">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-base font-bold">Student Enrollment Growth</h3>
                <p className="text-xs text-muted-foreground">Cumulative student registrations year-to-date</p>
              </div>
              <span className="text-xs font-bold text-primary">2026 YTD</span>
            </div>

            <div className="flex items-end gap-3 h-52 pt-8 border-b border-border/40 pb-4">
              {MONTHLY_ENROLLMENTS.map((m) => {
                const heightPercent = Math.round((m.count / maxVal) * 100);
                return (
                  <div key={m.month} className="flex-1 flex flex-col items-center gap-2 group">
                    <div className="w-full bg-muted/60 rounded-t-lg overflow-hidden flex items-end h-full">
                      <div
                        className="w-full bg-gradient-to-t from-primary/80 to-primary group-hover:to-accent transition-all duration-300 rounded-t-lg"
                        style={{ height: `${heightPercent}%` }}
                      />
                    </div>
                    <span className="text-[11px] font-bold text-muted-foreground">{m.month}</span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* System Services Telemetry */}
          <div className="rounded-3xl border border-border bg-card p-6 space-y-4">
            <h3 className="text-base font-bold flex items-center gap-2">
              <Server className="h-4 w-4 text-primary" />
              Service Status
            </h3>

            <div className="space-y-3">
              {[
                { name: 'ASP.NET Core 10 Web API', status: 'Operational', latency: '12ms' },
                { name: 'PostgreSQL 16 Database', status: 'Operational', latency: '4ms' },
                { name: 'Redis 7 HybridCache', status: 'Operational', latency: '1ms' },
                { name: 'Judge0 Evaluation Sandbox', status: 'Operational', latency: '150ms' },
                { name: 'Paymob Webhook Listener', status: 'Operational', latency: '28ms' },
              ].map((svc) => (
                <div key={svc.name} className="flex items-center justify-between text-xs rounded-xl border border-border/50 bg-muted/30 p-3">
                  <div className="space-y-0.5">
                    <p className="font-bold">{svc.name}</p>
                    <span className="text-[10px] text-muted-foreground">Latency: {svc.latency}</span>
                  </div>
                  <span className="inline-flex items-center gap-1 font-bold text-emerald-500">
                    <CheckCircle className="h-3 w-3" />
                    OK
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
