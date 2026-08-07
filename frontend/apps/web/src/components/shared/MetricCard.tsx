import React from 'react';
import { TrendingUp, TrendingDown } from 'lucide-react';

interface MetricCardProps {
  label: string;
  value: string | number;
  changePercentage?: number;
  trendText?: string;
}

export function MetricCard({ label, value, changePercentage, trendText }: MetricCardProps) {
  const isPositive = changePercentage !== undefined && changePercentage >= 0;

  return (
    <div className="rounded-2xl border border-border/70 bg-card p-6 shadow-sm">
      <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">{label}</span>
      <div className="mt-2 flex items-baseline justify-between">
        <span className="text-3xl font-extrabold">{value}</span>
        {changePercentage !== undefined && (
          <span
            className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-semibold ${
              isPositive ? 'bg-emerald-500/10 text-emerald-500' : 'bg-destructive/10 text-destructive'
            }`}
          >
            {isPositive ? <TrendingUp className="h-3 w-3" /> : <TrendingDown className="h-3 w-3" />}
            {isPositive ? `+${changePercentage}%` : `${changePercentage}%`}
          </span>
        )}
      </div>
      {trendText && <p className="mt-2 text-xs text-muted-foreground">{trendText}</p>}
    </div>
  );
}
