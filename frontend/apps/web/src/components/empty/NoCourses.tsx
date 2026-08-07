import React from 'react';
import { BookOpen } from 'lucide-react';
import Link from 'next/link';

export function NoCourses() {
  return (
    <div className="flex min-h-[300px] w-full flex-col items-center justify-center rounded-2xl border border-dashed border-border/80 bg-card/40 p-8 text-center">
      <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10 text-primary mb-4">
        <BookOpen className="h-7 w-7" />
      </div>
      <h3 className="text-lg font-bold">No Courses Enrolled</h3>
      <p className="mt-1 max-w-sm text-xs text-muted-foreground">
        You haven&apos;t enrolled in any courses yet. Explore our course catalog to start learning.
      </p>
      <Link
        href="/courses"
        className="mt-6 rounded-xl bg-primary px-5 py-2.5 text-xs font-semibold text-white shadow-md hover:bg-primary/90"
      >
        Browse Courses
      </Link>
    </div>
  );
}
