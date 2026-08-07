import React from 'react';

export function DashboardSkeleton() {
  return (
    <div className="space-y-8 animate-pulse">
      <div className="h-10 w-64 rounded-xl bg-muted" />
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-32 rounded-2xl border border-border/60 bg-card p-6" />
        ))}
      </div>
      <div className="h-64 rounded-2xl border border-border/60 bg-card p-6" />
    </div>
  );
}
