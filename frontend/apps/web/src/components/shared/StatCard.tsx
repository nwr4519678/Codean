import React from 'react';
import { LucideIcon } from 'lucide-react';

interface StatCardProps {
  title: string;
  value: string | number;
  subtext?: string;
  icon: LucideIcon;
  colorClass?: string;
}

export function StatCard({ title, value, subtext, icon: Icon, colorClass = 'text-primary' }: StatCardProps) {
  return (
    <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm transition-all hover:border-border hover:shadow-md">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-muted-foreground">{title}</span>
        <div className={`flex h-10 w-10 items-center justify-center rounded-xl bg-muted/60 ${colorClass}`}>
          <Icon className="h-5 w-5" />
        </div>
      </div>
      <p className="mt-4 text-3xl font-extrabold tracking-tight">{value}</p>
      {subtext && <p className="mt-1 text-xs text-muted-foreground">{subtext}</p>}
    </div>
  );
}
