import React from 'react';
import Link from 'next/link';
import { ShieldOff, ArrowLeft } from 'lucide-react';

export default function ForbiddenPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background p-4 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-destructive/10 text-destructive mb-6">
        <ShieldOff className="h-8 w-8" />
      </div>
      <h1 className="text-5xl font-black text-destructive">403</h1>
      <h2 className="mt-2 text-2xl font-bold tracking-tight">Access Forbidden</h2>
      <p className="mt-2 text-sm text-muted-foreground max-w-md">
        You don&apos;t have permission to access this resource. Please contact your administrator if you believe this is an error.
      </p>
      <Link href="/" className="mt-8 inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg hover:bg-primary/90 transition-all active:scale-95">
        <ArrowLeft className="h-4 w-4" /> Back to Home
      </Link>
    </div>
  );
}
