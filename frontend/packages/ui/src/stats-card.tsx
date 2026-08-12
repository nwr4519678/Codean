"use client";

import * as React from "react";
import { TrendingUp, TrendingDown } from "lucide-react";
import { cn } from "./utils";
import { Card } from "./card";

export interface StatsCardProps {
  title: string;
  value: string | number;
  change?: string;
  isPositive?: boolean;
  icon?: React.ReactNode;
  description?: string;
  className?: string;
}

export function StatsCard({
  title,
  value,
  change,
  isPositive = true,
  icon,
  description,
  className,
}: StatsCardProps) {
  return (
    <Card className={cn("p-5 border-border/80 bg-card hover:border-primary/40 transition-all shadow-sm hover:shadow-md", className)}>
      <div className="flex items-center justify-between">
        <span className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">{title}</span>
        {icon && <div className="p-2.5 rounded-xl bg-primary/10 text-primary">{icon}</div>}
      </div>

      <div className="mt-3 flex items-baseline justify-between gap-2">
        <h4 className="text-2xl font-bold tracking-tight text-foreground">{value}</h4>

        {change && (
          <span
            className={cn(
              "inline-flex items-center gap-0.5 text-xs font-semibold px-2 py-0.5 rounded-full border",
              isPositive
                ? "bg-emerald-500/10 text-emerald-500 border-emerald-500/20"
                : "bg-rose-500/10 text-rose-500 border-rose-500/20"
            )}
          >
            {isPositive ? <TrendingUp className="w-3 h-3 me-0.5" /> : <TrendingDown className="w-3 h-3 me-0.5" />}
            {change}
          </span>
        )}
      </div>

      {description && <p className="mt-1 text-xs text-muted-foreground">{description}</p>}
    </Card>
  );
}
