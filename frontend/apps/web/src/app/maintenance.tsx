import React from 'react';
import { Wrench } from 'lucide-react';

export default function MaintenancePage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background p-4 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/10 text-primary mb-6">
        <Wrench className="h-8 w-8 animate-bounce" />
      </div>
      <h1 className="text-3xl font-extrabold tracking-tight">We&apos;ll be back soon!</h1>
      <p className="mt-3 text-sm text-muted-foreground max-w-md">
        The platform is currently undergoing scheduled maintenance to improve your experience.
        This usually takes less than 30 minutes.
      </p>
      <div className="mt-8 flex items-center gap-2 rounded-2xl border border-border/60 bg-card px-5 py-3 text-xs text-muted-foreground">
        <span className="h-2 w-2 rounded-full bg-amber-500 animate-pulse" />
        Estimated completion: 30 minutes
      </div>
    </div>
  );
}
