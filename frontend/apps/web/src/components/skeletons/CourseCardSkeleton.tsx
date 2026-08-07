import React from 'react';

export function CourseCardSkeleton() {
  return (
    <div className="flex flex-col rounded-2xl border border-border/60 bg-card p-5 animate-pulse space-y-4">
      <div className="h-44 w-full rounded-xl bg-muted" />
      <div className="h-4 w-1/3 rounded bg-muted" />
      <div className="h-6 w-3/4 rounded bg-muted" />
      <div className="h-4 w-full rounded bg-muted" />
      <div className="flex justify-between items-center pt-2">
        <div className="h-5 w-16 rounded bg-muted" />
        <div className="h-8 w-24 rounded-xl bg-muted" />
      </div>
    </div>
  );
}
