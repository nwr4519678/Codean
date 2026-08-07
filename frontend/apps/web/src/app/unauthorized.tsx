import React from 'react';
import Link from 'next/link';
import { Lock, ArrowLeft } from 'lucide-react';

export default function UnauthorizedPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background p-4 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-amber-500/10 text-amber-500 mb-6">
        <Lock className="h-8 w-8" />
      </div>
      <h1 className="text-5xl font-black text-amber-500">401</h1>
      <h2 className="mt-2 text-2xl font-bold tracking-tight">Authentication Required</h2>
      <p className="mt-2 text-sm text-muted-foreground max-w-md">
        You need to be signed in to access this page. Please log in with your account credentials.
      </p>
      <Link href="/auth/login" className="mt-8 inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg hover:bg-primary/90 transition-all active:scale-95">
        Sign In
      </Link>
      <Link href="/" className="mt-3 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="h-4 w-4" /> Back to Home
      </Link>
    </div>
  );
}
