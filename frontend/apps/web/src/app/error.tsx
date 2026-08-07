'use client';

import React, { useEffect } from 'react';
import { AlertTriangle, RotateCcw } from 'lucide-react';

export default function ErrorPage({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error('Unhandled runtime error:', error);
  }, [error]);

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background p-4 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive mb-6 shadow-inner">
        <AlertTriangle className="h-8 w-8" />
      </div>

      <h1 className="text-3xl font-extrabold tracking-tight">Something went wrong!</h1>
      <p className="mt-2 text-muted-foreground max-w-md text-sm">
        An unexpected error occurred. Our telemetry team has been notified.
      </p>

      {error.digest && (
        <code className="mt-4 rounded-lg bg-muted px-3 py-1 text-xs text-muted-foreground font-mono">
          Error Ref: {error.digest}
        </code>
      )}

      <button
        onClick={() => reset()}
        className="mt-8 inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95"
      >
        <RotateCcw className="h-4 w-4" />
        Try Again
      </button>
    </div>
  );
}
