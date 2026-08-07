import Link from 'next/link';
import { Compass } from 'lucide-react';

export default function NotFoundPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background p-4 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary/10 text-primary mb-6">
        <Compass className="h-8 w-8 animate-spin-slow" />
      </div>

      <h1 className="text-6xl font-black text-primary">404</h1>
      <h2 className="mt-2 text-2xl font-bold tracking-tight">Page Not Found</h2>
      <p className="mt-2 text-muted-foreground max-w-md text-sm">
        The page you are looking for doesn&apos;t exist or has been moved.
      </p>

      <Link
        href="/"
        className="mt-8 inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-primary/25 hover:bg-primary/90 transition-all active:scale-95"
      >
        Back to Home
      </Link>
    </div>
  );
}
