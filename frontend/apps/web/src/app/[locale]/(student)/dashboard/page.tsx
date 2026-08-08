'use client';

import React from 'react';
import { BookOpen, Code2, Clock, PlayCircle, Zap, ArrowRight, Loader2, AlertCircle } from 'lucide-react';
import { useCurrentUser } from '@platform/api';
import { useEnrolledCourses } from '@platform/api';
import { useCourses } from '@platform/api';
import Link from 'next/link';

export default function StudentDashboard() {
  const { data: user, isLoading: userLoading } = useCurrentUser();
  const { data: enrolledData, isLoading: enrolledLoading, isError: enrolledError } = useEnrolledCourses();
  const { data: browseCourses, isLoading: browseLoading } = useCourses({ isPublished: true, pageSize: 6 });

  const enrolled = enrolledData ?? [];
  const inProgress = enrolled.filter((e: any) => e.completionPercentage > 0 && e.completionPercentage < 100);
  const completed = enrolled.filter((e: any) => e.completionPercentage === 100);
  const firstName = userLoading ? '...' : (user?.firstName ?? 'Student');

  return (
    <div className="container mx-auto space-y-8 px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="flex flex-col gap-6 rounded-3xl border border-border/80 bg-gradient-to-r from-card via-card to-primary/5 p-6 shadow-sm lg:flex-row lg:items-center lg:justify-between">
        <div className="space-y-1">
          <div className="inline-flex items-center gap-2 rounded-full border border-primary/20 bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">
            <Zap className="h-3.5 w-3.5" /> Student Dashboard
          </div>
          <h1 className="text-3xl font-extrabold tracking-tight">
            Welcome back, {firstName}! 👋
          </h1>
          <p className="text-sm text-muted-foreground">Continue where you left off and keep learning.</p>
        </div>
        <Link
          href="/courses"
          className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-semibold text-white shadow-lg hover:bg-primary/90 transition-colors"
        >
          Browse Courses <ArrowRight className="h-4 w-4" />
        </Link>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Enrolled</span>
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10">
              <BookOpen className="h-5 w-5 text-primary" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{enrolledLoading ? '—' : enrolled.length}</p>
          <span className="mt-1 text-xs text-muted-foreground">{inProgress.length} in progress</span>
        </div>

        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">In Progress</span>
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent/10">
              <PlayCircle className="h-5 w-5 text-accent" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{enrolledLoading ? '—' : inProgress.length}</p>
          <span className="mt-1 text-xs text-muted-foreground">Active learning</span>
        </div>

        <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Completed</span>
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-500/10">
              <Code2 className="h-5 w-5 text-emerald-500" />
            </div>
          </div>
          <p className="mt-4 text-3xl font-black">{enrolledLoading ? '—' : completed.length}</p>
          <span className="mt-1 text-xs text-muted-foreground">Courses finished</span>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-2">
        {/* My Courses */}
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold flex items-center gap-2">
              <PlayCircle className="h-5 w-5 text-primary" />
              My Courses
            </h2>
            <Link href="/my-courses" className="text-xs font-bold text-primary hover:underline flex items-center gap-1">
              View All <ArrowRight className="h-3 w-3" />
            </Link>
          </div>

          {enrolledLoading ? (
            <div className="flex items-center justify-center py-12 rounded-2xl border border-border bg-card">
              <Loader2 className="h-7 w-7 animate-spin text-primary" />
            </div>
          ) : enrolledError ? (
            <div className="flex flex-col items-center justify-center py-12 rounded-2xl border border-border bg-card gap-2 text-destructive">
              <AlertCircle className="h-5 w-5" />
              <p className="text-sm">Failed to load your courses</p>
            </div>
          ) : enrolled.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 rounded-2xl border-2 border-dashed border-border bg-card gap-3 text-muted-foreground">
              <BookOpen className="h-8 w-8 opacity-40" />
              <p className="text-sm">You haven't enrolled in any courses yet.</p>
              <Link href="/courses" className="rounded-xl bg-primary px-4 py-2 text-xs font-semibold text-white hover:bg-primary/90">
                Browse Courses
              </Link>
            </div>
          ) : (
            <div className="space-y-3">
              {enrolled.slice(0, 5).map((e: any) => (
                <Link
                  key={e.courseId ?? e.id}
                  href={`/student/learn/${e.courseId ?? e.id}`}
                  className="flex items-center gap-4 rounded-2xl border border-border bg-card p-4 hover:bg-muted/40 transition-colors"
                >
                  <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary font-black text-lg">
                    {(e.courseTitle ?? e.title ?? '?')[0]}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold text-sm line-clamp-1">{e.courseTitle ?? e.title}</p>
                    <div className="mt-1.5 flex items-center gap-2">
                      <div className="h-1.5 flex-1 rounded-full bg-muted overflow-hidden">
                        <div
                          className="h-full bg-primary transition-all"
                          style={{ width: `${e.completionPercentage ?? 0}%` }}
                        />
                      </div>
                      <span className="text-[10px] font-bold text-muted-foreground">{e.completionPercentage ?? 0}%</span>
                    </div>
                  </div>
                  <ArrowRight className="h-4 w-4 text-muted-foreground shrink-0" />
                </Link>
              ))}
            </div>
          )}
        </div>

        {/* Browse Available Courses */}
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl font-bold flex items-center gap-2">
              <BookOpen className="h-5 w-5 text-accent" />
              Available Courses
            </h2>
            <Link href="/courses" className="text-xs font-bold text-primary hover:underline flex items-center gap-1">
              Browse All <ArrowRight className="h-3 w-3" />
            </Link>
          </div>

          {browseLoading ? (
            <div className="flex items-center justify-center py-12 rounded-2xl border border-border bg-card">
              <Loader2 className="h-7 w-7 animate-spin text-primary" />
            </div>
          ) : (browseCourses?.items?.length ?? 0) === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 rounded-2xl border-2 border-dashed border-border bg-card gap-3 text-muted-foreground">
              <BookOpen className="h-8 w-8 opacity-40" />
              <p className="text-sm">No courses published yet.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {browseCourses!.items.slice(0, 5).map((c: any) => (
                <Link
                  key={c.id}
                  href={`/courses/${c.id}`}
                  className="flex items-center gap-4 rounded-2xl border border-border bg-card p-4 hover:bg-muted/40 transition-colors"
                >
                  <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-accent/10 text-accent font-black text-lg">
                    {c.title[0]}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold text-sm line-clamp-1">{c.title}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">{c.category}</p>
                  </div>
                  <span className="text-xs font-bold text-primary shrink-0">
                    {c.price === 0 ? 'Free' : `$${c.price}`}
                  </span>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
